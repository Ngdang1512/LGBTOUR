using Microsoft.EntityFrameworkCore;
using LGBTOUR.Api.Entities;

namespace LGBTOUR.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Khai báo các bảng sẽ có trong Database
        public DbSet<POI> POIs { get; set; }
        public DbSet<Narration> Narrations { get; set; }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<TourPOI> TourPOIs { get; set; }
        public DbSet<UserLog> UserLogs { get; set; }
        public DbSet<Admin> Admins { get; set; }

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
        }
    }
}