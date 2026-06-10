using FluentValidation;
using Kickstart.Application.Common.Localization;

namespace Kickstart.Application.Features.Tenants.Commands.UpdateTenant;

public class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator(IJsonStringLocalizer localizer)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage(localizer["validation.required", localizer["validation.fields.tenantId"]]);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.tenantName"]])
            .MaximumLength(100).WithMessage(localizer["validation.maxLength", localizer["validation.fields.tenantName"], 100]);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(localizer["validation.maxLength", localizer["validation.fields.description"], 500]);

        RuleFor(x => x.Domain)
            .MaximumLength(100).WithMessage(localizer["validation.maxLength", localizer["validation.fields.domain"], 100])
            .Matches(@"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*$")
            .When(x => !string.IsNullOrEmpty(x.Domain))
            .WithMessage(localizer["validation.domainFormat"]);

        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage(localizer["validation.emailFormat"])
            .MaximumLength(100).WithMessage(localizer["validation.maxLength", localizer["validation.fields.contactEmail"], 100])
            .When(x => !string.IsNullOrEmpty(x.ContactEmail));

        RuleFor(x => x.ContactPhone)
            .MaximumLength(20).WithMessage(localizer["validation.maxLength", localizer["validation.fields.contactPhone"], 20]);
    }
}
