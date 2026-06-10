using CanSayNo.Application.Common.Results;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CanSayNo.Application.Features.Auth.Commands.RevokeSessionById
{
    public class RevokeSessionByIdCommandHandler : IRequestHandler<RevokeSessionByIdCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RevokeSessionByIdCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(RevokeSessionByIdCommand request, CancellationToken cancellationToken)
        {
            var token = await _unitOfWork.RefreshTokens.GetByIdAsync(request.SessionId);

            if (token == null)
                return Result.Failure(Error.Failure(ErrorCode.NotFound, "Session not found"));

            if (token.UserId != request.RequestingUserId)
                return Result.Failure(Error.Failure(ErrorCode.Forbidden, "Access denied"));

            if (!token.IsActive)
                return Result.Failure(Error.Failure(ErrorCode.InvalidRequest, "Session is already inactive"));

            token.Revoke(request.IpAddress, request.UserAgent, "Revoked by user");
            _unitOfWork.RefreshTokens.Update(token);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
