using FluentValidation;
using Kickstart.Application.Common.Localization;

namespace Kickstart.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator(IJsonStringLocalizer localizer)
        {
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

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.password"]])
                .MinimumLength(6).WithMessage(localizer["validation.minLength", localizer["validation.fields.password"], 6])
                .MaximumLength(100).WithMessage(localizer["validation.maxLength", localizer["validation.fields.password"], 100])
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
                .WithMessage(localizer["validation.passwordComplexity"]);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage(localizer["validation.maxLength", localizer["validation.fields.phoneNumber"], 20])
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage(localizer["validation.invalidEnum", localizer["validation.fields.status"]]);
        }
    }
}
