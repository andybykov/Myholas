using Microsoft.EntityFrameworkCore;
using Myholas.Core.Dtos;

namespace Myholas.Core
{
    // ОО-представление БД
    public class DataContext : DbContext
    {
        // Подключаем DbSet's      
        public DbSet<DeviceEntityDto> Devices { get; set; }

        public DbSet<StateEntityDto> States { get; set; }

        public DbSet<AutomationEntityDto> Automations { get; set; }

        public DbSet<UserEntityDto> Users { get; set; }

        public DbSet<UserDeviceAccessDto> UserDeviceAccess { get; set; }

        //public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options) { }


        // Переопределяем провайдер БД
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = Myholas.Core.Options.ConnectionString;
            optionsBuilder.UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StateEntityDto>(entity =>
            {
                entity.Property(e => e.Id).UseIdentityColumn();
            });

            // DeviceEntityDto
            modelBuilder.Entity<DeviceEntityDto>(entity =>
            {
                entity.HasIndex(d => d.EntityId).IsUnique();
                entity.HasIndex(d => d.DeviceId).IsUnique(); // альтернативный ключ
            });

            modelBuilder.Entity<UserEntityDto>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
            });

            modelBuilder.Entity<StateEntityDto>()
                .HasIndex(s => new { s.EntityId, s.CreatedAt });

            modelBuilder.Entity<StateEntityDto>()
                .HasOne(s => s.Device)
                .WithMany(d => d.States)
                .HasForeignKey(s => s.EntityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройка UserDeviceAccess
            modelBuilder.Entity<UserDeviceAccessDto>(entity =>
            {
                entity.HasKey(uda => new { uda.UserId, uda.DeviceId });

                entity.HasOne(uda => uda.User)
                    .WithMany(u => u.DeviceAccess)
                    .HasForeignKey(uda => uda.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(uda => uda.Device)
                    .WithMany(d => d.UserAccess)
                    .HasForeignKey(uda => uda.DeviceId)
                    .HasPrincipalKey(d => d.DeviceId)   // важно: DeviceId как альтернативный ключ
                    .OnDelete(DeleteBehavior.Cascade);

                

                entity.HasIndex(uda => uda.GrantedByUserId);
            });

            // Настройка связи Automation -> User (CreatedByUser)
            modelBuilder.Entity<AutomationEntityDto>(entity =>
            {
                entity.HasOne(a => a.CreatedByUser)
                    .WithMany(u => u.Automations)
                    .HasForeignKey(a => a.CreatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull); // или Cascade
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
