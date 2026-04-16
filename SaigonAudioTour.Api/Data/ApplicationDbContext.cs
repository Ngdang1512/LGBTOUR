using Microsoft.EntityFrameworkCore;
using SaigonAudioTour.Api.Entities;

namespace SaigonAudioTour.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Khai báo các bảng sẽ có trong Database
        public DbSet<POI> POIs { get; set; }
        public DbSet<Narration> Narrations { get; set; }
        public DbSet<Audio> Audios { get; set; }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<TourPOI> TourPOIs { get; set; }
        public DbSet<UserLog> UserLogs { get; set; }
        public DbSet<AppUser> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        
        // RBAC & 2FA
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<AdminRole> AdminRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình rõ ràng mối quan hệ Many-to-Many giữa Tour và POI
            modelBuilder.Entity<TourPOI>()
                .HasOne(tp => tp.Tour)
                .WithMany(t => t.TourPOIs)
                .HasForeignKey(tp => tp.TourId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TourPOI>()
                .HasOne(tp => tp.POI)
                .WithMany(p => p.TourPOIs)
                .HasForeignKey(tp => tp.POI_Id)
                .OnDelete(DeleteBehavior.Cascade);

            // Đảm bảo UserLog nếu xóa POI thì chỉ set Null chứ không xóa lịch sử người dùng
            modelBuilder.Entity<UserLog>()
                .HasOne(ul => ul.POI)
                .WithMany(p => p.UserLogs)
                .HasForeignKey(ul => ul.POIId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("Users");
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // ========== RBAC Configuration ==========
            // Role-Permission many-to-many
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany()
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany()
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Admin-Role many-to-many
            modelBuilder.Entity<AdminRole>()
                .HasKey(ar => new { ar.AdminId, ar.RoleId });

            modelBuilder.Entity<AdminRole>()
                .HasOne(ar => ar.Admin)
                .WithMany()
                .HasForeignKey(ar => ar.AdminId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AdminRole>()
                .HasOne(ar => ar.Role)
                .WithMany(r => r.AdminRoles)
                .HasForeignKey(ar => ar.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Role-Permission relationship via junction table
            modelBuilder.Entity<Role>()
                .HasMany(r => r.Permissions)
                .WithMany(p => p.Roles)
                .UsingEntity<RolePermission>();
        }
    }
}