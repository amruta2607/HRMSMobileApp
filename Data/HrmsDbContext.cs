using Microsoft.EntityFrameworkCore;
using MobileWebApi.Entities;

namespace MobileWebApi.Data
{
    public class HrmsDbContext : DbContext
    {
        public HrmsDbContext(DbContextOptions<HrmsDbContext> options)
            : base(options)
        {
        }

        public DbSet<TenantConfigurationEntity> TenantConfigurations => Set<TenantConfigurationEntity>();
        public DbSet<TenantConfiguredDayOffDay> TenantConfiguredDayOffDays => Set<TenantConfiguredDayOffDay>();
        public DbSet<Day> Days => Set<Day>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TenantConfigurationEntity>(entity =>
            {
                entity.ToTable("TenantConfiguration");
                entity.HasKey(e => e.TenantConfigurationId);
                entity.Property(e => e.TenantId).IsRequired();
            });

            modelBuilder.Entity<TenantConfiguredDayOffDay>(entity =>
            {
                entity.ToTable("TenantConfiguredDayOffDays");
                entity.HasKey(e => e.TenantConfiguredDayOffDaysId);
                entity.Property(e => e.TenantConfigurationId).IsRequired();
                entity.Property(e => e.DayOffId).IsRequired();
            });

            modelBuilder.Entity<Day>(entity =>
            {
                entity.ToTable("Days");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DayName)
                    .HasColumnName("Day")
                    .IsRequired()
                    .HasMaxLength(50);
            });
        }
    }
}
