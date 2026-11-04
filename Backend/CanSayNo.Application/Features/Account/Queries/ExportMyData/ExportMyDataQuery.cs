using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Application.Common.Localization;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Common.Interfaces.Repositories;
using CanSayNo.Domain.Enums;
using CanSayNo.Domain.Models;
using MediatR;

namespace CanSayNo.Application.Features.Account.Queries.ExportMyData;

/// <summary>
/// Kullanıcının uygulamadaki tüm verisi, makinece okunabilir JSON olarak (KVKK md. 11 erişim hakkı).
/// Şifre hash'i, token'lar ve ödeme sağlayıcısının ham webhook verisi dahil edilmez.
/// </summary>
public record ExportMyDataQuery : IRequest<Result<AccountExportDto>>;

public record AccountExportDto
{
    public DateTime ExportedAt { get; init; }
    public ExportProfileDto Profile { get; init; } = new();
    public ExportStatsDto? Stats { get; init; }
    public List<ExportSessionDto> Sessions { get; init; } = new();
    public List<ExportDailyUsageDto> DailyUsage { get; init; } = new();
    public List<ExportSubscriptionDto> Subscriptions { get; init; } = new();
}

public record ExportProfileDto
{
    public string FirstName { get; init; } = "";
    public string LastName { get; init; } = "";
    public string Email { get; init; } = "";
    public bool EmailConfirmed { get; init; }
    public string? PhoneNumber { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? LastLoginDate { get; init; }
}

public record ExportStatsDto
{
    public int CurrentStreak { get; init; }
    public int LongestStreak { get; init; }
    public DateOnly? LastPracticeDate { get; init; }
    public int TotalSessions { get; init; }
    public int TotalCompletedSessions { get; init; }
    public int TotalPoints { get; init; }
    public int Level { get; init; }
    public List<Badge> UnlockedBadges { get; init; } = new();
}

public record ExportSessionDto
{
    public Guid Id { get; init; }
    public string ScenarioId { get; init; } = "";
    public string ScenarioTitle { get; init; } = "";
    public SessionStatus Status { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public int? TotalScore { get; init; }
    public List<ExportMessageDto> Messages { get; init; } = new();
    public ExportEvaluationDto? Evaluation { get; init; }
}

public record ExportMessageDto(string Role, string Content, DateTime Timestamp);

public record ExportEvaluationDto
{
    public Dictionary<string, int> Scores { get; init; } = new();
    public List<string> Strengths { get; init; } = new();
    public List<string> Improvements { get; init; } = new();
    public string CoachNote { get; init; } = "";
    public string? IdealResponseExample { get; init; }
    public int TotalScore { get; init; }
}

public record ExportDailyUsageDto(DateOnly Date, int SessionsCount);

public record ExportSubscriptionDto
{
    public string ProductId { get; init; } = "";
    public SubscriptionStatus Status { get; init; }
    public string Provider { get; init; } = "";
    public DateTime? StartedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public DateTime? CancelledAt { get; init; }
}

public class ExportMyDataQueryHandler : IRequestHandler<ExportMyDataQuery, Result<AccountExportDto>>
{
    private readonly IAccountDataRepository _accountData;
    private readonly ICurrentUserService _currentUserService;
    private readonly IJsonStringLocalizer _localizer;

    public ExportMyDataQueryHandler(
        IAccountDataRepository accountData,
        ICurrentUserService currentUserService,
        IJsonStringLocalizer localizer)
    {
        _accountData = accountData;
        _currentUserService = currentUserService;
        _localizer = localizer;
    }

    public async Task<Result<AccountExportDto>> Handle(ExportMyDataQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not int userId)
            return Result<AccountExportDto>.Failure(
                Error.Failure(ErrorCode.Unauthorized, _localizer["auth.userNotAuthenticated"]));

        var data = await _accountData.GetSnapshotAsync(userId, cancellationToken);
        if (data is null)
            return Result<AccountExportDto>.Failure(Error.Failure(ErrorCode.NotFound, _localizer["users.notFound"]));

        var u = data.User;
        return Result<AccountExportDto>.Success(new AccountExportDto
        {
            ExportedAt = DateTime.UtcNow,
            Profile = new ExportProfileDto
            {
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                EmailConfirmed = u.EmailConfirmed,
                PhoneNumber = u.PhoneNumber,
                CreatedDate = u.CreatedDate,
                LastLoginDate = u.LastLoginDate
            },
            Stats = data.Stats is null ? null : new ExportStatsDto
            {
                CurrentStreak = data.Stats.CurrentStreak,
                LongestStreak = data.Stats.LongestStreak,
                LastPracticeDate = data.Stats.LastPracticeDate,
                TotalSessions = data.Stats.TotalSessions,
                TotalCompletedSessions = data.Stats.TotalCompletedSessions,
                TotalPoints = data.Stats.TotalPoints,
                Level = data.Stats.Level,
                UnlockedBadges = data.Stats.UnlockedBadges.ToList()
            },
            Sessions = data.Sessions.Select(s => new ExportSessionDto
            {
                Id = s.Id,
                ScenarioId = s.ScenarioId,
                ScenarioTitle = s.Scenario?.Title ?? "",
                Status = s.Status,
                StartedAt = s.StartedAt,
                CompletedAt = s.CompletedAt,
                TotalScore = s.TotalScore,
                Messages = s.Messages.Select(m => new ExportMessageDto(m.Role, m.Content, m.Timestamp)).ToList(),
                Evaluation = s.Evaluation is null ? null : new ExportEvaluationDto
                {
                    Scores = s.Evaluation.Scores.ToDictionary(
                        kv => JsonNamingPolicy.CamelCase.ConvertName(kv.Key.ToString()), kv => kv.Value),
                    Strengths = s.Evaluation.Strengths,
                    Improvements = s.Evaluation.Improvements,
                    CoachNote = s.Evaluation.CoachNote,
                    IdealResponseExample = s.Evaluation.IdealResponseExample,
                    TotalScore = s.Evaluation.TotalScore
                }
            }).ToList(),
            DailyUsage = data.DailyUsage.Select(d => new ExportDailyUsageDto(d.Date, d.SessionsCount)).ToList(),
            Subscriptions = data.Subscriptions.Select(s => new ExportSubscriptionDto
            {
                ProductId = s.ProductId,
                Status = s.Status,
                Provider = s.Provider,
                StartedAt = s.StartedAt,
                ExpiresAt = s.ExpiresAt,
                CancelledAt = s.CancelledAt
            }).ToList()
        });
    }
}
