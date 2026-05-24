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

        public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }

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

            // Уникальные индексы
            modelBuilder.Entity<DeviceEntityDto>()
                .HasIndex(d => d.EntityId)
                .IsUnique();

            modelBuilder.Entity<UserEntityDto>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<UserEntityDto>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Индексы
            modelBuilder.Entity<StateEntityDto>()
                .HasIndex(s => new { s.EntityId, s.CreatedAt });

            modelBuilder.Entity<RefreshTokenEntity>()
                .HasIndex(rt => rt.Token)
                .IsUnique();

            // Каскадное удаление 
            modelBuilder.Entity<StateEntityDto>()
                .HasOne(s => s.Device)
                .WithMany(d => d.States)
                .HasForeignKey(s => s.EntityId)
                .OnDelete(DeleteBehavior.Cascade);

 
            base.OnModelCreating(modelBuilder);

        }
    }
}
