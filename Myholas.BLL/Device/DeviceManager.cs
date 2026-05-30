using AutoMapper;
using Myholas.Core.Dtos;
using Myholas.Core.Dtos.Devices;
using Myholas.Core.Interfaces;
using Myholas.Core.Models.Input;
using Myholas.Core.Models.Output;

namespace Myholas.BLL.Device
{
    // Менеджер устройств - CRUD операции и бизнес логика
     public class DeviceManager : IDeviceManager
    {
        private readonly IDeviceRepository _deviceRepo;
        private readonly IMapper _mapper;

        public DeviceManager(IDeviceRepository deviceRepo, IMapper mapper)
        {
            _deviceRepo = deviceRepo;
            _mapper = mapper;
        }

        // Получить одну сущность по EntityId
        public async Task<EntityOutputModel?> GetEntityByIdAsync(string entityId)
        {
            var entity = await _deviceRepo.GetByEntityIdAsync(entityId);
            return _mapper.Map<EntityOutputModel>(entity);
        }

        // Получить все датчики всех устройств плоским списком
        public async Task<List<EntityOutputModel>> GetAllEntitiesAsync(bool includeUnavailable = false)
        {
            var devices = await _deviceRepo.GetAllDevicesAsync(includeUnavailable);

            // Разворачиваем список устройств в один плоский список всех сущностей
            var allEntities = devices.SelectMany(d => d.Entities).ToList();

            return _mapper.Map<List<EntityOutputModel>>(allEntities);
        }

        // Получить сущности, отфильтрованные по домену (switch, sensor и т.д.)
        public async Task<List<EntityOutputModel>> GetEntitiesByDomainAsync(string domain)
        {
            var entities = await _deviceRepo.GetByDomainAsync(domain);
            return _mapper.Map<List<EntityOutputModel>>(entities);
        }

        // Добавить или обновить устройство и его сущность
        // Принимает Input-модели, маппит их в DTO для репозитория
        public async Task<EntityDto> AddOrUpdateAsync(DeviceDtoInputModel deviceInput, EntityDtoInputModel entityInput)
        {
            // Валидация обязательного поля
            if (string.IsNullOrWhiteSpace(entityInput.EntityId))
                throw new ArgumentException("EntityId is required");

            // Маппинг из Input-моделей в DTO базы данных
            var deviceDto = _mapper.Map<DeviceDto>(deviceInput);
            var entityDto = _mapper.Map<EntityDto>(entityInput);

            return await _deviceRepo.AddOrUpdateEntityAsync(deviceDto, entityDto);
        }

        // Получить сгруппированные данные (Физическое устройство -> Список сущностей)
        public async Task<List<DeviceOutputModel>> GetGroupedDevicesAsync()
        {
            var devices = await _deviceRepo.GetAllDevicesAsync(true);

            // AutoMapper автоматически маппит DeviceDto -> DeviceOutputModel 
            // и вложенную коллекцию Entities -> List<EntityOutputModel>
            return _mapper.Map<List<DeviceOutputModel>>(devices);
        }

        // Удалить сущность по EntityId (каскадно удалит историю и автоматизации)
        public async Task<bool> DeleteAsync(string entityId)
        {
            return await _deviceRepo.DeleteEntityAsync(entityId);
        }

        // Проверить существование сущности по EntityId
        public async Task<bool> ExistsAsync(string entityId)
        {
            return await _deviceRepo.ExistsAsync(entityId);
        }

        // Получить все сущности, привязанные к конкретному физическому DeviceId
        public async Task<List<EntityOutputModel>> GetEntitiesByDeviceIdAsync(string deviceId)
        {
            var device = await _deviceRepo.GetByDeviceIdAsync(deviceId);

            if (device == null)
                return new List<EntityOutputModel>();

            return _mapper.Map<List<EntityOutputModel>>(device.Entities);
        }
    }
}
