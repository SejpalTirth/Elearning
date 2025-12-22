using Microsoft.EntityFrameworkCore;
using ProgresService.DAL.Models;

namespace ProgresService.DAL.Data;

public partial class ProgressDbContext : DbContext
{
    public ProgressDbContext(DbContextOptions<ProgressDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ProgressTracking> ProgressTrackings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

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
