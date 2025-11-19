using Microsoft.EntityFrameworkCore;

namespace UserService.DAL.Models
{
    public partial class UserContext : DbContext
    {
        public UserContext() { }

        public UserContext(DbContextOptions<UserContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }

        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(
                "Server=SYN-009738\\SQLEXPRESS;Database=Elearning;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True");


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //-------------------------------------------------------
            // USER
            //-------------------------------------------------------
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                // USER ↔ PERMISSIONS
                entity.HasMany(u => u.Permissions)
                      .WithMany(p => p.Users)
                      .UsingEntity<UserPermission>(
                        j =>
                        {
                            j.HasKey(up => new { up.UserId, up.PermissionId });
                            j.ToTable("UserPermissions");
                        });

                // USER ↔ ROLES
                entity.HasMany(u => u.Roles)
                      .WithMany(r => r.Users)
                      .UsingEntity<UserRole>(
                        j =>
                        {
                            j.HasKey(ur => new { ur.UserId, ur.RoleId });
                            j.ToTable("UserRoles");
                        });
            });

            //-------------------------------------------------------
            // ROLE
            //-------------------------------------------------------
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);

                // ROLE ↔ PERMISSIONS
                entity.HasMany(r => r.Permissions)
                      .WithMany(p => p.Roles)
                      .UsingEntity<RolePermission>(
                        j =>
                        {
                            j.HasKey(rp => new { rp.RoleId, rp.PermissionId });
                            j.ToTable("RolePermissions");
                        });
            });

            //-------------------------------------------------------
            // PERMISSION
            //-------------------------------------------------------
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            //-------------------------------------------------------
            // REFRESH TOKENS
            //-------------------------------------------------------
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

                entity.HasOne(rt => rt.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(rt => rt.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
