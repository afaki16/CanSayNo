using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CanSayNo.Application.Common.Localization;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Common.Settings;
using CanSayNo.Application.Features.Sessions.Common;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;
using CanSayNo.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CanSayNo.Application.Features.Sessions.Commands.SendMessage;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<SessionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IJsonStringLocalizer _localizer;
    private readonly HayirciSettings _settings;
    private readonly ICharacterChatService _characterChat;
    private readonly ISessionLock _sessionLock;
    private readonly ILogger<SendMessageCommandHandler> _logger;

    public SendMessageCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IJsonStringLocalizer localizer,
        HayirciSettings settings,
        ICharacterChatService characterChat,
        ISessionLock sessionLock,
        ILogger<SendMessageCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _localizer = localizer;
        _settings = settings;
        _characterChat = characterChat;
        _sessionLock = sessionLock;
        _logger = logger;
    }

    public async Task<Result<SessionDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not int userId)
            return Fail(ErrorCode.Unauthorized, "auth.userNotAuthenticated");

        // Oturum yüklenmeden ÖNCE kilitlenir: aynı rauntta ikinci istek beklemez, LLM'e de gitmez.
        // (Bekleseydi bir sonraki raunda kayardı; kullanıcı aynı mesajı iki kez göndermiş olurdu.)
        using var sessionLock = await _sessionLock.TryAcquireAsync(request.SessionId, TimeSpan.Zero, cancellationToken);
        if (sessionLock is null)
            return Fail(ErrorCode.SessionBusy, "sessions.busy");

        var session = await _unitOfWork.Sessions.GetUserSessionAsync(
            request.SessionId, userId, asNoTracking: false, cancellationToken);
        if (session is null)
            return Fail(ErrorCode.NotFound, "sessions.notFound");

        var maxRounds = Math.Max(1, session.Scenario.PressureTactics.MaxRounds);
        if (session.Status != SessionStatus.Active || session.CurrentRound >= maxRounds)
            return Fail(ErrorCode.SessionNotActive, "sessions.notActive");

        if (session.TokensUsed >= _settings.SessionTokenLimit)
            return Fail(ErrorCode.SessionTokenLimitReached, "sessions.tokenLimitReached");

        var content = request.Content.Trim();
        var round = session.CurrentRound + 1;
        var prompt = CharacterPromptBuilder.Build(session.Scenario, session.Messages, content, round);

        var reply = await _characterChat.GetReplyAsync(prompt, cancellationToken);
        session.TokensUsed += reply.TokensUsed;

        if (reply.Outcome != AiCallOutcome.Success)
        {
            // Raunt ilerlemez, mesaj yazılmaz; kullanıcı aynı mesajı (ya da farklısını) tekrar gönderebilir.
            // Harcanan token yine de sayılır.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Character reply failed. SessionId: {SessionId}, Round: {Round}, Outcome: {Outcome}",
                session.Id, round, reply.Outcome);

            return reply.Outcome == AiCallOutcome.Refused
                ? Fail(ErrorCode.AiResponseRefused, "sessions.aiRefused")
                : Fail(ErrorCode.AiUnavailable, "sessions.aiUnavailable");
        }

        var utcNow = DateTime.UtcNow;

        // JSONB listesi: yeni örnek atamak değişikliğin izlenmesini garanti eder.
        session.Messages = new List<SessionMessage>(session.Messages)
        {
            new() { Role = SessionMessageRoles.User, Content = content, Timestamp = utcNow },
            new()
            {
                Role = SessionMessageRoles.Ai,
                Content = reply.Text,
                Parts = reply.Parts?.ToList(),
                Mood = reply.Mood,
                Timestamp = DateTime.UtcNow
            }
        };
        session.CurrentRound = round;

        if (round >= maxRounds)
        {
            session.Status = SessionStatus.Completed;
            session.CompletedAt = utcNow;
            session.DurationSeconds = (int)Math.Max(0, (utcNow - session.StartedAt).TotalSeconds);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SessionDto>.Success(_mapper.Map<SessionDto>(session));
    }

    private Result<SessionDto> Fail(ErrorCode code, string messageKey) =>
        Result<SessionDto>.Failure(Error.Failure(code, _localizer[messageKey]));
}
