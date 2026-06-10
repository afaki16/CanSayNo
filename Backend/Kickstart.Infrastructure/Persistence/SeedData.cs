using Kickstart.Domain.Entities;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Kickstart.Infrastructure.Persistence
{
    public static class SeedData
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
            var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            try
            {
                // Clear existing data if needed
                await ClearExistingDataAsync(context, logger);

                // Seed Permissions
                await SeedPermissionsAsync(context);

                // Seed Roles
                await SeedRolesAsync(context);

                // Seed Tenants
                await SeedTenantsAsync(context);

                // Seed Admin User
                await SeedAdminUserAsync(context, logger, environment, configuration);



                logger.LogInformation("Seed data completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding data.");
                throw;
            }
        }

        public static async Task SeedAsyncIfEmpty(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
            var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            try
            {
                // Check if database has any data
                var hasAnyData = await context.Users.AnyAsync() ||
                                await context.Roles.AnyAsync() ||
                                await context.Permissions.AnyAsync() ||
                                await context.Tenants.AnyAsync();
            ;

            if (hasAnyData)
                {
                    logger.LogInformation("Database already contains data. Skipping seed data.");
                    return;
                }

                logger.LogInformation("Database is empty. Starting seed data process...");

                // Seed Permissions
                await SeedPermissionsAsync(context);

                // Seed Roles
                await SeedRolesAsync(context);

                // Seed Tenants
                await SeedTenantsAsync(context);

                // Seed Admin User
                await SeedAdminUserAsync(context, logger, environment, configuration);

                logger.LogInformation("Seed data completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding data.");
                throw;
            }
        }

        private static async Task ClearExistingDataAsync(ApplicationDbContext context, ILogger logger)
        {
            try
            {
                logger.LogInformation("Clearing existing data...");

                // Clear in correct order to avoid foreign key constraints
                context.UserRoles.RemoveRange(context.UserRoles);
                context.RolePermissions.RemoveRange(context.RolePermissions);
                context.Users.RemoveRange(context.Users);
                context.Tenants.RemoveRange(context.Tenants);
                context.Roles.RemoveRange(context.Roles);
                context.Permissions.RemoveRange(context.Permissions);
                context.RefreshTokens.RemoveRange(context.RefreshTokens);

                await context.SaveChangesAsync();
                logger.LogInformation("Existing data cleared successfully.");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error clearing existing data, continuing with seed...");
            }
        }

        public static async Task SeedPermissionsAsync(ApplicationDbContext context)
        {
            // Get all permissions from static constants
            var allPermissions = Permissions.Helper.GetAllPermissions();

            // Veritabanındaki mevcut permission isimlerini al
            var existingPermissionNames = await context.Permissions
                .Select(p => p.Name)
                .ToListAsync();

            var newPermissions = new List<Permission>();

            foreach (var permissionName in allPermissions)
            {
                if (existingPermissionNames.Contains(permissionName))
                    continue; // Zaten varsa ekleme

                var parts = permissionName.Split('.');
                if (parts.Length == 2)
                {
                    var resource = parts[0];
                    var action = parts[1];
                    var permissionType = MapActionToPermissionType(action);

                    newPermissions.Add(new Permission
                    {
                        Name = permissionName,
                        Description = $"Can {action.ToLower()} {resource.ToLower()}",
                        Resource = resource,
                        Type = permissionType
                    });
                }
            }

            if (newPermissions.Count > 0)
            {
                await context.Permissions.AddRangeAsync(newPermissions);
                await context.SaveChangesAsync();
            }
        }

        private static PermissionType MapActionToPermissionType(string action)
        {
            return action.ToLower() switch
            {
                "read" => PermissionType.Read,
                "create" => PermissionType.Create,
                "update" => PermissionType.Update,
                "delete" => PermissionType.Delete,
                _ => PermissionType.Read // Default to Read
            };
        }

        private static async Task SeedRolesAsync(ApplicationDbContext context)
        {
            var superAdminRole = new Role
            {
                Name = RoleNames.SuperAdmin,
                Description = "System super administrator with full access including sensitive operations",
                IsSystemRole = true
            };

            var adminRole = new Role
            {
                Name = RoleNames.Admin,
                Description = "System administrator with full access",
                IsSystemRole = true
            };

            var userRole = new Role
            {
                Name = RoleNames.User,
                Description = "Standard authenticated user. No management permissions by default. " +
                              "Derived projects should add domain-specific permissions as needed.",
                IsSystemRole = false
            };

            await context.Roles.AddRangeAsync(superAdminRole, adminRole, userRole);
            await context.SaveChangesAsync();

            // Get all permissions and assign to super admin and admin roles
            var allPermissions = await context.Permissions.ToListAsync();
            var superAdminRolePermissions = allPermissions.Select(p => new RolePermission
            {
                RoleId = superAdminRole.Id,
                PermissionId = p.Id
            });
            var adminRolePermissions = allPermissions.Select(p => new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = p.Id
            });

            await context.RolePermissions.AddRangeAsync(superAdminRolePermissions);
            await context.RolePermissions.AddRangeAsync(adminRolePermissions);

            // User role: NO PERMISSIONS by default.
            // Standard authenticated users have no management capabilities.
            // Derived projects should add specific permissions to User role as needed.

            await context.SaveChangesAsync();
        }

    private static async Task SeedTenantsAsync(ApplicationDbContext context)
    {
        // Check if tenants already exist
        if (await context.Tenants.AnyAsync())
        {
            return; // Tenants already seeded
        }

        var defaultTenant = new Tenant
        {
            Name = "Default Tenant",
            Description = "Default system tenant",
            Domain = "default",
            IsActive = true,
            ContactEmail = "admin@default.com",
            ContactPhone = "+905551234567",
            CreatedDate = DateTime.UtcNow
        };

        var demoTenant = new Tenant
        {
            Name = "Demo Tenant",
            Description = "Demo tenant for testing purposes",
            Domain = "demo",
            IsActive = true,
            ContactEmail = "admin@demo.com",
            ContactPhone = "+905559876543",
            CreatedDate = DateTime.UtcNow
        };

        await context.Tenants.AddRangeAsync(defaultTenant, demoTenant);
        await context.SaveChangesAsync();
    }

        private const int MinAdminPasswordLength = 8;

        private static async Task SeedAdminUserAsync(
            ApplicationDbContext context,
            ILogger logger,
            IHostEnvironment environment,
            IConfiguration configuration)
        {
            string superAdminPassword;
            string adminPassword;

            if (environment.IsDevelopment())
            {
                superAdminPassword = "SuperAdmin123!";
                adminPassword = "Admin123!";
                logger.LogInformation("Seeding admin users with default development passwords.");
            }
            else
            {
                superAdminPassword = configuration["INITIAL_SUPERADMIN_PASSWORD"]
                    ?? Environment.GetEnvironmentVariable("INITIAL_SUPERADMIN_PASSWORD");
                adminPassword = configuration["INITIAL_ADMIN_PASSWORD"]
                    ?? Environment.GetEnvironmentVariable("INITIAL_ADMIN_PASSWORD");

                if (string.IsNullOrWhiteSpace(superAdminPassword) ||
                    string.IsNullOrWhiteSpace(adminPassword) ||
                    superAdminPassword.Length < MinAdminPasswordLength ||
                    adminPassword.Length < MinAdminPasswordLength)
                {
                    logger.LogWarning(
                        "Skipping admin seed: INITIAL_SUPERADMIN_PASSWORD/INITIAL_ADMIN_PASSWORD env vars not set or shorter than {MinLength} characters. " +
                        "Create admin users manually or set the env vars and re-run on a fresh DB.",
                        MinAdminPasswordLength);
                    return;
                }
            }

            // Get default tenant
            var defaultTenant = await context.Tenants.FirstOrDefaultAsync(t => t.Domain == "default");

            var superAdminUser = new User
            {
                FirstName = "Super",
                LastName = "Administrator",
                Email = "superadmin@kickstart.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(superAdminPassword),
                PhoneNumber = "+905550000001",
                Status = UserStatus.Active,
                EmailConfirmed = true,
                PhoneConfirmed = true,
                TenantId = defaultTenant?.Id,
                CreatedDate = DateTime.UtcNow
            };

            var adminUser = new User
            {
                FirstName = "System",
                LastName = "Administrator",
                Email = "admin@kickstart.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                PhoneNumber = "+905551234567",
                Status = UserStatus.Active,
                EmailConfirmed = true,
                PhoneConfirmed = true,
                TenantId = defaultTenant?.Id,
                CreatedDate = DateTime.UtcNow
            };

            await context.Users.AddRangeAsync(superAdminUser, adminUser);
            await context.SaveChangesAsync();

            // Assign super admin role to super admin user
            var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.SuperAdmin);
            if (superAdminRole != null)
            {
                await context.UserRoles.AddAsync(new UserRole
                {
                    UserId = superAdminUser.Id,
                    RoleId = superAdminRole.Id
                });
            }

            // Assign admin role to admin user
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.Admin);
            if (adminRole != null)
            {
                await context.UserRoles.AddAsync(new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });
            }

            // Seed standard demo user only in Development
            if (environment.IsDevelopment())
            {
                var standardUser = new User
                {
                    FirstName = "Standard",
                    LastName = "User",
                    Email = "user@kickstart.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                    PhoneNumber = "+905559998877",
                    Status = UserStatus.Active,
                    EmailConfirmed = true,
                    PhoneConfirmed = true,
                    TenantId = defaultTenant?.Id,
                    CreatedDate = DateTime.UtcNow
                };

                await context.Users.AddAsync(standardUser);
                await context.SaveChangesAsync();

                var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.User);
                if (userRole != null)
                {
                    await context.UserRoles.AddAsync(new UserRole
                    {
                        UserId = standardUser.Id,
                        RoleId = userRole.Id
                    });
                }
            }

            await context.SaveChangesAsync();
        }
    }
} 
