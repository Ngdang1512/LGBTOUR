using System.ComponentModel.DataAnnotations;

namespace SaigonAudioTour.Api.Entities;

/// <summary>
/// Role entity for RBAC (Role-Based Access Control).
/// Admins can be assigned multiple roles, and roles define sets of permissions.
/// </summary>
public class Role
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    /// <summary>
    /// System roles cannot be deleted (Super Admin, Admin, Operator).
    /// User-defined roles can be deleted.
    /// </summary>
    public bool IsSystem { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    public ICollection<AdminRole> AdminRoles { get; set; } = new List<AdminRole>();
}

/// <summary>
/// Permission entity - defines what actions can be performed.
/// Permissions are assigned to roles, not directly to users.
/// </summary>
public class Permission
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // e.g., "create_poi", "edit_tour", "view_analytics"

    [Required]
    [MaxLength(50)]
    public string Resource { get; set; } = string.Empty; // e.g., "POI", "Tour", "Analytics", "Users"

    [Required]
    [MaxLength(20)]
    public string Action { get; set; } = string.Empty; // e.g., "Create", "Read", "Update", "Delete"

    [MaxLength(255)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation - many-to-many with Role
    public ICollection<Role> Roles { get; set; } = new List<Role>();
}

/// <summary>
/// Join table for Role-Permission many-to-many relationship.
/// </summary>
public class RolePermission
{
    public int RoleId { get; set; }
    public Role? Role { get; set; }

    public int PermissionId { get; set; }
    public Permission? Permission { get; set; }
}

/// <summary>
/// Join table for Admin-Role many-to-many relationship.
/// Allows one admin to have multiple roles (for flexibility).
/// </summary>
public class AdminRole
{
    public int AdminId { get; set; }
    public Admin? Admin { get; set; }

    public int RoleId { get; set; }
    public Role? Role { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
