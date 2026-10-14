using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CanSayNo.Application.Common.Localization;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Features.Sessions.Common;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Models;
using MediatR;

namespace CanSayNo.Application.Features.Sessions.Queries.GetSessionById;

public class GetSessionByIdQueryHandler : IRequestHandler<GetSessionByIdQuery, Result<SessionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IJsonStringLocalizer _localizer;

    public GetSessionByIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IJsonStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _localizer = localizer;
    }

    public async Task<Result<SessionDto>> Handle(
        GetSessionByIdQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not int userId)
            return Result<SessionDto>.Failure(
                Error.Failure(ErrorCode.Unauthorized, _localizer["auth.userNotAuthenticated"]));

        // Başka kullanıcının oturumu da NotFound döner — varlığı sızdırılmaz.
        var session = await _unitOfWork.Sessions.GetUserSessionAsync(
            request.Id, userId, asNoTracking: true, cancellationToken);

        if (session is null)
            return Result<SessionDto>.Failure(
                Error.Failure(ErrorCode.NotFound, _localizer["sessions.notFound"]));

        return Result<SessionDto>.Success(_mapper.Map<SessionDto>(session));
    }
}
