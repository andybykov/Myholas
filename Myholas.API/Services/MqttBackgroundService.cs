using Myholas.BLL.Device;
using Myholas.Core.Interfaces;
using Myholas.Core.MQTT;
using Myholas.Core.Dtos.DeserializationDtos.ESPDevices;
using Myholas.Core.Dtos.DeserializationDtos.Z2mDevices;

namespace Myholas.API.Services
{
    // Фоновый сервис для работы MQTT
    public class MqttBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;

        public MqttBackgroundService(IServiceProvider services)
        {
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _services.CreateScope();

            // 1. Получаем экземпляры сервисов дискавери
            var espDiscovery = scope.ServiceProvider.GetRequiredService<MqttDeviceDiscoveryService>();
            var z2mDiscovery = scope.ServiceProvider.GetRequiredService<Zigbee2MqttDiscoveryService>();

            // 2. Получаем менеджер состояний и синхронизатор
            var stateManager = scope.ServiceProvider.GetRequiredService<IStateManager>();
            var synchronizer = scope.ServiceProvider.GetRequiredService<DeviceSynchronizerService>();

            // 3. Инициализируем МОСТЫ. 
            // Поскольку они Generic, указываем конкретные типы.
            // Простого вызова GetRequiredService достаточно, чтобы мосты создались и подписались на события.
            scope.ServiceProvider.GetRequiredService<MqttToEventBusBridge<EspDeviceDto, BaseEntityConfigDto>>();
            scope.ServiceProvider.GetRequiredService<MqttToEventBusBridge<Z2MDeviceDto, Z2MExposeDto>>();

            // 4. Запуск
            await espDiscovery.StartAsync();
            await z2mDiscovery.StartAsync();
            await stateManager.InitializeCacheAsync();

            Console.WriteLine("[MqttBackgroundService] All MQTT discovery services started.");

            try
            {
                // Ждем отмены токена (остановки приложения)
                await Task.Delay(-1, stoppingToken);
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine($"MQTT service stopping. Reason: {ex.Message}");
            }
            finally
            {
                // 5. Корректная остановка всех сервисов
              //  await espDiscovery.StopAsync();
                await z2mDiscovery.StopAsync();
                Console.WriteLine("MQTT services stopped.");
            }
        }
    }
}
