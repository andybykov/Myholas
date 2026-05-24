using Myholas.Core.Dtos;
using Myholas.Core.Interfaces;

namespace Myholas.BLL
{
    //  формирует и отправляет команды устройствам
    public class CommandService : ICommandService
    {
        private readonly IDeviceRepository _deviceRepo;

        private readonly IStateRepository _statesRepo;

        private readonly ICommandSender _cmd;


        public CommandService(IDeviceRepository deviceRepo, IStateRepository stateRepository, ICommandSender commandSender)
        {
            _deviceRepo = deviceRepo;
            _cmd = commandSender;
            _statesRepo = stateRepository;
        }

        // отправка команды
        public async Task SendCommandAsync(string entityId, string command, object? parameters = null)
        {

            var device = await _deviceRepo.GetByIdAsync(entityId);

            // устройство существует и имеет топик для команд
            if (device == null || string.IsNullOrEmpty(device.CommandTopic))
                throw new InvalidOperationException($"Device {entityId} not found or has no CommandTopic");

            var state = await _statesRepo.GetLastStateAsync(device.EntityId);

            var lastState= state?.State;

            // формируем payload в зависимости от типа устройства и команды
            string payload = BuildPayload(device, command, parameters, lastState);

            // отправляем JSON в MQTT‑топик
            await _cmd.SendCommandAsync(device.CommandTopic, payload);
        }

        // JSON‑payload
        private string BuildPayload(DeviceEntityDto device, string command, object? parameters, string lastState = "")
        {
            var cmd = command.ToLowerInvariant();
            //Console.WriteLine($"CMD: {cmd}");

            if (device.Domain.ToLower() == "light")
            {
                switch (cmd)
                {
                    case "on": 
                        return "{\"state\":\"ON\"}";
                    case "off": 
                        return "{\"state\":\"OFF\"}";
                    case "toggle":
                        if(lastState.ToLower() == "on")
                        {
                            return "{\"state\":\"OFF\"}";
                        }
                        else
                        {
                            return "{\"state\":\"ON\"}";
                        }
                        
                    // задается параметром, если он передан
                    case "brightness" when parameters != null:
                        return $"{{\"state\":\"ON\",\"brightness\":{GetBrightness(parameters)}}}";
                    // JSON – возвращаем как есть
                    default: return command;
                }
            }

            if (device.Domain.ToLower() == "switch")
            {
                switch (cmd)
                {
                    case "on":
                        return "ON";
                    case "off":
                        return "OFF";
                    case "toggle":
                        return "TOGGLE";                    
                    // JSON – возвращаем как есть
                    default: return command;
                }
            }

            // Устройства типа select
            if (device.Domain == "select")
                return command;

            // остальные типы 
            return command;
        }

        // Извлекает значение brightness из объекта параметров
        private int GetBrightness(object parameters)
        {
            var prop = parameters.GetType().GetProperty("brightness");
            if (prop != null)
                return Convert.ToInt32(prop.GetValue(parameters));

            return 128; // значение по умолчанию
        }
    }
}
