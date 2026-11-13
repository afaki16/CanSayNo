using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Application.Common.Settings;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Entities.Hayirci;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CanSayNo.Application.Features.Subscriptions.Webhooks;

/// <summary>
/// RevenueCat webhook'unu işler. Ham gövde saklanabilsin diye string olarak gelir.
/// Kimlik doğrulama burada (sabit zamanlı karşılaştırma) — controller yalnızca HTTP koduna çevirir.
/// </summary>
public record ProcessRevenueCatWebhookCommand(string? AuthorizationHeader, string RawBody)
    : IRequest<WebhookResult>;

public enum WebhookResult
{
    /// <summary>İşlendi ya da bilerek yok sayıldı — RevenueCat tekrar denemesin (200).</summary>
    Ok,
    Unauthorized,
    BadRequest,
    /// <summary>Webhook gizli değeri ayarlanmamış — uç kapalı (503).</summary>
    NotConfigured
}

public class ProcessRevenueCatWebhookCommandHandler : IRequestHandler<ProcessRevenueCatWebhookCommand, WebhookResult>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IUnitOfWork _unitOfWork;
    private readonly RevenueCatSettings _revenueCat;
    private readonly HayirciSettings _hayirci;
    private readonly ILogger<ProcessRevenueCatWebhookCommandHandler> _logger;

    public ProcessRevenueCatWebhookCommandHandler(
        IUnitOfWork unitOfWork,
        RevenueCatSettings revenueCat,
        HayirciSettings hayirci,
        ILogger<ProcessRevenueCatWebhookCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _revenueCat = revenueCat;
        _hayirci = hayirci;
        _logger = logger;
    }

    public async Task<WebhookResult> Handle(ProcessRevenueCatWebhookCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_revenueCat.WebhookAuthorization))
        {
            _logger.LogError("RevenueCat webhook received but RevenueCat:WebhookAuthorization is not configured");
            return WebhookResult.NotConfigured;
        }

        if (!IsAuthorized(request.AuthorizationHeader))
        {
            _logger.LogWarning("RevenueCat webhook rejected: invalid Authorization header");
            return WebhookResult.Unauthorized;
        }

        RevenueCatEvent? e;
        try
        {
            e = JsonSerializer.Deserialize<RevenueCatWebhookPayload>(request.RawBody, JsonOptions)?.Event;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "RevenueCat webhook body is not valid JSON");
            return WebhookResult.BadRequest;
        }

        if (e is null || string.IsNullOrEmpty(e.Type) || string.IsNullOrEmpty(e.Id))
            return WebhookResult.BadRequest;

        if (e.Type == "TEST")
        {
            _logger.LogInformation("RevenueCat TEST webhook received. EventId: {EventId}", e.Id);
            return WebhookResult.Ok;
        }

        if (string.Equals(e.Environment, "SANDBOX", StringComparison.OrdinalIgnoreCase) && !_revenueCat.AcceptSandboxEvents)
        {
            _logger.LogInformation("RevenueCat SANDBOX event ignored. EventId: {EventId}, Type: {Type}", e.Id, e.Type);
            return WebhookResult.Ok;
        }

        if (e.Type == "TRANSFER")
            return await ApplyTransferAsync(e, cancellationToken);

        var transactionKey = e.OriginalTransactionId ?? e.TransactionId;
        if (string.IsNullOrEmpty(transactionKey))
        {
            _logger.LogInformation("RevenueCat event without transaction id ignored. EventId: {EventId}, Type: {Type}", e.Id, e.Type);
            return WebhookResult.Ok;
        }

        // Kaydı olan işlem, kaydın sahibine uygulanır: TRANSFER'dan sonra olayın original_app_user_id'si
        // hâlâ eski hesabı gösterebilir — kullanıcıyı olaydan çözmek aboneliği eski hesaba geri verirdi.
        var subscription = await _unitOfWork.Subscriptions.GetByProviderTransactionAsync(
            _hayirci.AppId, transactionKey, cancellationToken);

        var userId = subscription?.UserId ?? await ResolveUserIdAsync(e);
        if (userId is null)
        {
            // Anonim RevenueCat kullanıcısı ya da silinmiş hesap: tekrar denemenin faydası yok.
            _logger.LogWarning("RevenueCat event for unknown user ignored. EventId: {EventId}, Type: {Type}", e.Id, e.Type);
            return WebhookResult.Ok;
        }

        // Tekrar gelen ya da sırası karışmış (daha eski) olay durumu geri almasın.
        // Yeniden denemeler aynı id ve event_timestamp_ms ile gelir.
        if (subscription is not null && !IsNewerThanStored(subscription, e))
        {
            _logger.LogInformation("RevenueCat event skipped (duplicate or older). EventId: {EventId}, Type: {Type}", e.Id, e.Type);
            return WebhookResult.Ok;
        }

        var isNew = subscription is null;
        subscription ??= new Subscription
        {
            UserId = userId.Value,
            AppId = _hayirci.AppId,
            Provider = "RevenueCat",
            ProviderTransactionId = transactionKey,
            ProductId = e.ProductId ?? string.Empty,
            Status = Domain.Enums.SubscriptionStatus.Pending
        };

        var outcome = RevenueCatEventApplier.Apply(subscription, e, DateTime.UtcNow);
        if (outcome == RevenueCatEventApplier.Outcome.NoStateChange && isNew)
        {
            // Kaydı olmayan bir işlem için durum bilgisi taşımayan olay: kayıt açma.
            _logger.LogInformation("RevenueCat {Type} for unknown transaction ignored. EventId: {EventId}", e.Type, e.Id);
            return WebhookResult.Ok;
        }

        subscription.ProviderUserId = e.AppUserId ?? subscription.ProviderUserId;
        subscription.RawWebhookData = request.RawBody;

        if (isNew)
            await _unitOfWork.Subscriptions.AddAsync(subscription);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "RevenueCat event applied. EventId: {EventId}, Type: {Type}, UserId: {UserId}, Status: {Status}, ExpiresAt: {ExpiresAt}",
            e.Id, e.Type, userId, subscription.Status, subscription.ExpiresAt);
        return WebhookResult.Ok;
    }

    // TRANSFER: kullanıcı başka hesapta "Satın alımları geri yükle" dedi; RevenueCat işlemleri
    // transferred_from hesaplarından transferred_to hesabına taşıdı. Kayıtları da taşırız — yoksa
    // geri yükleyen hesap premium olmaz, eski hesap premium kalır. Tekrar gelen olay zararsız
    // (taşınacak kayıt kalmaz). Ham gövde yazılmaz: işlem olaylarının sıra kontrolü onu kullanıyor.
    private async Task<WebhookResult> ApplyTransferAsync(RevenueCatEvent e, CancellationToken cancellationToken)
    {
        var toUserId = await FirstExistingUserIdAsync(e.TransferredTo);
        if (toUserId is null)
        {
            _logger.LogWarning("RevenueCat TRANSFER to unknown user ignored. EventId: {EventId}", e.Id);
            return WebhookResult.Ok;
        }

        var fromUserIds = (e.TransferredFrom ?? new List<string>())
            .Select(id => int.TryParse(id, out var parsed) ? parsed : (int?)null)
            .Where(id => id is not null && id != toUserId)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var moved = fromUserIds.Count == 0
            ? new List<Subscription>()
            : await _unitOfWork.Subscriptions.GetAllForUsersAsync(fromUserIds, _hayirci.AppId, cancellationToken);

        foreach (var subscription in moved)
        {
            subscription.UserId = toUserId.Value;
            subscription.ProviderUserId = toUserId.Value.ToString();
        }

        if (moved.Count > 0)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "RevenueCat TRANSFER applied. EventId: {EventId}, ToUserId: {ToUserId}, FromUserIds: {FromUserIds}, Moved: {Moved}",
            e.Id, toUserId, fromUserIds, moved.Count);
        return WebhookResult.Ok;
    }

    private async Task<int?> FirstExistingUserIdAsync(IEnumerable<string?>? candidates)
    {
        if (candidates is null)
            return null;

        foreach (var id in candidates.Where(c => !string.IsNullOrEmpty(c)).Distinct())
        {
            if (int.TryParse(id, out var userId) && await _unitOfWork.Users.GetByIdAsync(userId) is not null)
                return userId;
        }
        return null;
    }

    private bool IsAuthorized(string? header)
    {
        if (string.IsNullOrEmpty(header))
            return false;

        // Panelde girilen değer birebir gönderilir; "Bearer " önekiyle girilmiş olma ihtimaline
        // karşı ikisi de kabul edilir. Karşılaştırma sabit zamanlı.
        var expected = _revenueCat.WebhookAuthorization.Trim();
        var actual = header.Trim();
        return FixedTimeEquals(actual, expected) || FixedTimeEquals(actual, "Bearer " + expected);
    }

    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));

    // Mobil, RevenueCat'e bizim kullanıcı Id'mizi App User ID olarak verir. Belgenin önerdiği gibi
    // original_app_user_id, app_user_id ve aliases taranır; var olan ilk sayısal Id kullanılır.
    private Task<int?> ResolveUserIdAsync(RevenueCatEvent e)
    {
        var candidates = new List<string?> { e.OriginalAppUserId, e.AppUserId };
        if (e.Aliases is not null)
            candidates.AddRange(e.Aliases);
        return FirstExistingUserIdAsync(candidates);
    }

    private static bool IsNewerThanStored(Subscription subscription, RevenueCatEvent incoming)
    {
        if (string.IsNullOrEmpty(subscription.RawWebhookData))
            return true;
        try
        {
            var stored = JsonSerializer.Deserialize<RevenueCatWebhookPayload>(subscription.RawWebhookData, JsonOptions)?.Event;
            if (stored is null)
                return true;
            if (stored.Id == incoming.Id)
                return false;
            return (incoming.EventTimestampMs ?? 0) > (stored.EventTimestampMs ?? 0);
        }
        catch (JsonException)
        {
            return true;
        }
    }
}
