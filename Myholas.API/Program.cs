using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Myholas.API.Services;
using Myholas.BLL.Automation;
using Myholas.BLL.Device;
using Myholas.BLL.State;
using Myholas.BLL.User;
using Myholas.Core;
using Myholas.Core.Dtos.DeserializationDtos.ESPDevices;
using Myholas.Core.Dtos.DeserializationDtos.Z2mDevices;
using Myholas.Core.Interfaces;
using Myholas.Core.MappingProfiles;
using Myholas.Core.MQTT;
using Myholas.DAL.Repositories;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;

namespace Myholas.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            // Swagger
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v0.1", new OpenApiInfo
                {
                    Title = "Myjolas Services API",
                    Version = "v0.1",
                });

                // Authorize в Swagger
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // из appsettings.json
            var jwtKey = builder.Configuration["JwtSettings:Key"];
            var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
            var jwtAudience = builder.Configuration["JwtSettings:Audience"];


            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new Exception("JWT Key not found in appsettings.json!");
            }

            // JWT auth
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "MyholasServer",
                        ValidAudience = "MyholasClient",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        RoleClaimType = ClaimTypes.Role // Чтобы работал [Authorize(Roles = "Admin")]
                    };
                });

            builder.Services.AddAuthorization();
            
            // Core  
            builder.Services.AddSingleton<IMqttService, MqttService>();           
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
            // builder.Services.AddSingleton<DeviceSynchronizer>();
            builder.Services.AddScoped<ICommandService, CommandService>();
            builder.Services.AddScoped<IAutomationManager, AutomationManager>();


            

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

            //  ESPHome Discovery
            builder.Services.AddSingleton<MqttDeviceDiscoveryService>(sp =>
            {
                var mqttEspService = sp.GetRequiredService<IMqttService>();
                return new MqttDeviceDiscoveryService(mqttEspService, mqttServer, mqttPort);
            });

            // Регистрируем его же как интерфейс
            builder.Services.AddSingleton<IMqttDiscoveryService<EspDeviceDto, BaseEntityConfigDto>>(sp =>
                sp.GetRequiredService<MqttDeviceDiscoveryService>());

            //  Zigbee2MQTT Discovery
            builder.Services.AddSingleton<Zigbee2MqttDiscoveryService>(sp =>
            {
                var mqttService = sp.GetRequiredService<IMqttService>();
                return new Zigbee2MqttDiscoveryService(mqttService, mqttServer, mqttPort);
            });

            // Регистрируем его же как интерфейс
            builder.Services.AddSingleton<IMqttDiscoveryService<Z2MDeviceDto, Z2MExposeDto>>(sp =>
                sp.GetRequiredService<Zigbee2MqttDiscoveryService>());

            // Мост для ESPHome
            builder.Services.AddSingleton<MqttToEventBusBridge<EspDeviceDto, BaseEntityConfigDto>>(sp =>
            {
                var eventBus = sp.GetRequiredService<IEventBus>();             
                var discovery = sp.GetRequiredService<IMqttDiscoveryService<EspDeviceDto, BaseEntityConfigDto>>();
                return new MqttToEventBusBridge<EspDeviceDto, BaseEntityConfigDto>(eventBus, discovery);
            });

            // Мост для Zigbee2MQTT
            builder.Services.AddSingleton<MqttToEventBusBridge<Z2MDeviceDto, Z2MExposeDto>>(sp =>
            {
                var eventBus = sp.GetRequiredService<IEventBus>();               
                var discovery = sp.GetRequiredService<IMqttDiscoveryService<Z2MDeviceDto, Z2MExposeDto>>();
                return new MqttToEventBusBridge<Z2MDeviceDto, Z2MExposeDto>(eventBus, discovery);
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
            app.UseCors("AllowBlazor"); // CORS
            app.UseAuthentication(); // Аутентификация
            app.UseAuthorization();  // Потом авторизация
            app.MapControllers();
            app.Run();
        }
    }
}