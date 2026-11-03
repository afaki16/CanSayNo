using FluentValidation;
using CanSayNo.Application.Common.Localization;

namespace CanSayNo.Application.Features.Auth.Commands.UpdateProfile
{
    public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
    {
        // Same limits as CreateUserCommandValidator
        public UpdateProfileCommandValidator(IJsonStringLocalizer localizer)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.firstName"]])
                .MaximumLength(50).WithMessage(localizer["validation.maxLength", localizer["validation.fields.firstName"], 50]);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.lastName"]])
                .MaximumLength(50).WithMessage(localizer["validation.maxLength", localizer["validation.fields.lastName"], 50]);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage(localizer["validation.maxLength", localizer["validation.fields.phoneNumber"], 20])
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
        }
    }
}
