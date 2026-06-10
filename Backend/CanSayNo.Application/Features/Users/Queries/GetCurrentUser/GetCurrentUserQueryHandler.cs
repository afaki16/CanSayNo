using AutoMapper;
using CanSayNo.Application.Features.Users.Dtos;
using CanSayNo.Application.Features.Users.Queries.GetCurrentUser;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Application.Common.Results;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Models;
using MediatR;

namespace CanSayNo.Application.Features.Users.Queries.GetCurrentUser
{
    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetCurrentUserQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetUserWithPermissionsAsync(request.UserId);

            if (user == null)
                return Result<UserDto>.Failure(Error.Failure(
                    ErrorCode.NotFound,
                    "User not found"));

            var userDto = _mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
    }
}
