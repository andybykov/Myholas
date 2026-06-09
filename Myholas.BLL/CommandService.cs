using Myholas.Core.Dtos;
using Myholas.Core.Dtos.Devices;
using Myholas.Core.Interfaces;

namespace Myholas.BLL.Device
{
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
            //  ищем EntityDto
            var entity = await _deviceRepo.GetByEntityIdAsync(entityId);

            //  существует ли сущность и есть 
            if (entity == null || string.IsNullOrEmpty(entity.CommandTopic))
                throw new InvalidOperationException($"Entity {entityId} not found or has no CommandTopic");

            //  Получаем последнее состояние 
             var state = await _statesRepo.GetLastStateAsync(entityId);
            var lastState = state?.State;

            //  Формируем payload 
            string payload = BuildPayload(entity, command, parameters, lastState);

            //Отправляем в MQTT
            await _cmd.SendCommandAsync(entity.CommandTopic, payload);
        }

        // Логика формирования сообщения 
        private string BuildPayload(EntityDto entity, string command, object? parameters, string? lastState = "")
        {
            var cmd = command.ToLowerInvariant();

            //  Всегда JSON
            if (entity.Domain.ToLower() == "light")
            {
                switch (cmd)
                {
                    case "on":
                        return "{\"state\":\"ON\"}";
                    case "off":
                        return "{\"state\":\"OFF\"}";
                    case "toggle":
                       
                        return (lastState?.ToLower() == "on")
                            ? "{\"state\":\"OFF\"}"
                            : "{\"state\":\"ON\"}";

                    case "brightness" when parameters != null:
                        return $"{{\"state\":\"ON\",\"brightness\":{GetBrightness(parameters)}}}";

                    default:
                        return command; 
                }
            }

            //  Простой текст
            if (entity.Domain.ToLower() == "switch")
            {
                switch (cmd)
                {
                    case "on":
                        return "ON";
                    case "off":
                        return "OFF";
                    case "toggle":
                        return "TOGGLE";
                    default:
                        return command;
                }
            }

            //  как есть
            return command;
        }

        private string GetBrightness(object parameters)
        {
            return parameters?.ToString() ?? "100";
        }
    }
}
