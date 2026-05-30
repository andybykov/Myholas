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
            // 1. Теперь ищем СУЩНОСТЬ (EntityDto), а не устройство
            var entity = await _deviceRepo.GetByEntityIdAsync(entityId);

            // Проверяем, существует ли сущность и есть ли у неё топик для команд
            if (entity == null || string.IsNullOrEmpty(entity.CommandTopic))
                throw new InvalidOperationException($"Entity {entityId} not found or has no CommandTopic");

            // 2. Получаем последнее состояние для реализации логики TOGGLE
            // Наш новый GetLastStateAsync принимает string entityId
            var state = await _statesRepo.GetLastStateAsync(entityId);
            var lastState = state?.State;

            // 3. Формируем payload в зависимости от домена сущности
            string payload = BuildPayload(entity, command, parameters, lastState);

            // 4. Отправляем в MQTT-топик
            await _cmd.SendCommandAsync(entity.CommandTopic, payload);
        }

        // Логика формирования сообщения (JSON для света, String для остальных)
        private string BuildPayload(EntityDto entity, string command, object? parameters, string? lastState = "")
        {
            var cmd = command.ToLowerInvariant();

            // ЛОГИКА ДЛЯ СВЕТА (Light) -> Всегда JSON
            if (entity.Domain.ToLower() == "light")
            {
                switch (cmd)
                {
                    case "on":
                        return "{\"state\":\"ON\"}";
                    case "off":
                        return "{\"state\":\"OFF\"}";
                    case "toggle":
                        // Если текущее состояние ON -> шлем OFF, и наоборот
                        return (lastState?.ToLower() == "on")
                            ? "{\"state\":\"OFF\"}"
                            : "{\"state\":\"ON\"}";

                    case "brightness" when parameters != null:
                        return $"{{\"state\":\"ON\",\"brightness\":{GetBrightness(parameters)}}}";

                    default:
                        return command; // Если пришла специфическая команда, шлем её как есть
                }
            }

            // ЛОГИКА ДЛЯ ВЫКЛЮЧАТЕЛЯ (Switch) -> Простой текст
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

            // Для select и остальных типов отправляем команду как есть
            return command;
        }

        private string GetBrightness(object parameters)
        {
            // Логика извлечения числа яркости из объекта параметров
            return parameters?.ToString() ?? "100";
        }
    }
}
