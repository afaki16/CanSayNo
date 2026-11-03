using System;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Features.Sessions.Common;
using MediatR;

namespace CanSayNo.Application.Features.Sessions.Commands.SendMessage;

/// <summary>
/// Kullanıcının pratik mesajını alır, karakterin (LLM) cevabını üretir ve ikisini oturuma yazar.
/// Her kullanıcı mesajı bir raunttur; son rauntta karakter kapanış yapar ve oturum tamamlanır.
/// </summary>
public record SendMessageCommand : IRequest<Result<SessionDto>>
{
    public Guid SessionId { get; init; }
    public string Content { get; init; } = "";
}

/// <summary>POST /api/Sessions/{id}/messages gövdesi.</summary>
public record SendMessageRequest
{
    public string Content { get; init; } = "";
}
