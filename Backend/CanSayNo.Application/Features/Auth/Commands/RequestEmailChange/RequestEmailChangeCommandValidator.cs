using FluentValidation;
using CanSayNo.Application.Common.Localization;

namespace CanSayNo.Application.Features.Auth.Commands.RequestEmailChange
{
    public class RequestEmailChangeCommandValidator : AbstractValidator<RequestEmailChangeCommand>
    {
        public RequestEmailChangeCommandValidator(IJsonStringLocalizer localizer)
        {
            RuleFor(x => x.NewEmail)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.email"]])
                .EmailAddress().WithMessage(localizer["validation.emailFormat"])
                .MaximumLength(100).WithMessage(localizer["validation.maxLength", localizer["validation.fields.email"], 100]);

            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.password"]]);
        }
    }
}
