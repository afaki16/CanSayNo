using AutoMapper;
using Kickstart.Application.Features.Users.Dtos;
using Kickstart.Application.Features.Roles.Dtos;
using Kickstart.Application.Features.Tenants.Dtos;
using Kickstart.Application.Features.Permissions.Dtos;
using Kickstart.Application.Features.Auth.Dtos;
using Kickstart.Application.Features.Auth.Commands.Login;
using Kickstart.Application.Features.Auth.Commands.Logout;
using Kickstart.Application.Features.Auth.Commands.LogoutAll;
using Kickstart.Application.Features.Auth.Commands.LogoutDevice;
using Kickstart.Application.Features.Auth.Commands.Register;
using Kickstart.Application.Features.Auth.Commands.RefreshToken;
using Kickstart.Application.Features.Auth.Commands.ChangePassword;
using Kickstart.Application.Features.Auth.Commands.ForgotPassword;
using Kickstart.Application.Features.Auth.Commands.ResetPassword;
using Kickstart.Application.Features.Roles.Commands.CreateRole;
using Kickstart.Application.Features.Roles.Commands.UpdateRole;
using Kickstart.Application.Features.Roles.Commands.DeleteRole;
using Kickstart.Application.Features.Users.Commands.CreateUser;
using Kickstart.Application.Features.Users.Commands.UpdateUser;
using Kickstart.Application.Features.Users.Commands.DeleteUser;
using Kickstart.Application.Features.Tenants.Commands.CreateTenant;
using Kickstart.Application.Features.Tenants.Commands.UpdateTenant;
using Kickstart.Application.Features.Tenants.Commands.DeleteTenant;
using Kickstart.Domain.Entities;
using Kickstart.Domain.Common.Enums;
namespace Kickstart.Application.Features.Mappings;

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role)))
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => 
                    src.UserRoles.SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission)).Distinct()));

            // User list mapping (without permissions for performance)
            CreateMap<User, UserListDto>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role)));

            CreateMap<CreateUserCommand, User>()
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore());

            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore());

            CreateMap<UpdateUserCommand, User>()
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            CreateMap<RegisterCommand, User>()
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password)); // This will be hashed in handler

            CreateMap<RegisterRequestDto, RegisterCommand>();
            CreateMap<LoginRequestDto, LoginCommand>();
            CreateMap<RefreshTokenRequestDto, RefreshTokenCommand>();

            // Role mappings
            CreateMap<Role, RoleDto>()
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => 
                    src.RolePermissions.Select(rp => rp.Permission).Where(p => p != null).ToList()));

            CreateMap<CreateRoleCommand, Role>()
                .ForMember(dest => dest.RolePermissions, opt => opt.Ignore());

            CreateMap<CreateRoleDto, Role>()
                .ForMember(dest => dest.RolePermissions, opt => opt.Ignore());

            CreateMap<UpdateRoleCommand, Role>()
                .ForMember(dest => dest.RolePermissions, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            CreateMap<UpdateRoleDto, Role>()
                .ForMember(dest => dest.RolePermissions, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            // Permission mappings
            CreateMap<Permission, PermissionDto>()
                .ForMember(dest => dest.IndividualPermissions, opt => opt.MapFrom(src => 
                    src.Type.GetIndividualPermissions().Select(p => p.ToString()).ToList()));

           

            // UserRole mappings
            CreateMap<UserRole, UserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.User.Id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.User.Status))
                .ForMember(dest => dest.LastLoginDate, opt => opt.MapFrom(src => src.User.LastLoginDate))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.User.EmailConfirmed))
                .ForMember(dest => dest.PhoneConfirmed, opt => opt.MapFrom(src => src.User.PhoneConfirmed))
                .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src => src.User.ProfileImageUrl))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.User.CreatedDate))
                .ForMember(dest => dest.Roles, opt => opt.Ignore())
                .ForMember(dest => dest.Permissions, opt => opt.Ignore());

            // RolePermission mappings
            CreateMap<RolePermission, RoleDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Role.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Role.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Role.Description))
                .ForMember(dest => dest.IsSystemRole, opt => opt.MapFrom(src => src.Role.IsSystemRole))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.Role.CreatedDate))
                .ForMember(dest => dest.Permissions, opt => opt.Ignore());

            // RefreshToken mappings (plaintext secret is never on the entity; use JwtService / PlainToken)
            CreateMap<RefreshToken, LoginResponseDto>()
                .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
                .ForMember(dest => dest.ExpiresAt, opt => opt.MapFrom(src => src.ExpiryDate))
                .ForMember(dest => dest.AccessToken, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

        // Tenant mappings
        CreateMap<Tenant, TenantDto>()
            .ForMember(dest => dest.UserCount, opt => opt.MapFrom(src => src.Users != null ? src.Users.Count : 0));

        CreateMap<Tenant, TenantListDto>()
            .ForMember(dest => dest.UserCount, opt => opt.MapFrom(src => src.Users != null ? src.Users.Count : 0));

        // DTO -> Command mappings (Controller'da kullan�l�r)
        CreateMap<CreateTenantDto, CreateTenantCommand>();
        CreateMap<UpdateTenantDto, UpdateTenantCommand>();
    }
    }

