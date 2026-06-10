using FluentValidation;
using Kickstart.Application.Common.Localization;

namespace Kickstart.Application.Features.Auth.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator(IJsonStringLocalizer localizer)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.email"]])
                .EmailAddress().WithMessage(localizer["validation.emailFormat"]);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.password"]]);
        }
    }
}
