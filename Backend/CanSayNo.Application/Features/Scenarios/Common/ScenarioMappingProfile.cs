using AutoMapper;
using CanSayNo.Domain.Entities.Hayirci;

namespace CanSayNo.Application.Features.Scenarios.Common;

/// <summary>
/// Scenario entity -> DTO eşlemeleri. User-spesifik alanlar (IsCompleted, BestScore,
/// AttemptsCount, LastAttemptAt) burada eşlenmez; bunlar Sessions agregasından handler
/// içinde manuel doldurulur. PressureTactics hiçbir DTO'ya taşınmaz.
/// </summary>
public class ScenarioMappingProfile : Profile
{
    public ScenarioMappingProfile()
    {
        CreateMap<Scenario, ScenarioListItemDto>()
            .ForMember(d => d.IsCompleted, opt => opt.Ignore())
            .ForMember(d => d.BestScore, opt => opt.Ignore())
            .ForMember(d => d.AttemptsCount, opt => opt.Ignore());

        CreateMap<Scenario, ScenarioDetailDto>()
            .ForMember(d => d.IsCompleted, opt => opt.Ignore())
            .ForMember(d => d.BestScore, opt => opt.Ignore())
            .ForMember(d => d.AttemptsCount, opt => opt.Ignore())
            .ForMember(d => d.LastAttemptAt, opt => opt.Ignore());

        CreateMap<ScenarioScene, ScenarioSceneDto>();
        CreateMap<ScenarioCharacter, ScenarioCharacterDto>();
    }
}
