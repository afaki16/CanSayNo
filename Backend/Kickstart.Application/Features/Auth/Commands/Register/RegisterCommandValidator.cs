using FluentValidation;
using Kickstart.Application.Common.Localization;

namespace Kickstart.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator(IJsonStringLocalizer localizer)
        {
            // Allow Unicode letters (Türkçe ç, ğ, ı, ö, ş, ü dahil), spaces, apostrophes, hyphens, dots.
            // Explicitly rejects HTML characters (<, >, &, ", '), URLs (://) and any digit/punctuation
            // that doesn't belong in a real name. Examples allowed: "Ahmet", "Mary-Jane", "O'Brien", "Dr. Smith".
            const string NameRegex = @"^[\p{L}\p{M}\s.'\-]+$";

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.firstName"]])
                .MaximumLength(50).WithMessage(localizer["validation.maxLength", localizer["validation.fields.firstName"], 50])
                .Matches(NameRegex).WithMessage(localizer["validation.nameFormat", localizer["validation.fields.firstName"]]);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.lastName"]])
                .MaximumLength(50).WithMessage(localizer["validation.maxLength", localizer["validation.fields.lastName"], 50])
                .Matches(NameRegex).WithMessage(localizer["validation.nameFormat", localizer["validation.fields.lastName"]]);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.email"]])
                .EmailAddress().WithMessage(localizer["validation.emailFormat"])
                .MaximumLength(100).WithMessage(localizer["validation.maxLength", localizer["validation.fields.email"], 100]);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.password"]])
                .MaximumLength(100).WithMessage(localizer["validation.maxLength", localizer["validation.fields.password"], 100]);

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.confirmPassword"]])
                .Equal(x => x.Password).WithMessage(localizer["validation.passwordsDoNotMatch"]);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage(localizer["validation.maxLength", localizer["validation.fields.phoneNumber"], 20])
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
        }
    }
}
