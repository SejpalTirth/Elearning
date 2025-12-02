using Microsoft.EntityFrameworkCore;
using ProgresService.DAL.Models;

namespace ProgresService.DAL.Data;

public partial class ProgressDbContext : DbContext
{
    public ProgressDbContext(DbContextOptions<ProgressDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CourseCompletion> CourseCompletions { get; set; }

    public virtual DbSet<ProgressTracking> ProgressTrackings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CourseCompletion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CourseCo__3214EC0706059A1F");

            entity.ToTable("CourseCompletion");

            entity.Property(e => e.CertificateUrl).HasMaxLength(500);
            entity.Property(e => e.CompletedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<ProgressTracking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Progress__3214EC07ABD94E4B");

            entity.ToTable("ProgressTracking");

            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ProgressPercent).HasColumnType("decimal(5, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
