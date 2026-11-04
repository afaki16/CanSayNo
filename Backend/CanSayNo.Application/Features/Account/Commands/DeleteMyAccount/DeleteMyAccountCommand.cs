using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Application.Common.Localization;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Constants;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Common.Interfaces.Repositories;
using CanSayNo.Domain.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CanSayNo.Application.Features.Account.Commands.DeleteMyAccount;

/// <summary>
/// Kullanıcının kendi hesabını tüm verisiyle KALICI olarak siler (App Store 5.1.1(v), KVKK).
/// Mevcut şifre istenir: çalınmış bir oturum tek başına hesabı silemesin.
/// Mağaza abonelikleri burada iptal edilmez — kullanıcı App Store / Google Play'den iptal eder.
/// </summary>
public record DeleteMyAccountCommand : IRequest<Result>
{
    public string Password { get; init; } = "";
}

public class DeleteMyAccountCommandValidator : AbstractValidator<DeleteMyAccountCommand>
{
    public DeleteMyAccountCommandValidator(IJsonStringLocalizer localizer)
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.currentPassword"]]);
    }
}

public class DeleteMyAccountCommandHandler : IRequestHandler<DeleteMyAccountCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountDataRepository _accountData;
    private readonly IPasswordService _passwordService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IJsonStringLocalizer _localizer;
    private readonly ILogger<DeleteMyAccountCommandHandler> _logger;

    public DeleteMyAccountCommandHandler(
        IUnitOfWork unitOfWork,
        IAccountDataRepository accountData,
        IPasswordService passwordService,
        ICurrentUserService currentUserService,
        IJsonStringLocalizer localizer,
        ILogger<DeleteMyAccountCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _accountData = accountData;
        _passwordService = passwordService;
        _currentUserService = currentUserService;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteMyAccountCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not int userId)
            return Result.Failure(Error.Failure(ErrorCode.Unauthorized, _localizer["auth.userNotAuthenticated"]));

        var user = await _unitOfWork.Users.GetUserWithRolesReadOnlyAsync(userId);
        if (user is null)
            return Result.Failure(Error.Failure(ErrorCode.NotFound, _localizer["users.notFound"]));

        var passwordCheck = _passwordService.VerifyPassword(request.Password, user.PasswordHash);
        if (!passwordCheck.IsSuccess || !passwordCheck.Value)
        {
            _logger.LogWarning("Account deletion rejected: wrong password. UserId: {UserId}", userId);
            return Result.Failure(Error.Failure(ErrorCode.ValidationFailed, _localizer["auth.currentPasswordIncorrect"]));
        }

        var isSuperAdmin = user.UserRoles?.Any(ur => ur.Role?.Name == RoleNames.SuperAdmin) == true;
        if (isSuperAdmin && await _unitOfWork.Users.CountActiveSuperAdminsAsync() <= 1)
            return Result.Failure(Error.Failure(ErrorCode.Forbidden, _localizer["users.cannotDeleteLastSuperAdmin"]));

        await _accountData.HardDeleteUserAsync(userId, user.Email, cancellationToken);

        _logger.LogWarning("Account permanently deleted by its owner. UserId: {UserId}", userId);
        return Result.Success();
    }
}
