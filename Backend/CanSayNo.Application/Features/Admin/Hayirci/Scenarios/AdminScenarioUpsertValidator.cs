using FluentValidation;

namespace CanSayNo.Application.Features.Admin.Hayirci.Scenarios;

/// <summary>
/// Senaryo gövdesi kuralları. Sınırlar mobil arayüze ve LLM prompt'una göre seçildi:
/// metinler prompt'a girdiği için gereksiz uzunluk hem maliyet hem dikkat dağınıklığı demek.
/// </summary>
public class AdminScenarioUpsertValidator : AbstractValidator<AdminScenarioUpsertDto>
{
    public AdminScenarioUpsertValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(80);
        RuleFor(x => x.ShortDescription).NotEmpty().MaximumLength(160);
        RuleFor(x => x.ContextId).IsInEnum();
        RuleFor(x => x.Difficulty).InclusiveBetween(1, 5);
        RuleFor(x => x.EstimatedDurationMinutes).InclusiveBetween(1, 30);
        RuleFor(x => x.SortOrder).InclusiveBetween(0, 10_000);
        RuleFor(x => x.Tags).Must(t => t.Count <= 8).WithMessage("En fazla 8 etiket olabilir.");
        RuleForEach(x => x.Tags).NotEmpty().MaximumLength(30);

        RuleFor(x => x.Scene).NotNull();
        RuleFor(x => x.Scene.Setting).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Scene.Situation).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Scene.UserGoal).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Scene.Hint).MaximumLength(200);
        RuleFor(x => x.Scene.OpeningMessage).NotEmpty().MaximumLength(600);

        RuleFor(x => x.Character).NotNull();
        RuleFor(x => x.Character.Name).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Character.Age).InclusiveBetween(5, 100);
        RuleFor(x => x.Character.Role).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Character.Relationship).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Character.Archetype).IsInEnum();

        RuleFor(x => x.PressureTactics).NotNull();
        RuleFor(x => x.PressureTactics.RoundOne).Must(HasOneToThree).WithMessage("1. raunt için 1-3 taktik seçilmeli.");
        RuleFor(x => x.PressureTactics.RoundTwo).Must(HasOneToThree).WithMessage("2. raunt için 1-3 taktik seçilmeli.");
        RuleFor(x => x.PressureTactics.RoundThree).Must(HasOneToThree).WithMessage("3. raunt için 1-3 taktik seçilmeli.");
        RuleForEach(x => x.PressureTactics.RoundOne).IsInEnum();
        RuleForEach(x => x.PressureTactics.RoundTwo).IsInEnum();
        RuleForEach(x => x.PressureTactics.RoundThree).IsInEnum();
    }

    private static bool HasOneToThree<T>(List<T>? items) => items is { Count: >= 1 and <= 3 };
}
