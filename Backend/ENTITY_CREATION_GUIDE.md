# Kickstart Backend — Yeni Entity Oluşturma Rehberi

Bu rehber, projeye yeni bir entity (CRUD) eklerken **hangi dosyaları oluşturman**, **hangi mevcut dosyalara satır eklemen** gerektiğini sırasıyla gösterir. Örnekler **Tenant** ve **User** entity'lerinden alınmıştır; yeni entity'lerin de aynı pattern'i izlemesi gerekir.

Boyunca `Product` örnek entity adı olarak kullanılmıştır. Senin entity'nin adına göre `Product → SeninEntity`, `Products → SeninEntiteler` şeklinde değiştir.

---

## Mimari

```
Kickstart.Domain          → Entity, Repository Interface, Enum, Constant (dış bağımlılık yok)
Kickstart.Application     → CQRS Command/Query/Handler, DTO, Validator, AutoMapper (Domain'e bağımlı)
Kickstart.Infrastructure  → Repository implementasyonu, EF Configuration, DbContext (Application + Domain'e bağımlı)
Kickstart.API             → Controller, Middleware (Application'a bağımlı)
```

**Temel kurallar:**
- Tüm entity'ler `BaseAuditableEntity`'den türer (Id, CreatedDate, CreatedBy, UpdatedDate, UpdatedBy, IsDeleted otomatik gelir)
- Tüm handler'lar `IUnitOfWork` üzerinden repository'e erişir, repository direkt inject edilmez
- Silme her zaman **soft delete** (`IsDeleted = true`), `BaseEntity` türevlerine global query filter ile uygulanır
- Tüm command/query response'ları `Result<T>`, `Result<bool>` veya `PagedResult<T>` ile sarılır
- Validation `FluentValidation` ile yapılır; `ValidationBehavior` pipeline'ı otomatik tetikler
- Permission string'leri `Permissions.cs`'e eklenir; `SeedPermissionsAsync` reflection ile otomatik DB'ye seed eder

**Otomatik çalışan şeyler (manuel yapmana gerek yok):**

| Bileşen | Mekanizma |
|---------|-----------|
| Repository DI kaydı | `Infrastructure.DependencyInjection.AddRepositories()` — reflection; interface adı `Repository` ile bitmeli |
| MediatR Handler kaydı | `services.AddMediatR(assembly)` — otomatik |
| FluentValidation Validator kaydı | `services.AddValidatorsFromAssembly(assembly)` — otomatik |
| AutoMapper Profile kaydı | `services.AddAutoMapper(assembly)` — otomatik (sadece `MappingProfile.cs`'e mapping eklenir) |
| Permission Authorization Policy | `PermissionPolicyProvider` — `[Authorize(Policy = "...")]` içinde nokta varsa dinamik olarak policy üretir |
| Permission Seed | `SeedPermissionsAsync()` — `Permissions.cs`'i reflection ile okur |
| Soft Delete Query Filter | `ApplicationDbContext.OnModelCreating()` — `BaseEntity` türevlerine otomatik |
| Audit Fields (CreatedDate, CreatedBy, UpdatedDate, UpdatedBy) | `ApplicationDbContext.SaveChangesAsync()` override — otomatik |
| Validation pipeline | `ValidationBehavior<TRequest,TResponse>` — her command için validator varsa otomatik tetiklenir |

---

## ADIM 1 — Domain Katmanı

### 1.1 Entity Sınıfı (YENİ DOSYA)

**Dosya:** `Kickstart.Domain/Entities/Product.cs`

```csharp
using System.Collections.Generic;

namespace Kickstart.Domain.Entities;

public class Product : BaseAuditableEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }

    // Tenant desteği (entity tenant'a aitse — User, Tenant ilişkili entitylerde gerekir)
    public int? TenantId { get; set; }

    // ---- İlişki örnekleri ----

    // Many-to-One (FK kendi tarafında)
    public int CategoryId { get; set; }
    public Category Category { get; set; }

    // Tenant navigation (entity tenant'a bağlıysa)
    public Tenant? Tenant { get; set; }

    // One-to-Many (FK karşı tarafta)
    public ICollection<ProductImage> Images { get; set; }

    // Many-to-Many (ara tablo ile — User↔Role pattern'ı)
    public ICollection<ProductTag> ProductTags { get; set; }

    public Product()
    {
        Images = new HashSet<ProductImage>();
        ProductTags = new HashSet<ProductTag>();
        IsActive = true;
    }
}
```

**Base sınıf alanları (otomatik gelir):**

| Sınıf | Alanlar |
|-------|---------|
| `BaseEntity` | `Id (int)`, `CreatedDate`, `UpdatedDate?`, `IsDeleted` |
| `BaseAuditableEntity : BaseEntity` | + `CreatedBy (int?)`, `UpdatedBy (int?)` |

> `CreatedDate`/`CreatedBy`/`UpdatedDate`/`UpdatedBy` `ApplicationDbContext.SaveChangesAsync()` override'ında otomatik set edilir. **Manuel set etme.**

---

### 1.2 Repository Interface (YENİ DOSYA)

**Dosya:** `Kickstart.Domain/Common/Interfaces/Repositories/IProductRepository.cs`

```csharp
using Kickstart.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kickstart.Domain.Common.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product, int>
{
    // IRepository<T,TKey> şu metotları zaten sağlar (RepositoryBase üzerinden):
    //   GetByIdAsync, AddAsync, Update, Delete, SoftDelete, GetAllAsync,
    //   FindAsync, FindFirstAsync, ExistsAsync, GetQueryable, UpdateRange
    // Tekrar implement etme.

    // Entity'ye özel sorgu metotları:
    Task<bool> NameExistsAsync(string name, int? tenantId, int? excludeId = null);

    Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsPagedAsync(
        int page, int pageSize, string searchTerm = null, int? tenantId = null);

    Task<Product> GetProductWithRelationsAsync(int productId);

    // Read-only variant (Query handler'larda kullan — AsNoTracking ile %15-30 daha az bellek)
    Task<Product> GetProductWithRelationsReadOnlyAsync(int productId);

    // Many-to-Many için (User↔Role pattern'ı):
    Task AddProductTagAsync(ProductTag productTag);
    void RemoveProductTag(ProductTag productTag);
}
```

> **Kural:** Repository interface adı **`Repository` ile bitmelidir.** Aksi halde `AddRepositories()` reflection'ı tarafından kaydedilmez.

---

### 1.3 IUnitOfWork — Property Ekle (MEVCUT DOSYAYA SATIR EKLE)

**Dosya:** `Kickstart.Domain/Common/Interfaces/IUnitOfWork.cs`

Mevcut repository property'lerin yanına ekle:

```csharp
// Mevcut: IUserRepository Users { get; } ... vb.
IProductRepository Products { get; }   // ← BU SATIRI EKLE
```

---

### 1.4 Permissions Sabiti (MEVCUT DOSYAYA SATIR EKLE)

**Dosya:** `Kickstart.Domain/Constants/Permissions.cs`

Mevcut nested class'ların yanına yeni bir nested class ekle:

```csharp
// Mevcut: Users, Roles, Permission, Tenants ... vb.

public static class Products   // ← BU NESTED CLASS'I EKLE
{
    public const string Read   = "Products.Read";
    public const string Create = "Products.Create";
    public const string Update = "Products.Update";
    public const string Delete = "Products.Delete";
}
```

**KRİTİK — Permission isimlendirme kuralı:**

`Permissions.cs`'deki sabit string'ler **PascalCase**'dir: `"Products.Read"`.

Controller'larda `[Authorize(Policy = "...")]` parametresi **küçük harf** ile yazılır: `[Authorize(Policy = "products.read")]`.

Bu çelişki değildir — `PermissionService.HasPermissionAsync` karşılaştırmayı `StringComparer.OrdinalIgnoreCase` ile yapar. Yani:
- `Permissions.cs` → DB'ye `"Products.Read"` olarak seed edilir
- Controller `[Authorize(Policy = "products.read")]` → policy ismi büyük/küçük harf duyarsız eşleşir

> Bu projede mevcut tüm permission'lar (Users, Roles, Tenants, Permission) zaten bu pattern'i kullanır.

---

### 1.5 Enum (Gerekiyorsa — YENİ DOSYA)

Eğer entity'nde status, type gibi enum alanlar varsa:

**Dosya:** `Kickstart.Domain/Common/Enums/ProductStatus.cs`

```csharp
namespace Kickstart.Domain.Common.Enums;

public enum ProductStatus
{
    Draft     = 1,
    Active    = 2,
    Inactive  = 3,
    Archived  = 4
}
```

> Mevcut örnekler: `UserStatus`, `PermissionType`, `TokenType`, `VerificationChannel`, `VerificationPurpose`.

---

## ADIM 2 — Infrastructure Katmanı

### 2.1 EF Configuration (YENİ DOSYA)

**Dosya:** `Kickstart.Infrastructure/Persistence/EntityConfigurations/ProductConfiguration.cs`

```csharp
using Kickstart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickstart.Infrastructure.Persistence.EntityConfigurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedDate)
            .IsRequired();

        builder.Property(x => x.TenantId)
            .IsRequired(false);

        // Unique index (soft-delete'lenmiş kayıtları hariç tutar — silinen ismin tekrar
        // kullanılabilmesi için. Tenant'a göre scope'lar.)
        builder.HasIndex(x => new { x.TenantId, x.Name })
            .IsUnique()
            .HasFilter("\"TenantId\" IS NOT NULL AND NOT \"IsDeleted\"");

        builder.HasIndex(x => new { x.Name })
            .IsUnique()
            .HasFilter("\"TenantId\" IS NULL AND NOT \"IsDeleted\"");

        builder.HasIndex(x => x.TenantId);

        // ---- İlişki konfigürasyonları ----

        // Many-to-One (Product → Category)
        builder.HasOne(x => x.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tenant ilişkisi (entity tenant'a bağlıysa)
        builder.HasOne(x => x.Tenant)
            .WithMany()                  // Tenant tarafında collection yoksa
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.SetNull);

        // One-to-Many (Product → ProductImage)
        builder.HasMany(x => x.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Many-to-Many ara tablosu (kendi Configuration'ı ayrı dosyada yapılır)
    }
}
```

**Cascade silme politikası:**

| Senaryo | `OnDelete` |
|---------|-----------|
| Child parent olmadan anlamsız (ör. Order → OrderItem) | `Cascade` |
| Child başka parent'a atanabilir | `Restrict` |
| Parent silinince null olabilir (ör. Tenant → User) | `SetNull` |

> Referans: `TenantConfiguration.cs` (`Users` ile `SetNull`), `UserConfiguration.cs` (`UserRoles`, `RefreshTokens` ile `Cascade`).

---

### 2.2 Repository Implementasyonu (YENİ DOSYA)

**Dosya:** `Kickstart.Infrastructure/Repositories/ProductRepository.cs`

```csharp
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Domain.Entities;
using Kickstart.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kickstart.Infrastructure.Repositories;

public class ProductRepository : RepositoryBase<Product, int>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context) { }

    public async Task<bool> NameExistsAsync(string name, int? tenantId, int? excludeId = null)
    {
        var query = _context.Set<Product>()
            .Where(p => p.Name == name && p.TenantId == tenantId);

        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsPagedAsync(
        int page, int pageSize, string searchTerm = null, int? tenantId = null)
    {
        var query = _context.Set<Product>().AsQueryable();

        if (tenantId.HasValue)
            query = query.Where(p => p.TenantId == tenantId.Value);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                (p.Description != null && p.Description.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var products = await query
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products, totalCount);
    }

    public async Task<Product> GetProductWithRelationsAsync(int productId)
    {
        return await _context.Set<Product>()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == productId);
    }

    public async Task<Product> GetProductWithRelationsReadOnlyAsync(int productId)
    {
        return await _context.Set<Product>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == productId);
    }

    public async Task AddProductTagAsync(ProductTag productTag)
        => await _context.Set<ProductTag>().AddAsync(productTag);

    public void RemoveProductTag(ProductTag productTag)
        => _context.Set<ProductTag>().Remove(productTag);
}
```

**Önemli notlar:**
- `RepositoryBase<T,TKey>` sınıfından türet — base CRUD metotları (Add/Update/Delete/SoftDelete/GetById/GetAll/GetQueryable/Find/Exists) hazır gelir
- `_context` protected field olarak `RepositoryBase`'den miras alınır
- Query handler'lar için `ReadOnly` (AsNoTracking) varyantlar tercih edilir (memory/CPU avantajı)
- Modify edilecek entity'ler için tracking'li varyant kullan
- Pattern referansı: `TenantRepository.cs` (`GetTenantWithUsersAsync` ve `GetTenantWithUsersReadOnlyAsync`)

---

### 2.3 DbContext'e DbSet Ekle (MEVCUT DOSYAYA SATIR EKLE)

**Dosya:** `Kickstart.Infrastructure/Persistence/ApplicationDbContext.cs`

Mevcut `DbSet` property'lerinin yanına ekle:

```csharp
public DbSet<Product> Products { get; set; }   // ← BU SATIRI EKLE
```

Eğer ara tablon (Many-to-Many) varsa onu da ekle:

```csharp
public DbSet<ProductTag> ProductTags { get; set; }
```

> `OnModelCreating` zaten `ApplyConfigurationsFromAssembly` ile tüm `IEntityTypeConfiguration`'ları otomatik yükler — manuel yapma.

---

### 2.4 UnitOfWork'e Repository Kaydı (MEVCUT DOSYAYA 3 SATIR EKLE)

**Dosya:** `Kickstart.Infrastructure/Persistence/UnitOfWork.cs`

3 yere ekleme yapılır:

**A) Lazy field tanımı (constructor'ın üstündeki private field bloğuna):**

```csharp
private readonly Lazy<IProductRepository> _products;   // ← EKLE
```

**B) Constructor'da initialize (mevcut lazy initializasyonların yanına):**

```csharp
_products = new Lazy<IProductRepository>(
    () => _serviceProvider.GetRequiredService<IProductRepository>());   // ← EKLE
```

**C) Public property (mevcut property bloğunun yanına):**

```csharp
public IProductRepository Products => _products.Value;   // ← EKLE
```

> Repository sınıfı `Infrastructure.DependencyInjection.AddRepositories()` reflection'ı tarafından otomatik DI'ye kaydedilir — manuel `AddScoped` yazmana gerek yok. Sadece sınıf adının `Repository` ile bitmesi ve `IProductRepository` interface'ini implement etmesi yeterli.

---

### 2.5 Migration

```bash
# Backend/ klasöründen çalıştır (Kickstart.sln'in olduğu yer):
dotnet ef migrations add AddProductTable --project Kickstart.Infrastructure --startup-project Kickstart.API

# Uygulama (DB'ye uygulamak için):
dotnet ef database update --project Kickstart.Infrastructure --startup-project Kickstart.API
```

---

## ADIM 3 — Application Katmanı

### Klasör Yapısı

Yeni entity için tüm bu klasörü oluşturursun:

```
Kickstart.Application/Features/Products/
├── Commands/
│   ├── CreateProduct/
│   │   ├── CreateProductCommand.cs
│   │   ├── CreateProductCommandHandler.cs
│   │   └── CreateProductCommandValidator.cs
│   ├── UpdateProduct/
│   │   ├── UpdateProductCommand.cs
│   │   ├── UpdateProductCommandHandler.cs
│   │   └── UpdateProductCommandValidator.cs
│   └── DeleteProduct/
│       ├── DeleteProductCommand.cs
│       └── DeleteProductCommandHandler.cs
├── Queries/
│   ├── GetAllProducts/
│   │   ├── GetAllProductsQuery.cs
│   │   └── GetAllProductsQueryHandler.cs
│   └── GetProductById/
│       ├── GetProductByIdQuery.cs
│       └── GetProductByIdQueryHandler.cs
└── Dtos/
    ├── CreateProductDto.cs
    ├── UpdateProductDto.cs
    ├── ProductListDto.cs
    └── ProductDto.cs
```

---

### 3.1 DTO'lar (YENİ DOSYALAR)

**`Dtos/CreateProductDto.cs`** — Controller `[FromBody]` ile alır:

```csharp
namespace Kickstart.Application.Features.Products.Dtos;

public class CreateProductDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    /// <summary>Sadece SuperAdmin belirtebilir. Belirtilmezse mevcut kullanıcının tenant'ı kullanılır.</summary>
    public int? TenantId { get; set; }
}
```

**`Dtos/UpdateProductDto.cs`:**

```csharp
namespace Kickstart.Application.Features.Products.Dtos;

public class UpdateProductDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public int CategoryId { get; set; }
}
```

**`Dtos/ProductListDto.cs`** — Liste response'larında kullanılır, hafif:

```csharp
using System;

namespace Kickstart.Application.Features.Products.Dtos;

public class ProductListDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }     // Navigation'dan flatten
    public DateTime CreatedDate { get; set; }
    public int? TenantId { get; set; }
}
```

**`Dtos/ProductDto.cs`** — GetById response'unda kullanılır, ilişkili koleksiyonlar dahil:

```csharp
using System;
using System.Collections.Generic;

namespace Kickstart.Application.Features.Products.Dtos;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public int CategoryId { get; set; }
    public CategoryDto Category { get; set; }
    public List<ProductImageDto> Images { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int? UpdatedBy { get; set; }
    public int? TenantId { get; set; }
}
```

---

### 3.2 Create Command (YENİ 3 DOSYA)

**`Commands/CreateProduct/CreateProductCommand.cs`:**

```csharp
using Kickstart.Application.Common.Results;
using Kickstart.Application.Features.Products.Dtos;
using MediatR;

namespace Kickstart.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommand : IRequest<Result<ProductListDto>>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public int? TenantId { get; set; }
}
```

**`Commands/CreateProduct/CreateProductCommandHandler.cs`:**

```csharp
using AutoMapper;
using Kickstart.Application.Common.Localization;
using Kickstart.Application.Common.Results;
using Kickstart.Application.Features.Products.Dtos;
using Kickstart.Application.Interfaces;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Entities;
using Kickstart.Domain.Models;
using MediatR;

namespace Kickstart.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IJsonStringLocalizer _localizer;

    public CreateProductCommandHandler(
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

    public async Task<Result<ProductListDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // 1. Tenant belirleme (standart pattern — CreateUserCommandHandler'ı referans al)
        int? tenantId;
        if (request.TenantId.HasValue)
        {
            if (!_currentUserService.CanAccessAllTenants)
                return Result<ProductListDto>.Failure(Error.Failure(
                    ErrorCode.Forbidden,
                    _localizer["products.onlySuperAdminCanSpecifyTenant"]));
            tenantId = request.TenantId;
        }
        else
        {
            tenantId = _currentUserService.TenantId;
        }

        // 2. Unique kontrol
        if (await _unitOfWork.Products.NameExistsAsync(request.Name, tenantId))
            return Result<ProductListDto>.Failure(Error.Failure(
                ErrorCode.AlreadyExists,
                _localizer["products.nameExists"]));

        // 3. İlişkili entity varlık kontrolü
        var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId);
        if (category == null)
            return Result<ProductListDto>.Failure(Error.Failure(
                ErrorCode.NotFound,
                _localizer["categories.notFound"]));

        // 4. Entity oluştur
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            CategoryId = request.CategoryId,
            TenantId = tenantId,
            IsActive = true
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        // 5. (Opsiyonel) Many-to-Many ilişki — User↔Role pattern'ı:
        // if (request.TagIds?.Any() == true)
        // {
        //     foreach (var tagId in request.TagIds)
        //         await _unitOfWork.Products.AddProductTagAsync(
        //             new ProductTag { ProductId = product.Id, TagId = tagId });
        //     await _unitOfWork.SaveChangesAsync();
        // }

        // 6. İlişkili verilerle yeniden yükle ve döndür
        var productWithRelations = await _unitOfWork.Products.GetProductWithRelationsAsync(product.Id);
        return Result<ProductListDto>.Success(_mapper.Map<ProductListDto>(productWithRelations));
    }
}
```

**`Commands/CreateProduct/CreateProductCommandValidator.cs`:**

```csharp
using FluentValidation;
using Kickstart.Application.Common.Localization;

namespace Kickstart.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator(IJsonStringLocalizer localizer)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.productName"]])
            .MaximumLength(100).WithMessage(localizer["validation.maxLength", localizer["validation.fields.productName"], 100]);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(localizer["validation.maxLength", localizer["validation.fields.description"], 500])
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage(localizer["validation.greaterThan", localizer["validation.fields.price"], 0]);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage(localizer["validation.required", localizer["validation.fields.category"]]);
    }
}
```

> **Validator otomatik tetiklenir** — `ValidationBehavior<TRequest,TResponse>` MediatR pipeline'ında kayıtlıdır. Handler içinde manuel validate çağırma.

---

### 3.3 Update Command (YENİ 3 DOSYA)

**`Commands/UpdateProduct/UpdateProductCommand.cs`:**

```csharp
using Kickstart.Application.Common.Results;
using Kickstart.Application.Features.Products.Dtos;
using MediatR;

namespace Kickstart.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommand : IRequest<Result<ProductListDto>>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public int CategoryId { get; set; }
}
```

**`Commands/UpdateProduct/UpdateProductCommandHandler.cs`:**

```csharp
using AutoMapper;
using Kickstart.Application.Common.Localization;
using Kickstart.Application.Common.Results;
using Kickstart.Application.Features.Products.Dtos;
using Kickstart.Application.Interfaces;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Models;
using MediatR;

namespace Kickstart.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IJsonStringLocalizer _localizer;

    public UpdateProductCommandHandler(
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

    public async Task<Result<ProductListDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        // Tracking'li yükle — modify edilecek
        var product = await _unitOfWork.Products.GetProductWithRelationsAsync(request.Id);

        if (product == null)
            return Result<ProductListDto>.Failure(Error.Failure(
                ErrorCode.NotFound,
                _localizer["products.notFound"]));

        // Tenant izolasyon kontrolü — SuperAdmin değilse sadece kendi tenant'ı
        if (!_currentUserService.CanAccessAllTenants && product.TenantId != _currentUserService.TenantId)
            return Result<ProductListDto>.Failure(Error.Failure(
                ErrorCode.Forbidden,
                _localizer["products.noAccessToUpdate"]));

        // Unique kontrol (eğer isim değiştiyse)
        if (product.Name != request.Name &&
            await _unitOfWork.Products.NameExistsAsync(request.Name, product.TenantId, request.Id))
            return Result<ProductListDto>.Failure(Error.Failure(
                ErrorCode.AlreadyExists,
                _localizer["products.nameExists"]));

        // Alanları güncelle
        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.IsActive = request.IsActive;
        product.CategoryId = request.CategoryId;

        // Many-to-Many güncelleme (User→Role pattern'ı — UpdateUserCommandHandler referans):
        // var existingTags = product.ProductTags.ToList();
        // foreach (var tag in existingTags)
        //     _unitOfWork.Products.RemoveProductTag(tag);
        // foreach (var tagId in request.TagIds)
        //     await _unitOfWork.Products.AddProductTagAsync(
        //         new ProductTag { ProductId = product.Id, TagId = tagId });

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Products.GetProductWithRelationsAsync(product.Id);
        return Result<ProductListDto>.Success(_mapper.Map<ProductListDto>(updated));
    }
}
```

**`Commands/UpdateProduct/UpdateProductCommandValidator.cs`:**

```csharp
using FluentValidation;
using Kickstart.Application.Common.Localization;

namespace Kickstart.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator(IJsonStringLocalizer localizer)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage(localizer["validation.required", localizer["validation.fields.productId"]]);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.productName"]])
            .MaximumLength(100).WithMessage(localizer["validation.maxLength", localizer["validation.fields.productName"], 100]);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage(localizer["validation.greaterThan", localizer["validation.fields.price"], 0]);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage(localizer["validation.required", localizer["validation.fields.category"]]);
    }
}
```

---

### 3.4 Delete Command (YENİ 2 DOSYA)

**`Commands/DeleteProduct/DeleteProductCommand.cs`:**

```csharp
using Kickstart.Application.Common.Results;
using MediatR;

namespace Kickstart.Application.Features.Products.Commands.DeleteProduct;

public class DeleteProductCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
}
```

> **Önemli:** Delete command dönüş tipi `Result<bool>` (DeleteTenantCommandHandler ile aynı), `Result` (non-generic) değil. Bu projede pattern bu şekildedir.

**`Commands/DeleteProduct/DeleteProductCommandHandler.cs`:**

```csharp
using Kickstart.Application.Common.Localization;
using Kickstart.Application.Common.Results;
using Kickstart.Application.Interfaces;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Models;
using MediatR;

namespace Kickstart.Application.Features.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IJsonStringLocalizer _localizer;

    public DeleteProductCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IJsonStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _localizer = localizer;
    }

    public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id);

        if (product == null)
            return Result<bool>.Failure(Error.Failure(
                ErrorCode.NotFound,
                _localizer["products.notFound"]));

        // Tenant izolasyon kontrolü
        if (!_currentUserService.CanAccessAllTenants && product.TenantId != _currentUserService.TenantId)
            return Result<bool>.Failure(Error.Failure(
                ErrorCode.Forbidden,
                _localizer["products.noAccessToDelete"]));

        _unitOfWork.Products.SoftDelete(product);     // IsDeleted = true (fiziksel silme yok)
        await _unitOfWork.SaveChangesAsync();

        return Result<bool>.Success(true);
    }
}
```

> Delete için validator gerekmez (sadece Id alır, parametre route'tan gelir).

---

### 3.5 Get All Query (YENİ 2 DOSYA)

**`Queries/GetAllProducts/GetAllProductsQuery.cs`:**

```csharp
using Kickstart.Application.Common.Results;
using Kickstart.Application.Features.Products.Dtos;
using MediatR;

namespace Kickstart.Application.Features.Products.Queries.GetAllProducts;

public class GetAllProductsQuery : IRequest<PagedResult<ProductListDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SearchTerm { get; set; }
}
```

> **Pagination kullanılıyorsa** `PagedResult<T>` tercih et (frontend `useCrudOperations` ile uyumlu — items/totalCount/totalPages/pageNumber döner). Referans: `GetAllUsersQuery`.

**`Queries/GetAllProducts/GetAllProductsQueryHandler.cs`:**

```csharp
using AutoMapper;
using Kickstart.Application.Common.Results;
using Kickstart.Application.Features.Products.Dtos;
using Kickstart.Application.Interfaces;
using Kickstart.Domain.Common.Interfaces;
using MediatR;
using System.Collections.Generic;

namespace Kickstart.Application.Features.Products.Queries.GetAllProducts;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, PagedResult<ProductListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetAllProductsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<ProductListDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        // SuperAdmin tüm tenant'ların kayıtlarını görür; diğerleri sadece kendi tenant'ını
        int? tenantId = _currentUserService.CanAccessAllTenants ? null : _currentUserService.TenantId;

        var (products, totalCount) = await _unitOfWork.Products.GetProductsPagedAsync(
            request.Page, request.PageSize, request.SearchTerm, tenantId);

        var dtos = _mapper.Map<IEnumerable<ProductListDto>>(products);
        var totalPages = (int)System.Math.Ceiling(totalCount / (double)request.PageSize);

        return PagedResult<ProductListDto>.Success(dtos, request.Page, totalPages, totalCount);
    }
}
```

---

### 3.6 Get By Id Query (YENİ 2 DOSYA)

**`Queries/GetProductById/GetProductByIdQuery.cs`:**

```csharp
using Kickstart.Application.Common.Results;
using Kickstart.Application.Features.Products.Dtos;
using MediatR;

namespace Kickstart.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQuery : IRequest<Result<ProductDto>>
{
    public int Id { get; set; }
}
```

**`Queries/GetProductById/GetProductByIdQueryHandler.cs`:**

```csharp
using AutoMapper;
using Kickstart.Application.Common.Localization;
using Kickstart.Application.Common.Results;
using Kickstart.Application.Features.Products.Dtos;
using Kickstart.Application.Interfaces;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Models;
using MediatR;

namespace Kickstart.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IJsonStringLocalizer _localizer;

    public GetProductByIdQueryHandler(
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

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        // Query handler — ReadOnly variant kullan (AsNoTracking)
        var product = await _unitOfWork.Products.GetProductWithRelationsReadOnlyAsync(request.Id);

        if (product == null)
            return Result<ProductDto>.Failure(Error.Failure(
                ErrorCode.NotFound,
                _localizer["products.notFound"]));

        // Tenant izolasyon kontrolü
        if (!_currentUserService.CanAccessAllTenants && product.TenantId != _currentUserService.TenantId)
            return Result<ProductDto>.Failure(Error.Failure(
                ErrorCode.Forbidden,
                _localizer["products.noAccessToView"]));

        return Result<ProductDto>.Success(_mapper.Map<ProductDto>(product));
    }
}
```

---

### 3.7 AutoMapper Mapping (MEVCUT DOSYAYA SATIR EKLE)

**Dosya:** `Kickstart.Application/Features/Mappings/MappingProfile.cs`

Constructor'ın sonuna ekle:

```csharp
// Product mappings
CreateMap<Product, ProductListDto>()
    .ForMember(dest => dest.CategoryName,
        opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));

CreateMap<Product, ProductDto>();
    // İlişkili koleksiyonları (Images vb.) DTO yapına göre map etmek için ek `.ForMember` gerekirse ekle

// DTO → Command (Controller'da `_mapper.Map<CreateProductCommand>(dto)` kullanmak için):
CreateMap<CreateProductDto, CreateProductCommand>();
CreateMap<UpdateProductDto, UpdateProductCommand>();

// Eğer entity'ne direkt map edeceksen (genellikle handler'da elle yazılır):
// CreateMap<CreateProductCommand, Product>()
//     .ForMember(dest => dest.Images, opt => opt.Ignore())
//     .ForMember(dest => dest.ProductTags, opt => opt.Ignore());
```

> **Kural:** Navigation property koleksiyonları (Images, ProductTags, UserRoles vb.) mapping'de `opt.Ignore()` ile pas geçilir. Handler içinde elle yönetilir.

---

## ADIM 4 — API Katmanı

### 4.1 Controller (YENİ DOSYA)

**Dosya:** `Kickstart.API/Controllers/ProductsController.cs`

```csharp
using Kickstart.Application.Features.Products.Commands.CreateProduct;
using Kickstart.Application.Features.Products.Commands.UpdateProduct;
using Kickstart.Application.Features.Products.Commands.DeleteProduct;
using Kickstart.Application.Features.Products.Dtos;
using Kickstart.Application.Features.Products.Queries.GetAllProducts;
using Kickstart.Application.Features.Products.Queries.GetProductById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickstart.API.Controllers;

[Authorize]
public class ProductsController : BaseController
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all products with pagination
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "products.read")]
    [ProducesResponseType(typeof(IEnumerable<ProductListDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetAllProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string searchTerm = null)
    {
        var query = new GetAllProductsQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);
        return HandlePagedResult(result);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "products.read")]
    [ProducesResponseType(typeof(ProductDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProductById(int id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery { Id = id });
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "products.create")]
    [ProducesResponseType(typeof(ProductListDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var command = new CreateProductCommand
        {
            Name        = dto.Name,
            Description = dto.Description,
            Price       = dto.Price,
            CategoryId  = dto.CategoryId,
            TenantId    = dto.TenantId
        };

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
            return CreatedAtAction(
                nameof(GetProductById),
                new { id = result.Value.Id },
                new { success = true, data = result.Value });

        return HandleResult(result);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "products.update")]
    [ProducesResponseType(typeof(ProductListDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
    {
        var command = new UpdateProductCommand
        {
            Id          = id,
            Name        = dto.Name,
            Description = dto.Description,
            Price       = dto.Price,
            IsActive    = dto.IsActive,
            CategoryId  = dto.CategoryId
        };

        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a product (soft delete)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "products.delete")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var result = await _mediator.Send(new DeleteProductCommand { Id = id });
        return HandleResult(result);
    }
}
```

**Önemli:**
- `BaseController` zaten `[ApiController]`, `[Route("api/[controller]")]` ve `[Produces("application/json")]` tanımlar — tekrar ekleme.
- `BaseController` şu helper'ları sağlar: `HandleResult<T>(Result<T>)`, `HandleResult(Result)`, `HandlePagedResult<T>(PagedResult<T>)`.
- `[Authorize]` sınıf seviyesinde JWT zorunlu kılar. Her endpoint'e ayrıca `[Authorize(Policy = "...")]` ile permission policy eklenir.
- Policy ismi **küçük harf + nokta**'dır (örn. `"products.read"`). `PermissionPolicyProvider` policy adında nokta görürse dinamik olarak `PermissionRequirement` oluşturur ve `PermissionAuthorizationHandler` `StringComparer.OrdinalIgnoreCase` ile DB'deki permission'la (`"Products.Read"`) eşleştirir.

**Sadece-rol erişimi gerekirse** policy yerine bunları kullanabilirsin (zaten tanımlı):
- `[Authorize(Policy = "RequireAdminRole")]` — Admin veya SuperAdmin
- `[Authorize(Policy = "RequireSuperAdminRole")]` — Sadece SuperAdmin
- `[Authorize(Policy = "RequireManagerRole")]` — Manager, Admin veya SuperAdmin
- `[Authorize(Policy = "RequireUserRole")]` — Herhangi bir tanımlı rol
- `[Authorize(Policy = "RequireActiveUser")]`, `[Authorize(Policy = "RequireEmailVerified")]`

`TenantsController` tüm class'ta `[Authorize(Policy = "RequireSuperAdminRole")]` kullanır — örnek olarak referans al.

---

## ADIM 5 — Localization (MEVCUT DOSYALARA SATIR EKLE)

**Dosyalar:** `Kickstart.Infrastructure/Resources/en.json` ve `Kickstart.Infrastructure/Resources/tr.json`

İki dosyaya da entity için bir bölüm ekle:

```json
// en.json içine:
"products": {
    "notFound": "Product not found",
    "nameExists": "Product name already exists",
    "noAccessToView": "You do not have access to view this product",
    "noAccessToUpdate": "You do not have access to update this product",
    "noAccessToDelete": "You do not have access to delete this product",
    "onlySuperAdminCanSpecifyTenant": "Only SuperAdmin can create products in a specific tenant"
}

// validation.fields altına gerekirse alan isimleri:
"validation": {
    "fields": {
        "productId": "Product ID",
        "productName": "Product name",
        "price": "Price",
        "category": "Category"
    }
}
```

`tr.json` aynı yapıda Türkçe karşılıklarını içermelidir. Handler'larda ve validator'larda `_localizer["products.notFound"]` şeklinde key'le çağrılır.

---

## ADIM 6 — Migration ve Test

```bash
# 1. Migration oluştur
dotnet ef migrations add AddProductTable \
  --project Kickstart.Infrastructure \
  --startup-project Kickstart.API

# 2. DB'ye uygula
dotnet ef database update \
  --project Kickstart.Infrastructure \
  --startup-project Kickstart.API

# 3. API'yi başlat
dotnet run --project Kickstart.API

# 4. Swagger'da test et
# https://localhost:44333/swagger
```

> İlk başlangıçta `SeedPermissionsAsync()` yeni eklediğin `Products.Read/Create/Update/Delete` permission'larını otomatik olarak `Permissions` tablosuna ekler. SuperAdmin rolü tüm permission'ları otomatik alır.

---

## Standart Tenant Kontrolü Pattern'i

Tüm command/query handler'larda tekrar eden tenant izolasyon mantığı:

```csharp
// CREATE: yeni kayıt hangi tenant'a açılıyor?
int? tenantId;
if (request.TenantId.HasValue)
{
    if (!_currentUserService.CanAccessAllTenants)
        return Result<...>.Failure(Error.Failure(
            ErrorCode.Forbidden,
            _localizer["xxx.onlySuperAdminCanSpecifyTenant"]));
    tenantId = request.TenantId;
}
else
{
    tenantId = _currentUserService.TenantId;
}

// READ / UPDATE / DELETE: mevcut kayda erişim var mı?
if (!_currentUserService.CanAccessAllTenants && entity.TenantId != _currentUserService.TenantId)
    return Result<...>.Failure(Error.Failure(
        ErrorCode.Forbidden,
        _localizer["xxx.noAccessTo..."]));

// GetAll: kullanıcının tenant'ına göre filtreleme
int? tenantId = _currentUserService.CanAccessAllTenants ? null : _currentUserService.TenantId;
```

Bu kontroller yalnızca **tenant'a bağlı** entity'lerde (Product, Order vb.) gerekli. `Tenant`'ın kendisi gibi global entity'lerde gerekmez.

---

## ICurrentUserService — Kullanılabilir Property'ler

Handler constructor'ında `ICurrentUserService` inject et:

| Property/Metot | Tip | Açıklama |
|---|---|---|
| `UserId` | `int?` | Oturum açmış kullanıcının ID'si |
| `TenantId` | `int?` | Kullanıcının tenant'ı |
| `Email` | `string` | Kullanıcı e-postası |
| `FullName` | `string` | Ad soyad |
| `IsAuthenticated` | `bool` | Oturum açık mı? |
| `IsInRole(roleName)` | `bool` | Belirli bir rolde mi? |
| `CanAccessAllTenants` | `bool` | `true` ise SuperAdmin — tüm tenant'lara erişebilir |

> `HasPermission` metodu `ICurrentUserService`'de yoktur — permission kontrolü controller seviyesinde `[Authorize(Policy = "...")]` ile yapılır.

---

## İlişki Tipleri — Hızlı Referans

### Many-to-One (Product → Category)

```csharp
// Entity:
public int CategoryId { get; set; }
public Category Category { get; set; }

// EF Configuration:
builder.HasOne(x => x.Category)
    .WithMany(c => c.Products)
    .HasForeignKey(x => x.CategoryId)
    .OnDelete(DeleteBehavior.Restrict);

// Handler:
var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId);
if (category == null) return ...NotFound...;
product.CategoryId = request.CategoryId;
```

### One-to-Many (Product → ProductImage)

```csharp
// Parent entity:
public ICollection<ProductImage> Images { get; set; }

// EF Configuration:
builder.HasMany(x => x.Images)
    .WithOne(i => i.Product)
    .HasForeignKey(i => i.ProductId)
    .OnDelete(DeleteBehavior.Cascade);
```

### Many-to-Many (Product ↔ Tag) — User↔Role pattern

```csharp
// Ara entity (kendi entity'si olarak):
public class ProductTag : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public int TagId { get; set; }
    public Tag Tag { get; set; }
}

// IProductRepository'ye:
Task AddProductTagAsync(ProductTag productTag);
void RemoveProductTag(ProductTag productTag);

// Create handler:
foreach (var tagId in request.TagIds)
    await _unitOfWork.Products.AddProductTagAsync(
        new ProductTag { ProductId = product.Id, TagId = tagId });
await _unitOfWork.SaveChangesAsync();

// Update handler (tam yenileme):
var existing = product.ProductTags.ToList();
foreach (var tag in existing)
    _unitOfWork.Products.RemoveProductTag(tag);
foreach (var tagId in request.TagIds)
    await _unitOfWork.Products.AddProductTagAsync(
        new ProductTag { ProductId = product.Id, TagId = tagId });
await _unitOfWork.SaveChangesAsync();
```

Referans: `UserRepository.AddUserRoleAsync`, `UpdateUserCommandHandler`.

---

## Kontrol Listesi (Yeni Entity)

### Domain
- [ ] `Entities/Product.cs` — `BaseAuditableEntity`'den türetildi
- [ ] `Common/Interfaces/Repositories/IProductRepository.cs` — `IRepository<Product,int>`'i extend etti, özel metotlar eklendi, **adı `Repository` ile bitiyor**
- [ ] `Common/Interfaces/IUnitOfWork.cs` — `IProductRepository Products { get; }` eklendi
- [ ] `Constants/Permissions.cs` — `public static class Products` nested class'ı (Read/Create/Update/Delete) eklendi (PascalCase string'lerle)
- [ ] Enum gerekiyorsa `Common/Enums/` altına eklendi

### Infrastructure
- [ ] `Persistence/EntityConfigurations/ProductConfiguration.cs` — Fluent API yapılandırıldı
- [ ] `Repositories/ProductRepository.cs` — `RepositoryBase<Product,int>`'tan türedi, `IProductRepository` implement edildi, **adı `Repository` ile bitiyor**
- [ ] `Persistence/ApplicationDbContext.cs` — `DbSet<Product>` eklendi
- [ ] `Persistence/UnitOfWork.cs` — Lazy field + constructor init + property eklendi
- [ ] Migration oluşturuldu ve uygulandı (`dotnet ef migrations add ...`)

### Application
- [ ] `Features/Products/Dtos/CreateProductDto.cs`
- [ ] `Features/Products/Dtos/UpdateProductDto.cs`
- [ ] `Features/Products/Dtos/ProductListDto.cs`
- [ ] `Features/Products/Dtos/ProductDto.cs`
- [ ] `Features/Products/Commands/CreateProduct/` — Command + Handler + Validator
- [ ] `Features/Products/Commands/UpdateProduct/` — Command + Handler + Validator
- [ ] `Features/Products/Commands/DeleteProduct/` — Command + Handler (validator gerekmez)
- [ ] `Features/Products/Queries/GetAllProducts/` — Query + Handler (`PagedResult<>`)
- [ ] `Features/Products/Queries/GetProductById/` — Query + Handler
- [ ] `Features/Mappings/MappingProfile.cs` — Product mapping'leri eklendi

### API
- [ ] `Controllers/ProductsController.cs` — 5 endpoint (GetAll/GetById/Create/Update/Delete), her birinde `[Authorize(Policy = "products.xxx")]`

### Localization
- [ ] `Resources/en.json` — `"products": { ... }` bölümü eklendi
- [ ] `Resources/tr.json` — `"products": { ... }` bölümü eklendi (Türkçe karşılıklarla)

---

## Sık Yapılan Hatalar

| Hata | Doğrusu |
|------|---------|
| Repository'yi handler'a direkt inject etmek | `IUnitOfWork` inject et, `_unitOfWork.Products` kullan |
| `SaveChangesAsync()` çağrısını unutmak | Her `AddAsync`/`Update`/`SoftDelete` sonrası çağır |
| Fiziksel `Delete()` kullanmak | Her zaman `SoftDelete()` kullan |
| Permission string'ini sadece küçük harfle yazmak | `Permissions.cs`'de **PascalCase** (`"Products.Read"`), controller'da policy adı **küçük harf** (`"products.read"`) — case-insensitive eşleşir |
| `CreatedDate`/`CreatedBy`/`UpdatedDate`/`UpdatedBy`'ı handler'da manuel set etmek | `ApplicationDbContext.SaveChangesAsync()` override'ı otomatik halleder |
| Validator'ı handler içinde manuel çağırmak | `ValidationBehavior` pipeline'ı otomatik tetikler — sadece sınıfı oluştur |
| Mapper'da koleksiyon (UserRoles/Images vb.) eşlemesi yapmak | `opt.Ignore()` ile pas geç, handler'da elle yönet |
| Permission seed için `SeedData.cs`'i de değiştirmek | `SeedPermissionsAsync` reflection kullanır, sadece `Permissions.cs`'i değiştir |
| Repository sınıf/interface adının `Repository` ile bitmemesi | `AddRepositories()` reflection'ı kaydetmez — `IProductRepository` ve `ProductRepository` zorunlu |
| Delete handler'da `Result` (non-generic) dönüş tipi | Bu projede pattern `Result<bool>` (DeleteTenantCommandHandler/DeleteUserCommandHandler referans) |
| `GetAll` query'sinde `Result<IEnumerable<T>>` döndürmek | Pagination'lı liste için **`PagedResult<T>`** kullan — frontend `useCrudOperations` items/totalCount yapısı bekler |
| Controller'a `[ApiController]` veya `[Route]` eklemek | `BaseController` zaten tanımlamış |
| Query handler'da tracking'li yükleme yapmak | `ReadOnly` (AsNoTracking) varyant kullan (bellek/CPU avantajı); tracking sadece Command handler'larda gerekir |
| `MappingProfile.cs`'i hatırlamamak | Yeni entity'nin tüm mapping'leri tek bir yerde — `MappingProfile.cs` constructor'ında |
| `en.json` veya `tr.json`'a key eklememek | Localizer çağrısı çalışmaz, key string olarak döner |
