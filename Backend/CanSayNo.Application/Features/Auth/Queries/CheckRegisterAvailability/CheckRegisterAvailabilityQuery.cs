using CanSayNo.Application.Common.Localization;
using CanSayNo.Application.Common.Results;
using CanSayNo.Domain.Common.Interfaces;
using MediatR;

namespace CanSayNo.Application.Features.Auth.Queries.CheckRegisterAvailability
{
    /// <summary>
    /// Live check on the public register form. Reveals no more than the register endpoint itself
    /// (which answers "email exists" on submit) and shares its rate limit, so it adds no extra
    /// enumeration capacity. Same uniqueness scope as public register: users without a tenant.
    /// </summary>
    public class CheckRegisterAvailabilityQuery : IRequest<Result<CheckAvailabilityDto>>
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class FieldAvailability
    {
        public bool IsAvailable { get; set; }
        public string? Reason { get; set; }
    }

    /// <summary>A field is null when it was not asked about.</summary>
    public class CheckAvailabilityDto
    {
        public FieldAvailability? Email { get; set; }
        public FieldAvailability? PhoneNumber { get; set; }
    }

    public class CheckRegisterAvailabilityQueryHandler : IRequestHandler<CheckRegisterAvailabilityQuery, Result<CheckAvailabilityDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJsonStringLocalizer _localizer;

        public CheckRegisterAvailabilityQueryHandler(IUnitOfWork unitOfWork, IJsonStringLocalizer localizer)
        {
            _unitOfWork = unitOfWork;
            _localizer = localizer;
        }

        public async Task<Result<CheckAvailabilityDto>> Handle(CheckRegisterAvailabilityQuery request, CancellationToken cancellationToken)
        {
            var dto = new CheckAvailabilityDto();

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var taken = await _unitOfWork.Users.EmailExistsAsync(request.Email.Trim().ToLowerInvariant());
                dto.Email = new FieldAvailability { IsAvailable = !taken, Reason = taken ? _localizer["users.emailExists"] : null };
            }

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                // Compared as entered, exactly like RegisterCommandHandler
                var taken = await _unitOfWork.Users.PhoneExistsAsync(request.PhoneNumber, tenantId: null);
                dto.PhoneNumber = new FieldAvailability { IsAvailable = !taken, Reason = taken ? _localizer["users.phoneExists"] : null };
            }

            return Result<CheckAvailabilityDto>.Success(dto);
        }
    }
}
