using FluentValidation;
using Kickstart.Application.Common.Localization;

namespace Kickstart.Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator(IJsonStringLocalizer localizer)
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.userId"]]);

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.firstName"]])
                .MaximumLength(50).WithMessage(localizer["validation.maxLength", localizer["validation.fields.firstName"], 50]);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.lastName"]])
                .MaximumLength(50).WithMessage(localizer["validation.maxLength", localizer["validation.fields.lastName"], 50]);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.email"]])
                .EmailAddress().WithMessage(localizer["validation.emailFormat"])
                .MaximumLength(100).WithMessage(localizer["validation.maxLength", localizer["validation.fields.email"], 100]);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage(localizer["validation.maxLength", localizer["validation.fields.phoneNumber"], 20])
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage(localizer["validation.invalidEnum", localizer["validation.fields.status"]]);
        }
    }
}
