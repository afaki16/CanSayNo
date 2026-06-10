using FluentValidation;
using Kickstart.Application.Common.Localization;

namespace Kickstart.Application.Features.Auth.Commands.RefreshToken
{
    /// <summary>
    /// Refresh token validator.
    ///
    /// GÜVENLİK NOTU:
    /// - Web client'lar refresh token'ı HttpOnly cookie ile gönderir (body boş).
    ///   Controller cookie'den okuyup command.RefreshToken'a yazar.
    /// - Mobile/CLI client'lar body ile gönderebilir (geriye dönük uyumluluk).
    /// - UserId artık refresh token kaydından çıkarılıyor; AccessToken artık ZORUNLU DEĞİL.
    /// </summary>
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator(IJsonStringLocalizer localizer)
        {
            // RefreshToken zorunlu — controller cookie'den veya body'den doldurmalı
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.refreshToken"]]);

            // AccessToken artık opsiyonel (yeni client'lar göndermiyor)
        }
    }
}
