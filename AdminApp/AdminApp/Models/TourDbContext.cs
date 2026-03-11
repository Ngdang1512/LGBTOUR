using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AdminApp.Models;

public partial class TourDbContext : DbContext
{
    public TourDbContext()
    {
    }

    public TourDbContext(DbContextOptions<TourDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Narration> Narrations { get; set; }

    public virtual DbSet<Poi> Pois { get; set; }

    public virtual DbSet<Tour> Tours { get; set; }

    public virtual DbSet<TourPoi> TourPois { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-6TGJRKP\\SQLEXPRESS;Database=TourDB;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Logs__3214EC073C8F1E52");

            entity.Property(e => e.ListenTime).HasColumnType("datetime");
            entity.Property(e => e.PoiId).HasColumnName("POI_Id");
            entity.Property(e => e.UserId).HasMaxLength(100);

            entity.HasOne(d => d.Poi).WithMany(p => p.Logs)
                .HasForeignKey(d => d.PoiId)
                .HasConstraintName("FK__Logs__POI_Id__4222D4EF");
        });

        modelBuilder.Entity<Narration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Narratio__3214EC076AB458BC");

            entity.ToTable("Narration");

            entity.Property(e => e.LanguageCode).HasMaxLength(10);
            entity.Property(e => e.PoiId).HasColumnName("POI_Id");

            entity.HasOne(d => d.Poi).WithMany(p => p.Narrations)
                .HasForeignKey(d => d.PoiId)
                .HasConstraintName("FK__Narration__POI_I__3F466844");
        });

        modelBuilder.Entity<Poi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__POI__3214EC070CC80C0C");

            entity.ToTable("POI");

            entity.Property(e => e.AudioPath).HasMaxLength(255);
            entity.Property(e => e.Image).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tour__3214EC07637D3068");

            entity.ToTable("Tour");

            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<TourPoi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tour_POI__3214EC079224AA68");

            entity.ToTable("Tour_POI");

            entity.Property(e => e.PoiId).HasColumnName("POI_Id");

            entity.HasOne(d => d.Poi).WithMany(p => p.TourPois)
                .HasForeignKey(d => d.PoiId)
                .HasConstraintName("FK__Tour_POI__POI_Id__3C69FB99");

            entity.HasOne(d => d.Tour).WithMany(p => p.TourPois)
                .HasForeignKey(d => d.TourId)
                .HasConstraintName("FK__Tour_POI__TourId__3B75D760");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
