using Myholas.BLL.Device;
using Myholas.Core.Interfaces;
using Myholas.Core.MQTT;

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
            // ЭКЗКЕМПЛЯРЫ ИЗ DI!!!!!!!!!!!!!!!!!!!!!!!!!!!
            var discovery = scope.ServiceProvider.GetRequiredService<MqttDeviceDiscoveryService>();
            var stateManager = scope.ServiceProvider.GetRequiredService<IStateManager>();
            var synchronizer = scope.ServiceProvider.GetRequiredService<DeviceSynchronizerService>();
            var bridge = scope.ServiceProvider.GetRequiredService<MqttToEventBusBridge>();


            await discovery.StartAsync();
            await stateManager.InitializeCacheAsync();

            try
            {
                await Task.Delay(-1, stoppingToken);
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine($"MQTT service stopping. Reason:{ex}");
            }
            finally
            {
                await discovery.StopAsync();
                Console.WriteLine("MQTT service stopping");
            }
        }
    }
}

