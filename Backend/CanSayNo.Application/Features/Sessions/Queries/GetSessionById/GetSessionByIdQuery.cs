using System;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Features.Sessions.Common;
using MediatR;

namespace CanSayNo.Application.Features.Sessions.Queries.GetSessionById;

public record GetSessionByIdQuery(Guid Id) : IRequest<Result<SessionDto>>;
