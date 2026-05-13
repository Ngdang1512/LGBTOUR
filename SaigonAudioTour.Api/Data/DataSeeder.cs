using SaigonAudioTour.Api.Entities;
using SaigonAudioTour.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace SaigonAudioTour.Api.Data;

public static class DataSeeder
{
    public static void SeedAll(ApplicationDbContext context, bool useSqlServer)
    {
        EnsureUsersTable(context, useSqlServer);
        MigrateLegacyUsers(context);
        SeedDefaultAdmin(context);
        SeedDefaultPois(context);
        RbacSeeding.SeedRolesAndPermissionsAsync(context).Wait();
    }

    private static void SeedDefaultAdmin(ApplicationDbContext context)
    {
        if (!context.Admins.Any(a => a.Username == "MinhVy"))
        {
            context.Admins.Add(new Admin
            {
                Username = "MinhVy",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                FullName = "Admin Tuyến Xe Buýt"
            });
            context.SaveChanges();
        }
    }

    private static void SeedDefaultPois(ApplicationDbContext context)
    {
        var defaultPois = new[]
        {
            new { Name = "Chợ Bến Thành",               Description = "Phường Bến Thành, Quận 1, TP.HCM",                     Lat = 10.77252, Lng = 106.69805, Radius = 70, Priority = 10, IsStopStation = true },
            new { Name = "Nhà hát Thành phố",            Description = "Số 7 Công trường Lam Sơn, Quận 1, TP.HCM",             Lat = 10.77662, Lng = 106.70326, Radius = 65, Priority = 9,  IsStopStation = true },
            new { Name = "Trụ sở Ủy ban Nhân dân TP.HCM",Description = "Số 86 Lê Thánh Tôn, Quận 1, TP.HCM",                  Lat = 10.77695, Lng = 106.69949, Radius = 60, Priority = 8,  IsStopStation = true },
            new { Name = "Dinh Độc Lập",                 Description = "135 Nam Kỳ Khởi Nghĩa, Quận 1, TP.HCM",               Lat = 10.77713, Lng = 106.69531, Radius = 60, Priority = 7,  IsStopStation = true },
            new { Name = "Nhà thờ Đức Bà",               Description = "Số 1 Công xã Paris, Quận 1, TP.HCM",                   Lat = 10.77978, Lng = 106.69902, Radius = 58, Priority = 6,  IsStopStation = true },
            new { Name = "Bưu điện Thành phố",           Description = "Số 2 Công xã Paris, Quận 1, TP.HCM",                   Lat = 10.77956, Lng = 106.70049, Radius = 55, Priority = 5,  IsStopStation = true },
            new { Name = "Bảo tàng Chứng tích Chiến tranh",Description = "28 Võ Văn Tần, Quận 3, TP.HCM",                    Lat = 10.77937, Lng = 106.69134, Radius = 55, Priority = 4,  IsStopStation = true },
            new { Name = "Bảo tàng Lịch sử",             Description = "2 Nguyễn Bỉnh Khiêm, Quận 1, TP.HCM",                 Lat = 10.78693, Lng = 106.70252, Radius = 55, Priority = 3,  IsStopStation = true },
            new { Name = "Thảo Cầm Viên Sài Gòn",        Description = "2 Nguyễn Bỉnh Khiêm, Quận 1, TP.HCM",                 Lat = 10.78743, Lng = 106.70452, Radius = 60, Priority = 2,  IsStopStation = true },
            new { Name = "Saigon Skydeck",                Description = "Tầng 49, Bitexco Financial Tower, Quận 1, TP.HCM",    Lat = 10.77168, Lng = 106.70411, Radius = 60, Priority = 1,  IsStopStation = true },
        };

        foreach (var seed in defaultPois)
        {
            if (!context.POIs.Any(p => p.Name == seed.Name))
            {
                context.POIs.Add(new POI
                {
                    Name = seed.Name,
                    Description = seed.Description,
                    Lat = seed.Lat,
                    Lng = seed.Lng,
                    Radius = seed.Radius,
                    Priority = seed.Priority,
                    IsStopStation = seed.IsStopStation
                });
            }
        }
        context.SaveChanges();
    }

    private static void EnsureUsersTable(ApplicationDbContext context, bool useSqlServer)
    {
        if (useSqlServer) return;

        context.Database.ExecuteSqlRaw(@"
CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Email TEXT NOT NULL,
    PasswordHash TEXT NOT NULL,
    FullName TEXT NOT NULL,
    AvatarUrl TEXT NULL
);");
        context.Database.ExecuteSqlRaw(
            "CREATE UNIQUE INDEX IF NOT EXISTS IX_Users_Email ON Users(Email);");
    }

    private static void MigrateLegacyUsers(ApplicationDbContext context)
    {
        var legacyUsers = context.Admins
            .Where(a => a.Username.Contains('@'))
            .Select(a => new
            {
                Email = a.Username.Trim().ToLowerInvariant(),
                a.PasswordHash,
                FullName = string.IsNullOrWhiteSpace(a.FullName) ? a.Username : a.FullName
            })
            .ToList();

        if (legacyUsers.Count == 0) return;

        var existingEmails = context.Users
            .AsNoTracking()
            .Select(u => u.Email)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var imported = false;
        foreach (var legacy in legacyUsers)
        {
            if (existingEmails.Contains(legacy.Email)) continue;
            context.Users.Add(new AppUser
            {
                Email = legacy.Email,
                PasswordHash = legacy.PasswordHash,
                FullName = legacy.FullName,
                AvatarUrl = null,
                SubscriptionStatus = "free"
            });
            existingEmails.Add(legacy.Email);
            imported = true;
        }

        if (imported) context.SaveChanges();
    }
}
