using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Myholas.BLL;
using Myholas.Core;
using Myholas.Core.Dtos;
using Myholas.Core.Interfaces;
using Myholas.Core.Models;
using Myholas.Core.MQTT;
using Myholas.DAL;
using Myholas.DAL.Repositories;
using System.Reflection;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Myholas.Debug
{
    public class Program
    {
        public static async Task ChekEventBus(EventBus bus)
        {
            // Проверка шины
            // Включить лампу
            bus.Emit("command.switch.lamp01", "ON");
            await Task.Delay(1000);
            bus.Emit("command.light.lamp01", "Toggle");
            await Task.Delay(1000);
            // Выключить нагреватель
            bus.Emit("command.switch.heater", "OFF");
            await Task.Delay(1000);
            // Установить режим "high"
            bus.Emit("command.select.heater_mode", "high");
        }

        public static async void Test1()
        {
            //  DI
            var services = new ServiceCollection();

            services.AddDbContext<DataContext>(options =>
                options.UseNpgsql(Myholas.Core.Options.ConnectionString));
            //  репозитории
            services.AddScoped<IDeviceRepository, DeviceRepository>();
            services.AddScoped<IStateRepository, StateRepository>();


            var sp = services.BuildServiceProvider();

            using (var scope = sp.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                await context.Database.EnsureCreatedAsync(); // проверка БД
            }

            //  компоненты 
            var mqttServer = "192.168.100.39";
            var mqttService = new Core.MQTT.MqttService();
            var commandSender = new MqttCommandSender(mqttService);
            var bus = new EventBus();
            var stateM = new StateMachine();
            var discovery = new MqttDeviceDiscoveryService(mqttService, mqttServer);
            //var bridge = new MqttToEventBusBridge(bus, discovery);


            //  репозитории 
            var deviceRepo = sp.GetRequiredService<IDeviceRepository>();
            var stateRepo = sp.GetRequiredService<IStateRepository>();

            // DeviceManager

            //var deviceManager = new DeviceManager(bus, stateM, commandSender, dbDevMan, deviceRepo, stateRepo);
            await Task.Delay(1000);
            // запуск
            await discovery.StartAsync();
            await Task.Delay(2000);
            // var dd = deviceManager.GetAllDevices();
            //var lamp01 = deviceRepo.GetByDeviceIdAsync("esp-lamp01");

            //var allDevices = await deviceRepo.GetAllAsync();  
            /*
                     var grouped = allDevices.GroupBy(d => d.DeviceId);
                     DeviceGroupOutputModel devOut = new DeviceGroupOutputModel();
                     var gr = deviceManager.GetGroupedDevices();

                     foreach(var group in gr)
                     {
                         var iid = group.Entities.FirstOrDefault().EntityId;
                         var dev = deviceManager.GetDevice(iid);
                         Console.WriteLine(group.Entities.FirstOrDefault().Domain.ToUpper());
                         if (group.Entities.FirstOrDefault().Domain.ToLower() == "SWITCH".ToLower())
                         {
                             Console.WriteLine("TOGGLE!!!");
                             dev.HandleCommand("TOGGLE".ToLower());
                         }

                         Console.WriteLine(iid);
                         Console.WriteLine("---");
                     }

                     //Console.WriteLine($"DEVICES: ");
                     foreach (var d in allDevices)
                     {
                         devOut.DeviceId = d.DeviceId;
                         devOut.FriendlyName = d.FriendlyName;
                         devOut.Ip = d.IpAdress;
                         devOut.Entities.FirstOrDefault(e => e.EntityId == d.EntityId);
                         //devOut.Ip = d.States.Any(s => s.CreatedAt)
                         var domain = d.Domain;
                         var tmp = devOut;
                         Console.WriteLine(domain);
                         //var id = devOut.Entities.FirstOrDefault().EntityId;
                         //Console.WriteLine($"EntiityID {id}");



                     }


                  Console.WriteLine("\n");

                     foreach (var group in grouped)
                     {
                         Console.WriteLine($"\nDeviceId: {group.Key}");
                         foreach (var dev in group)
                         {
                             Console.WriteLine($"  EntityId: {dev.EntityId}  (Domain: {dev.Domain}, State: {dev.CurrentState})");
                         }
                     }
            */


            Console.ReadLine();

            await discovery.StopAsync();        
        }

        public static async void Test2()
        {           
            var mqttServer = "192.168.100.39";
            var mqttService = new MqttService();
            var zigbeeService = new Zigbee2MqttDiscoveryService(mqttService, mqttServer);
            await zigbeeService.StartAsync();
          
        }
        public static async Task Main(string[] args)
        {
            Test2();
            Console.ReadLine();
        }
    }
}
