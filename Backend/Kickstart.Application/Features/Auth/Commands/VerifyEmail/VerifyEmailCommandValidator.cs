using FluentValidation;
using Kickstart.Application.Common.Localization;

namespace Kickstart.Application.Features.Auth.Commands.VerifyEmail
{
    public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
    {
        public VerifyEmailCommandValidator(IJsonStringLocalizer localizer)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.email"]])
                .EmailAddress().WithMessage(localizer["validation.emailFormat"]);

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.token"]]);
        }
    }
}
