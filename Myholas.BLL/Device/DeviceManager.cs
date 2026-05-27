using AutoMapper;
using Myholas.Core.Dtos;
using Myholas.Core.Interfaces;
using Myholas.Core.Models.Output;

namespace Myholas.BLL.Device
{

    // Менеджер устройств - CRUD операции и бизнес-логика для DeviceEntityDto
    public class DeviceManager : IDeviceManager
    {
        private readonly IDeviceRepository _deviceRepo;

        private readonly IMapper _mapper;


        public DeviceManager(IDeviceRepository deviceRepo, IMapper mapper)
        {
            _deviceRepo = deviceRepo;
            _mapper = mapper;
        }

        // Одна сущность по EntityId
        public async Task<EntityOutputModel?> GetEntityByIdAsync(string entityId)
        {
            var device = await _deviceRepo.GetByIdAsync(entityId);
     
            return _mapper.Map<EntityOutputModel>(device);
        }

        // Все сущности 
        public async Task<List<EntityOutputModel>> GetAllEntitiesAsync(bool includeUnavailable = false)
        {
            var devices = await _deviceRepo.GetAllAsync(includeUnavailable); 

            return _mapper.Map<List<EntityOutputModel>>(devices);
        }

        // CRUD

        // Добавить новое устройство или обновить существующее        
        public async Task<DeviceEntityDto> AddOrUpdateAsync(DeviceEntityDto entity)
        {
            await Task.Delay(100);
            if (string.IsNullOrWhiteSpace(entity.EntityId))
                throw new ArgumentException("EntityId is required");


            return await _deviceRepo.AddOrUpdateAsync(entity);
        }

        // Группированные устройства
        public async Task<List<DeviceOutputModels>> GetGroupedDevicesAsync()
        {
            List<DeviceEntityDto> devices = await _deviceRepo.GetAllAsync(true);
            var groups = devices.GroupBy(d => d.DeviceId);
            var result = new List<DeviceOutputModels>();

            foreach (var grp in groups)
            {
                var first = grp.First();
                var group = new DeviceOutputModels
                {
                    DeviceId = grp.Key,
                    FriendlyName = first.FriendlyName,
                    Ip = first.IpAdress,
                    IsOnline = grp.Any(d => d.IsAvailable),
                    LastSeen = grp.Max(d => d.LastSeen),
                    Version = null,
                    Entities = _mapper.Map<List<EntityOutputModel>>(grp.ToList())
                };
                result.Add(group);
            }         

            return result;
        }


        // Удалить устройство по EntityId 
        public async Task<bool> DeleteAsync(string entityId)
        {
            return await _deviceRepo.DeleteAsync(entityId);
        }

        // Проверить существование устройства
        public async Task<bool> ExistsAsync(string entityId)
        {
            return await _deviceRepo.ExistsAsync(entityId);
        }

        // Сущности по домену
        public async Task<List<EntityOutputModel>> GetEntitiesByDomainAsync(string domain)
        {
            var devices = await _deviceRepo.GetByDomainAsync(domain);
        
            return _mapper.Map<List<EntityOutputModel>>(devices);
        }

        // Сущности по физическому DeviceId
        public async Task<List<EntityOutputModel>> GetEntitiesByDeviceIdAsync(string deviceId)
        {
            var devices = await _deviceRepo.GetByDeviceIdAsync(deviceId);           

            return _mapper.Map<List<EntityOutputModel>>(devices);
        }
    }
}