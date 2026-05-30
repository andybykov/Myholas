using Microsoft.EntityFrameworkCore;
using Myholas.Core.Dtos;
using Myholas.Core.Dtos.Automations;
using Myholas.Core.Dtos.Devices;
using Myholas.Core.Dtos.Users;

namespace Myholas.Core
{
    // ОО-представление БД
    public class DataContext : DbContext
    {
        // Подключаем DbSet's      
        public DbSet<DeviceDto> Devices { get; set; }           // Физические устройства (ESP)

        public DbSet<EntityDto> Entities { get; set; }         // Логические сущности (Датчики/Свет)

        public DbSet<StateEntityDto> States { get; set; }       // История состояний

        public DbSet<AutomationEntityDto> Automations { get; set; } // Автоматизации

        public DbSet<UserEntityDto> Users { get; set; }          // Пользователи

        public DbSet<UserDeviceAccessDto> UserDeviceAccess { get; set; } // Права доступа

        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        // Переопределяем провайдер БД
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = Myholas.Core.Options.ConnectionString;
            optionsBuilder.UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Настройка Device (Физическое устройство)
            modelBuilder.Entity<DeviceDto>(entity =>
            {
                // DeviceId (строка, например "esp-lamp01") должна быть уникальной
                entity.HasIndex(d => d.DeviceId).IsUnique();
            });

            // 2. Настройка Entity (Сущность/Датчик)
            modelBuilder.Entity<EntityDto>(entity =>
            {
                // EntityId (строка, например "sensor.temp") должна быть уникальной
                entity.HasIndex(e => e.EntityId).IsUnique();

                // Связь: Одно устройство -> Много сущностей
                entity.HasOne(e => e.Device)
                    .WithMany(d => d.Entities)
                    .HasForeignKey(e => e.DeviceId)
                    .OnDelete(DeleteBehavior.Cascade); // Удалили устройство -> удалили все его датчики
            });

            // 3. Настройка State (История состояний)
            modelBuilder.Entity<StateEntityDto>(entity =>
            {
                entity.Property(s => s.Id).UseIdentityColumn();

                // Связь: Одна сущность -> Много состояний
                entity.HasOne(s => s.Entity)
                    .WithMany(e => e.States)
                    .HasForeignKey(s => s.EntityId)
                    .OnDelete(DeleteBehavior.Cascade); // Удалили датчик -> удалили историю

                // Индекс для быстрой выборки истории по времени
                entity.HasIndex(s => new { s.EntityId, s.CreatedAt });
            });

            // 4. Настройка Automations (Автоматизации)
            modelBuilder.Entity<AutomationEntityDto>(entity =>
            {
                // Связь: Одна сущность -> Много автоматизаций
                // Теперь автоматизация привязана к конкретному датчику/переключателю
                entity.HasOne(a => a.Entity)
                    .WithMany()
                    .HasForeignKey(a => a.EntityId)
                    .OnDelete(DeleteBehavior.Cascade); // Удалили датчик -> удалили автоматизацию

                // Связь с пользователем (создателем)
                entity.HasOne(a => a.CreatedByUser)
                    .WithMany(u => u.Automations)
                    .HasForeignKey(a => a.CreatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull); // Создатель удален -> автоматизация остается (null)
            });

            // 5. Настройка User (Пользователи)
            modelBuilder.Entity<UserEntityDto>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
            });

            // 6. Настройка UserDeviceAccess (Права доступа)
            modelBuilder.Entity<UserDeviceAccessDto>(entity =>
            {
                // Составной первичный ключ: один пользователь может иметь одну запись прав на одно устройство
                entity.HasKey(uda => new { uda.UserId, uda.DeviceId });

                entity.HasOne(uda => uda.User)
                    .WithMany(u => u.DeviceAccess)
                    .HasForeignKey(uda => uda.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(uda => uda.Device)
                    .WithMany(d => d.UserAccess)
                    .HasForeignKey(uda => uda.DeviceId)
                    .OnDelete(DeleteBehavior.Cascade); // Удалили устройство -> удалили права доступа к нему

                entity.HasIndex(uda => uda.GrantedByUserId);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
