using System.Text.RegularExpressions;
using CanSayNo.Application.Common.Results;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;
using CanSayNo.Domain.Models;
using FluentValidation;
using MediatR;

namespace CanSayNo.Application.Features.Admin.Hayirci.Scenarios;

// ───────────── Ekle ─────────────

public record CreateAdminScenarioCommand(string Id, AdminScenarioUpsertDto Body) : IRequest<Result<AdminScenarioDto>>;

public class CreateAdminScenarioCommandValidator : AbstractValidator<CreateAdminScenarioCommand>
{
    // Örn. "romantik_011_yeni_konu": bağlam öneki + 3 haneli sıra + kısa konu.
    private static readonly Regex IdPattern = new("^(is|aile|arkadas|romantik|satici|komsu|okul|dijital)_[0-9]{3}_[a-z0-9_]{2,60}$");

    public CreateAdminScenarioCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .Must(id => IdPattern.IsMatch(id ?? string.Empty))
            .WithMessage("ID biçimi: <bağlam>_<3 hane>_<konu> (örn. romantik_011_yeni_konu), yalnızca küçük harf, rakam ve _.");
        RuleFor(x => x)
            .Must(x => x.Id.StartsWith(AdminScenarioIds.Prefix(x.Body.ContextId) + "_", StringComparison.Ordinal))
            .WithMessage("ID öneki senaryonun bağlamıyla aynı olmalı.")
            .When(x => x.Body != null && !string.IsNullOrEmpty(x.Id));
        RuleFor(x => x.Body).NotNull().SetValidator(new AdminScenarioUpsertValidator());
    }
}

public class CreateAdminScenarioCommandHandler : IRequestHandler<CreateAdminScenarioCommand, Result<AdminScenarioDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateAdminScenarioCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<AdminScenarioDto>> Handle(CreateAdminScenarioCommand request, CancellationToken cancellationToken)
    {
        // Pasif/silinmiş dahil herhangi bir kayıt ID'yi tutuyorsa yeniden kullanılmaz:
        // geçmiş oturumlar bu ID'ye bağlı.
        if (await _unitOfWork.Scenarios.ExistsAsync(request.Id))
            return Result<AdminScenarioDto>.Failure(
                Error.Failure(ErrorCode.AlreadyExists, "Bu ID ile bir senaryo zaten var."));

        var scenario = new Scenario { Id = request.Id };
        request.Body.ApplyTo(scenario);

        await _unitOfWork.Scenarios.AddAsync(scenario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AdminScenarioDto>.Success(AdminScenarioDto.From(scenario, AdminScenarioUsageDto.Empty));
    }
}

// ───────────── Düzenle ─────────────

public record UpdateAdminScenarioCommand(string Id, AdminScenarioUpsertDto Body) : IRequest<Result<AdminScenarioDto>>;

public class UpdateAdminScenarioCommandValidator : AbstractValidator<UpdateAdminScenarioCommand>
{
    public UpdateAdminScenarioCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Body).NotNull().SetValidator(new AdminScenarioUpsertValidator());
    }
}

public class UpdateAdminScenarioCommandHandler : IRequestHandler<UpdateAdminScenarioCommand, Result<AdminScenarioDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAdminScenarioCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<AdminScenarioDto>> Handle(UpdateAdminScenarioCommand request, CancellationToken cancellationToken)
    {
        var scenario = await _unitOfWork.Scenarios.GetByIdAsync(request.Id);
        if (scenario is null)
            return Result<AdminScenarioDto>.Failure(Error.Failure(ErrorCode.NotFound, "Senaryo bulunamadı."));

        // Bağlam değişirse ID öneki tutarsızlaşır ve geçmiş oturumların bağlam istatistiği
        // (rozetler) kayar — bağlam değiştirmek yerine yeni senaryo açılmalı.
        if (scenario.ContextId != request.Body.ContextId)
            return Result<AdminScenarioDto>.Failure(Error.Failure(
                ErrorCode.InvalidRequest, "Senaryonun bağlamı değiştirilemez; yeni bir senaryo oluştur."));

        request.Body.ApplyTo(scenario);
        _unitOfWork.Scenarios.Update(scenario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AdminScenarioDto>.Success(AdminScenarioDto.From(scenario, AdminScenarioUsageDto.Empty));
    }
}

// ───────────── Aktif / pasif ─────────────

public record SetAdminScenarioActiveCommand(string Id, bool IsActive) : IRequest<Result<AdminScenarioDto>>;

public class SetAdminScenarioActiveCommandHandler : IRequestHandler<SetAdminScenarioActiveCommand, Result<AdminScenarioDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SetAdminScenarioActiveCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<AdminScenarioDto>> Handle(SetAdminScenarioActiveCommand request, CancellationToken cancellationToken)
    {
        var scenario = await _unitOfWork.Scenarios.GetByIdAsync(request.Id);
        if (scenario is null)
            return Result<AdminScenarioDto>.Failure(Error.Failure(ErrorCode.NotFound, "Senaryo bulunamadı."));

        // Pasif senaryo mobilde listelenmez ve yeni oturum açılamaz; geçmiş oturumlar ve
        // sonuçları korunur (silme yok).
        scenario.IsActive = request.IsActive;
        _unitOfWork.Scenarios.Update(scenario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AdminScenarioDto>.Success(AdminScenarioDto.From(scenario, AdminScenarioUsageDto.Empty));
    }
}

public static class AdminScenarioIds
{
    public static string Prefix(ContextId context) => context switch
    {
        ContextId.Is => "is",
        ContextId.Aile => "aile",
        ContextId.Arkadas => "arkadas",
        ContextId.Romantik => "romantik",
        ContextId.Satici => "satici",
        ContextId.Komsu => "komsu",
        ContextId.Okul => "okul",
        ContextId.Dijital => "dijital",
        _ => "?"
    };
}
