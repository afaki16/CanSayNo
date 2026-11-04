using System;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Application.Common.Localization;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Common.Settings;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Enums;
using CanSayNo.Domain.Models;
using MediatR;

namespace CanSayNo.Application.Features.Subscriptions.Queries.GetMySubscription;

/// <summary>Mobil için premium durumu. Erişim kararı backend'de (HasActiveSubscriptionAsync) ile aynı kurala dayanır.</summary>
public record GetMySubscriptionQuery : IRequest<Result<MySubscriptionDto>>;

public record MySubscriptionDto
{
    public bool IsPremium { get; init; }
    public string? ProductId { get; init; }
    /// <summary>active | cancelled (yenilenmeyecek, süre bitene kadar geçerli) | gracePeriod | expired | pending; kayıt yoksa null.</summary>
    public SubscriptionStatus? Status { get; init; }
    public DateTime? ExpiresAt { get; init; }
    /// <summary>Otomatik yenileme açık mı (yalnızca premium iken anlamlı).</summary>
    public bool WillRenew { get; init; }
}

public class GetMySubscriptionQueryHandler : IRequestHandler<GetMySubscriptionQuery, Result<MySubscriptionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IJsonStringLocalizer _localizer;
    private readonly HayirciSettings _settings;

    public GetMySubscriptionQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IJsonStringLocalizer localizer,
        HayirciSettings settings)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _localizer = localizer;
        _settings = settings;
    }

    public async Task<Result<MySubscriptionDto>> Handle(GetMySubscriptionQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not int userId)
            return Result<MySubscriptionDto>.Failure(
                Error.Failure(ErrorCode.Unauthorized, _localizer["auth.userNotAuthenticated"]));

        var utcNow = DateTime.UtcNow;
        var isPremium = await _unitOfWork.Subscriptions.HasActiveSubscriptionAsync(
            userId, _settings.AppId, utcNow, cancellationToken);
        var current = await _unitOfWork.Subscriptions.GetCurrentAsync(
            userId, _settings.AppId, utcNow, cancellationToken);

        return Result<MySubscriptionDto>.Success(new MySubscriptionDto
        {
            IsPremium = isPremium,
            ProductId = current?.ProductId,
            Status = current?.Status,
            ExpiresAt = current?.ExpiresAt,
            WillRenew = isPremium && current?.Status is SubscriptionStatus.Active or SubscriptionStatus.GracePeriod
        });
    }
}
