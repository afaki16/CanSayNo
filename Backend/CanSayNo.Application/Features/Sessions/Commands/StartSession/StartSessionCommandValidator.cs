using CanSayNo.Application.Common.Localization;
using FluentValidation;

namespace CanSayNo.Application.Features.Sessions.Commands.StartSession;

public class StartSessionCommandValidator : AbstractValidator<StartSessionCommand>
{
    public StartSessionCommandValidator(IJsonStringLocalizer localizer)
    {
        RuleFor(x => x.ScenarioId)
            .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.scenarioId"]])
            .MaximumLength(100).WithMessage(localizer["validation.maxLength", localizer["validation.fields.scenarioId"], 100]);
    }
}
