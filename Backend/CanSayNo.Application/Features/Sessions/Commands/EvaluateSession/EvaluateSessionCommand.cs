using System;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Features.Sessions.Common;
using MediatR;

namespace CanSayNo.Application.Features.Sessions.Commands.EvaluateSession;

/// <summary>
/// Tamamlanmış pratiği 5 eksende değerlendirir, Session.Evaluation'a yazar ve
/// UserStats'ı (seri, XP, seviye, rozetler) günceller. İdempotent: değerlendirme
/// zaten varsa LLM çağrılmadan mevcut sonuç döner.
/// </summary>
public record EvaluateSessionCommand(Guid SessionId) : IRequest<Result<SessionDto>>;
