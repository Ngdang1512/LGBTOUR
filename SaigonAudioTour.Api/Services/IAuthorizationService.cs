using SaigonAudioTour.Api.Data;
using SaigonAudioTour.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace SaigonAudioTour.Api.Services;

/// <summary>
/// Service for checking user permissions based on roles.
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Check if admin has a specific permission.
    /// </summary>
    Task<bool> HasPermissionAsync(int adminId, string resource, string action);

    /// <summary>
    /// Get all permissions for an admin (through their roles).
    /// </summary>
    Task<List<PermissionDto>> GetAdminPermissionsAsync(int adminId);

    /// <summary>
    /// Get all roles assigned to admin.
    /// </summary>
    Task<List<RoleDto>> GetAdminRolesAsync(int adminId);

    /// <summary>
    /// Check if admin has any of the specified roles.
    /// </summary>
    Task<bool> HasRoleAsync(int adminId, params string[] roleNames);
}

/// <summary>
/// Implementation of authorization service.
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthorizationService> _logger;

    public AuthorizationService(ApplicationDbContext context, ILogger<AuthorizationService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> HasPermissionAsync(int adminId, string resource, string action)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(resource) || string.IsNullOrWhiteSpace(action))
                return false;

            // Check if admin has role with this permission
            var hasPermission = await _context.AdminRoles
                .Where(ar => ar.AdminId == adminId)
                .SelectMany(ar => ar.Role!.Permissions)
                .AnyAsync(p => p.Resource == resource && p.Action == action);

            if (!hasPermission)
            {
                _logger.LogWarning("Admin {AdminId} denied permission {Resource}:{Action}", adminId, resource, action);
                return false;
            }

            _logger.LogInformation("Admin {AdminId} granted permission {Resource}:{Action}", adminId, resource, action);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission for admin {AdminId}", adminId);
            return false;
        }
    }

    public async Task<List<PermissionDto>> GetAdminPermissionsAsync(int adminId)
    {
        try
        {
            var permissions = await _context.AdminRoles
                .Where(ar => ar.AdminId == adminId)
                .SelectMany(ar => ar.Role!.Permissions)
                .Distinct()
                .Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Resource = p.Resource,
                    Action = p.Action,
                    Description = p.Description
                })
                .ToListAsync();

            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for admin {AdminId}", adminId);
            return new List<PermissionDto>();
        }
    }

    public async Task<List<RoleDto>> GetAdminRolesAsync(int adminId)
    {
        try
        {
            var roles = await _context.AdminRoles
                .Where(ar => ar.AdminId == adminId)
                .Select(ar => new RoleDto
                {
                    Id = ar.Role!.Id,
                    Name = ar.Role.Name,
                    Description = ar.Role.Description,
                    IsSystem = ar.Role.IsSystem,
                    PermissionCount = ar.Role.Permissions.Count
                })
                .ToListAsync();

            return roles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for admin {AdminId}", adminId);
            return new List<RoleDto>();
        }
    }

    public async Task<bool> HasRoleAsync(int adminId, params string[] roleNames)
    {
        try
        {
            if (roleNames == null || roleNames.Length == 0)
                return false;

            var hasRole = await _context.AdminRoles
                .Where(ar => ar.AdminId == adminId)
                .Select(ar => ar.Role!.Name)
                .AnyAsync(name => roleNames.Contains(name));

            return hasRole;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking roles for admin {AdminId}", adminId);
            return false;
        }
    }
}

/// <summary>
/// DTOs for role and permission data transfer.
/// </summary>
public class PermissionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public int PermissionCount { get; set; }
}

public class RoleWithPermissionsDto : RoleDto
{
    public List<PermissionDto> Permissions { get; set; } = new();
}
