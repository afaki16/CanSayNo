using System.Linq;
using System.Text.Json;
using AutoMapper;
using CanSayNo.Domain.Entities.Hayirci;

namespace CanSayNo.Application.Features.Sessions.Common;

/// <summary>
/// Session entity -> DTO eşlemeleri. ScenarioTitle / ScenarioContextId, Session.Scenario
/// navigation'ından AutoMapper flattening ile gelir (Include edilmiş olmalı).
/// </summary>
public class SessionMappingProfile : Profile
{
    public SessionMappingProfile()
    {
        CreateMap<Session, SessionDto>()
            .ForMember(d => d.MaxRounds, opt => opt.MapFrom(s =>
                s.Scenario != null ? s.Scenario.PressureTactics.MaxRounds : 0));

        CreateMap<Session, SessionListItemDto>();

        CreateMap<SessionMessage, SessionMessageDto>();

        CreateMap<SessionEvaluation, SessionEvaluationDto>()
            .ForMember(d => d.Scores, opt => opt.MapFrom(e => e.Scores.ToDictionary(
                kv => JsonNamingPolicy.CamelCase.ConvertName(kv.Key.ToString()),
                kv => kv.Value)));
    }
}
