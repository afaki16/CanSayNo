using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Features.Sessions.Common;
using MediatR;

namespace CanSayNo.Application.Features.Sessions.Commands.StartSession;

/// <summary>
/// Bir senaryo için pratik oturumu başlatır. Aynı senaryoda henüz mesaj gönderilmemiş
/// açık bir oturum varsa yenisi açılmaz, o döner (günlük limit yanmaz).
/// </summary>
public record StartSessionCommand : IRequest<Result<SessionDto>>
{
    public string ScenarioId { get; init; } = "";
}
