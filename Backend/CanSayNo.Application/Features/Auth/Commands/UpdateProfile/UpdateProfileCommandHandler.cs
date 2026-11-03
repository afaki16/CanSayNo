using CanSayNo.Application.Common.Localization;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CanSayNo.Application.Features.Auth.Commands.UpdateProfile
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateProfileCommandHandler> _logger;
        private readonly IJsonStringLocalizer _localizer;

        public UpdateProfileCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<UpdateProfileCommandHandler> logger,
            IJsonStringLocalizer localizer)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<Result> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                return Result.Failure(Error.Failure(ErrorCode.Unauthorized, _localizer["auth.userNotAuthenticated"]));

            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
            if (user is null)
                return Result.Failure(Error.Failure(ErrorCode.NotFound, _localizer["users.notFound"]));

            var phone = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
            if (phone != null && phone != user.PhoneNumber &&
                await _unitOfWork.Users.PhoneExistsAsync(phone, user.TenantId, excludeUserId: user.Id))
                return Result.Failure(Error.Failure(ErrorCode.AlreadyExists, _localizer["users.phoneExists"]));

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            if (phone != user.PhoneNumber)
            {
                user.PhoneNumber = phone;
                // A new number has not been verified yet
                user.PhoneConfirmed = false;
            }

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Profile updated by user. UserId: {UserId}", user.Id);
            return Result.Success();
        }
    }
}
