using SaigonAudioTour.Api.Data;
using SaigonAudioTour.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace SaigonAudioTour.Api.Services;

/// <summary>
/// Utility for seeding default RBAC roles and permissions.
/// </summary>
public static class RbacSeeding
{
    /// <summary>
    /// Seed default roles and permissions into database.
    /// Should be called during application startup.
    /// </summary>
    public static async Task SeedRolesAndPermissionsAsync(ApplicationDbContext context)
    {
        try
        {
            // Check if data already seeded
            if (await context.Roles.AnyAsync())
                return;

            // Define default permissions
            var permissions = new List<Permission>
            {
                // POI Permissions
                new Permission { Name = "create_poi", Resource = "POI", Action = "Create", Description = "Tạo điểm thu âm mới" },
                new Permission { Name = "read_poi", Resource = "POI", Action = "Read", Description = "Xem thông tin điểm thu âm" },
                new Permission { Name = "update_poi", Resource = "POI", Action = "Update", Description = "Cập nhật điểm thu âm" },
                new Permission { Name = "delete_poi", Resource = "POI", Action = "Delete", Description = "Xóa điểm thu âm" },

                // Tour Permissions
                new Permission { Name = "create_tour", Resource = "Tour", Action = "Create", Description = "Tạo tour mới" },
                new Permission { Name = "read_tour", Resource = "Tour", Action = "Read", Description = "Xem thông tin tour" },
                new Permission { Name = "update_tour", Resource = "Tour", Action = "Update", Description = "Cập nhật tour" },
                new Permission { Name = "delete_tour", Resource = "Tour", Action = "Delete", Description = "Xóa tour" },

                // Narration Permissions
                new Permission { Name = "create_narration", Resource = "Narration", Action = "Create", Description = "Tạo bản dịch mới" },
                new Permission { Name = "read_narration", Resource = "Narration", Action = "Read", Description = "Xem bản dịch" },
                new Permission { Name = "update_narration", Resource = "Narration", Action = "Update", Description = "Cập nhật bản dịch" },
                new Permission { Name = "delete_narration", Resource = "Narration", Action = "Delete", Description = "Xóa bản dịch" },

                // Analytics Permissions
                new Permission { Name = "view_analytics", Resource = "Analytics", Action = "Read", Description = "Xem dữ liệu phân tích" },
                new Permission { Name = "export_analytics", Resource = "Analytics", Action = "Export", Description = "Xuất dữ liệu phân tích" },

                // User Management
                new Permission { Name = "manage_users", Resource = "Users", Action = "Update", Description = "Quản lý người dùng" },
                new Permission { Name = "view_users", Resource = "Users", Action = "Read", Description = "Xem danh sách người dùng" },

                // Admin & Role Management
                new Permission { Name = "manage_roles", Resource = "Roles", Action = "Update", Description = "Quản lý vai trò" },
                new Permission { Name = "manage_admins", Resource = "Admins", Action = "Update", Description = "Quản lý quản trị viên" },
                new Permission { Name = "view_audit_log", Resource = "AuditLog", Action = "Read", Description = "Xem nhật ký kiểm toán" },

                // Payment Management
                new Permission { Name = "manage_payments", Resource = "Payments", Action = "Update", Description = "Quản lý thanh toán" },
                new Permission { Name = "view_payments", Resource = "Payments", Action = "Read", Description = "Xem lịch sử thanh toán" }
            };

            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();

            // Define default roles
            var superAdminPermissions = permissions; // All permissions
            var adminPermissions = permissions.Where(p => p.Resource != "Roles" && p.Resource != "Admins").ToList(); // All except role/admin management
            var operatorPermissions = permissions.Where(p => new[] { "POI", "Tour", "Narration", "Analytics" }.Contains(p.Resource)).ToList(); // Content management only

            var superAdminRole = new Role
            {
                Name = "Super Admin",
                Description = "Toàn quyền truy cập tất cả tính năng",
                IsSystem = true,
                Permissions = superAdminPermissions
            };

            var adminRole = new Role
            {
                Name = "Admin",
                Description = "Quản lý nội dung và người dùng",
                IsSystem = true,
                Permissions = adminPermissions
            };

            var operatorRole = new Role
            {
                Name = "Operator",
                Description = "Quản lý nội dung điểm thu âm và tour",
                IsSystem = true,
                Permissions = operatorPermissions
            };

            await context.Roles.AddRangeAsync(new[] { superAdminRole, adminRole, operatorRole });
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log error but don't throw - allow app to start even if seeding fails
            System.Console.WriteLine($"Error seeding RBAC data: {ex.Message}");
        }
    }

    /// <summary>
    /// Assign a role to an admin user.
    /// </summary>
    public static async Task AssignRoleToAdminAsync(ApplicationDbContext context, int adminId, string roleName)
    {
        try
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
                throw new InvalidOperationException($"Role '{roleName}' not found");

            // Check if already assigned
            var existing = await context.AdminRoles
                .FirstOrDefaultAsync(ar => ar.AdminId == adminId && ar.RoleId == role.Id);

            if (existing != null)
                return; // Already assigned

            var adminRole = new AdminRole
            {
                AdminId = adminId,
                RoleId = role.Id,
                AssignedAt = DateTime.UtcNow
            };

            context.AdminRoles.Add(adminRole);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error assigning role to admin: {ex.Message}");
        }
    }
}
