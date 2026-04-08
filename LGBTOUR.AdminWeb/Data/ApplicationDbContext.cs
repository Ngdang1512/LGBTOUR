using Microsoft.EntityFrameworkCore;
using LGBTOUR.AdminWeb.Entities;

namespace LGBTOUR.AdminWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Khai báo các bảng sẽ xuất hiện trong SQL Server
        public DbSet<POI> POIs { get; set; }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<TourPOI> TourPOIs { get; set; }
        public DbSet<Narration> Narrations { get; set; }
        public DbSet<Audio> Audios { get; set; }
        public DbSet<UserLog> UserLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Thiết lập quan hệ n-n cho bảng trung gian TourPOI
            modelBuilder.Entity<TourPOI>()
                .HasOne(tp => tp.Tour)
                .WithMany(t => t.TourPOIs)
                .HasForeignKey(tp => tp.TourId);

            modelBuilder.Entity<TourPOI>()
                .HasOne(tp => tp.POI)
                .WithMany(p => p.TourPOIs)
                .HasForeignKey(tp => tp.POI_Id);
            modelBuilder.Entity<UserLog>()
               .HasOne(ul => ul.POI)
               .WithMany(p => p.UserLogs)
               .HasForeignKey(ul => ul.POIId)
               .OnDelete(DeleteBehavior.SetNull);
        }
    }
}