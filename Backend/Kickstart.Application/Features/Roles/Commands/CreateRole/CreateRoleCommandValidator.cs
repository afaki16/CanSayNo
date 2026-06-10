using FluentValidation;
using Kickstart.Application.Common.Localization;

namespace Kickstart.Application.Features.Roles.Commands.CreateRole
{
    public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleCommandValidator(IJsonStringLocalizer localizer)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.roleName"]])
                .MaximumLength(50).WithMessage(localizer["validation.maxLength", localizer["validation.fields.roleName"], 50]);

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage(localizer["validation.maxLength", localizer["validation.fields.description"], 500])
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
}
