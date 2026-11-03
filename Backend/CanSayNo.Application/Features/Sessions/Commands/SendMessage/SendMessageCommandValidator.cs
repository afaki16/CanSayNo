using CanSayNo.Application.Common.Localization;
using CanSayNo.Application.Common.Settings;
using FluentValidation;

namespace CanSayNo.Application.Features.Sessions.Commands.SendMessage;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator(IJsonStringLocalizer localizer, HayirciSettings settings)
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.id"]]);

        // Uzunluk kırpılmış metne göre: baştaki/sondaki boşluklar sınırı yemesin.
        RuleFor(x => x.Content)
            .Must(c => !string.IsNullOrWhiteSpace(c))
                .WithMessage(localizer["validation.required", localizer["validation.fields.message"]])
            .Must(c => (c ?? "").Trim().Length <= settings.MaxUserMessageLength)
                .WithMessage(localizer["validation.maxLength", localizer["validation.fields.message"], settings.MaxUserMessageLength]);
    }
}
