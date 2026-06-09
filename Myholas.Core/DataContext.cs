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
        public DbSet<DeviceDto> Devices { get; set; }           

        public DbSet<EntityDto> Entities { get; set; }         

        public DbSet<StateEntityDto> States { get; set; }       

        public DbSet<AutomationEntityDto> Automations { get; set; } 

        public DbSet<UserEntityDto> Users { get; set; }         

        public DbSet<UserDeviceAccessDto> UserDeviceAccess { get; set; } 

        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        // Переопределяем провайдер БД
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = Myholas.Core.Options.ConnectionString;
            optionsBuilder.UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
      
            modelBuilder.Entity<DeviceDto>(entity =>
            {
                
                entity.HasIndex(d => d.DeviceId).IsUnique();
            });

            
            modelBuilder.Entity<EntityDto>(entity =>
            {
                
                entity.HasIndex(e => e.EntityId).IsUnique();

                // Одно устройство -много сущностей
                entity.HasOne(e => e.Device)
                    .WithMany(d => d.Entities)
                    .HasForeignKey(e => e.DeviceId)
                    .OnDelete(DeleteBehavior.Cascade); // Удалили устройство == удалили все его датчики
            });

            
            modelBuilder.Entity<StateEntityDto>(entity =>
            {
                entity.Property(s => s.Id).UseIdentityColumn();

                //  Одна сущность -- Много состояний
                entity.HasOne(s => s.Entity)
                    .WithMany(e => e.States)
                    .HasForeignKey(s => s.EntityId)
                    .OnDelete(DeleteBehavior.Cascade); // каскад

                // Индекс 
                entity.HasIndex(s => new { s.EntityId, s.CreatedAt });
            });

      
            modelBuilder.Entity<AutomationEntityDto>(entity =>
            {
 
                entity.HasOne(a => a.Entity)
                    .WithMany()
                    .HasForeignKey(a => a.EntityId)
                    .OnDelete(DeleteBehavior.Cascade); 

            
                entity.HasOne(a => a.CreatedByUser)
                    .WithMany(u => u.Automations)
                    .HasForeignKey(a => a.CreatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull); 
            });

      
            modelBuilder.Entity<UserEntityDto>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
            });

            
            modelBuilder.Entity<UserDeviceAccessDto>(entity =>
            {
                // Составной первичный ключ
                entity.HasKey(uda => new { uda.UserId, uda.DeviceId });

                entity.HasOne(uda => uda.User)
                    .WithMany(u => u.DeviceAccess)
                    .HasForeignKey(uda => uda.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(uda => uda.Device)
                    .WithMany(d => d.UserAccess)
                    .HasForeignKey(uda => uda.DeviceId)
                    .OnDelete(DeleteBehavior.Cascade); 

                entity.HasIndex(uda => uda.GrantedByUserId);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
