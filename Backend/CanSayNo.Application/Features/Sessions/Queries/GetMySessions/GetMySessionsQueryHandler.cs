using System;
using System.Collections.Generic;
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

namespace CanSayNo.Application.Features.Sessions.Queries.GetMySessions;

public class GetMySessionsQueryHandler
    : IRequestHandler<GetMySessionsQuery, PagedResult<SessionListItemDto>>
{
    // Tek istekte çekilebilecek en fazla oturum — büyük pageSize ile tabloyu taramayı önler.
    private const int MaxPageSize = 50;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IJsonStringLocalizer _localizer;

    public GetMySessionsQueryHandler(
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

    public async Task<PagedResult<SessionListItemDto>> Handle(
        GetMySessionsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not int userId)
            return PagedResult<SessionListItemDto>.Failure(
                Error.Failure(ErrorCode.Unauthorized, _localizer["auth.userNotAuthenticated"]));

        // Validator yerine sıkıştırma: ValidationBehavior PagedResult<T> için failure üretemiyor.
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);

        var (sessions, totalCount) = await _unitOfWork.Sessions.GetUserSessionsPagedAsync(
            userId, page, pageSize, cancellationToken);

        var items = _mapper.Map<List<SessionListItemDto>>(sessions);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return PagedResult<SessionListItemDto>.Success(items, page, totalPages, totalCount);
    }
}
