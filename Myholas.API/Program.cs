using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Myholas.API.Services;
using Myholas.BLL;
using Myholas.BLL.Automation;
using Myholas.BLL.Device;
using Myholas.BLL.State;
using Myholas.BLL.User;
using Myholas.Core;
using Myholas.Core.Interfaces;
using Myholas.Core.MappingProfiles;
using Myholas.Core.MQTT;
using Myholas.DAL.Repositories;

namespace Myholas.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v0.1", new OpenApiInfo
                {
                    Title = "Myjolas Services API",
                    Version = "v0.1",
                });
            });

            builder.Services.AddSingleton<IMqttService, MqttService>();
            // Core  
            builder.Services.AddSingleton<IEventBus, EventBus>();
            builder.Services.AddSingleton<IStateMachine, StateMachine>();

            // AutoMapper 
            builder.Services.AddAutoMapper(cfg =>
                {
                    cfg.AddProfile<GeneralMappingProfile>(); //профили                    

                });

            // BLL  
            builder.Services.AddScoped<IDeviceManager, DeviceManager>();
            builder.Services.AddScoped<IStateManager, StateManager>();
            builder.Services.AddScoped<IUserManager, UserManager>();
            builder.Services.AddScoped<DeviceSynchronizerService>();
            builder.Services.AddScoped<ICommandService, CommandService>();           
            builder.Services.AddScoped<IAutomationManager, AutomationManager>();


            // builder.Services.AddSingleton<DeviceSynchronizer>();

            // DAL  
            builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
            builder.Services.AddScoped<IStateRepository, StateRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAutomationRepository, AutomationRepository>();

            // DbContext
            builder.Services.AddDbContext<DataContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString(Options.ConnectionString)));


            // MQTT 
            var mqttServer = "192.168.100.39";
            var mqttPort = "1883";

            builder.Services.AddSingleton<MqttDeviceDiscoveryService>(sp =>
            {
                var mqttService = sp.GetRequiredService<IMqttService>();
                return new MqttDeviceDiscoveryService(mqttService, mqttServer, mqttPort);
            });

            builder.Services.AddSingleton<MqttToEventBusBridge>(sp =>
            {
                var eventBus = sp.GetRequiredService<IEventBus>();
                var discovery = sp.GetRequiredService<MqttDeviceDiscoveryService>();
                return new MqttToEventBusBridge(eventBus, discovery);
            });

            builder.Services.AddScoped<ICommandSender, MqttCommandSender>(sp =>
               {
                   var service = sp.GetRequiredService<IMqttService>();
                   return new MqttCommandSender(service);
               });


            // Фоновый сервис
            builder.Services.AddHostedService<MqttBackgroundService>();
            builder.Services.AddHostedService<AutomationService>();


            // API должен разрешать кросс-доменные вызовы
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowBlazor", policy =>
                {
                    policy.WithOrigins("http://localhost:5000")   // порт web-server
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });


            var app = builder.Build();

            app.UseCors("AllowBlazor");

            // Инициализация базы данных 
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                await context.Database.EnsureCreatedAsync(); // EnsureCreated
            }           

           
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v0.1/swagger.json", "Myholas API v0.1");
                });
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}