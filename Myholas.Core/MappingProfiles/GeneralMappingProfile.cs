using AutoMapper;
using Myholas.Core.Automation;
using Myholas.Core.Dtos.Automations;
using Myholas.Core.Dtos.Devices;
using Myholas.Core.Dtos.Users;
using Myholas.Core.Models.Input;
using Myholas.Core.Models.Output;

namespace Myholas.Core.MappingProfiles
{
    public class GeneralMappingProfile : Profile
    {
        public GeneralMappingProfile()
        {
            // Устройство
            // DeviceDto to EntityOutputModel 
            CreateMap<DeviceDto, DeviceOutputModel>()
                .ForMember(dest => dest.DeviceId, opt => opt.MapFrom(src => src.DeviceId))
                .ForMember(dest => dest.FriendlyName, opt => opt.MapFrom(src => src.FriendlyName))
                .ForMember(dest => dest.Ip, opt => opt.MapFrom(src => src.IpAddress))
                .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.Version))
                .ForMember(dest => dest.IsOnline, opt => opt.MapFrom(src => src.IsOnline))
                .ForMember(dest => dest.Entities, opt => opt.MapFrom(src => src.Entities));

            // DeviceDtoInputModel to DeviceDto
            CreateMap<DeviceDtoInputModel, DeviceDto>()
                .ForMember(dest => dest.DeviceId, opt => opt.MapFrom(src => src.DeviceId))
                .ForMember(dest => dest.FriendlyName, opt => opt.MapFrom(src => src.FriendlyName));
           

            // конкретная сущность
            // EntityDto to EntityOutputModel
            CreateMap<EntityDto, EntityOutputModel>()
                .ForMember(dest => dest.EntityId, opt => opt.MapFrom(src => src.EntityId))
                .ForMember(dest => dest.Domain, opt => opt.MapFrom(src => src.Domain))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FriendlyName ?? src.EntityId))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.CurrentState))
                .ForMember(dest => dest.UnitOfMeasurement, opt => opt.MapFrom(src => src.UnitOfMeasurement))
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => ExtractOptionsFromAttributes(src.AttributesJson)))
                .ForMember(dest => dest.IsOn, opt => opt.MapFrom(src =>
                    (src.Domain == "switch" || src.Domain == "light") &&
                    src.CurrentState.Equals("on", StringComparison.OrdinalIgnoreCase)));

            // EntityDtoInputModel to EntityDto
            CreateMap<EntityDtoInputModel, EntityDto>()
                .ForMember(dest => dest.EntityId, opt => opt.MapFrom(src => src.EntityId))
                .ForMember(dest => dest.Domain, opt => opt.MapFrom(src => src.Domain))
                .ForMember(dest => dest.FriendlyName, opt => opt.MapFrom(src => src.FriendlyName ?? src.EntityId));


            // StateEntityDto to DeviceHistoryOutputModel
            // История состояний
            CreateMap<StateEntityDto, DeviceHistoryOutputModel>()
                .ForMember(dest => dest.EntityId, opt => opt.MapFrom(src => src.Entity.EntityId))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.AttributesSummary, opt => opt.MapFrom(src => SummarizeAttributes(src.AttributesJson)));

            // UserEntityDto to UserEntityOutputModel
            CreateMap<UserEntityDto, UserEntityOutputModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom((src) => src.IsActive))
                .ForMember(dest => dest.LastLogin, opt => opt.MapFrom(src => src.LastLogin))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

            // UserEntityInputModel to UserEntityDto
            CreateMap<UserEntityInputModel, UserEntityDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            // UserLoginInputModel to UserEntityInputModel
            CreateMap<UserLoginInputModel, UserEntityInputModel>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password));

            // Автоматизации
            //AutomationInputModel to AutomationEntityDto
            CreateMap<AutomationInputModel, AutomationEntityDto>()
                .ForMember(dest => dest.EntityId, opt => opt.Ignore()) // IGNORE
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedByUser, opt => opt.MapFrom(src => src.CreatedByUser));

            // AutomationEntityDto to AutomationOutputModel
            CreateMap<AutomationEntityDto, AutomationOutputModel>()
            .ForMember(dest => dest.EntityId, opt => opt.MapFrom(src => src.Entity.EntityId))
            .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser.Username ?? "hello"))
            //  Парсим JSON-строки в списки объектов, методы из AutomationExtension
            .ForMember(dest => dest.Triggers, opt => opt.MapFrom(src => src.GetTriggers()))
            .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.GetConditions()))
            .ForMember(dest => dest.Actions, opt => opt.MapFrom(src => src.GetActions()));


        }



        // Вспомогательные методы         
        private static List<string>? ExtractOptionsFromAttributes(string? attributesJson)
        {
            if (string.IsNullOrEmpty(attributesJson)) return null;
            using var doc = System.Text.Json.JsonDocument.Parse(attributesJson);
            if (doc.RootElement.TryGetProperty("options", out var opts) && opts.ValueKind == System.Text.Json.JsonValueKind.Array)
                return opts.EnumerateArray().Select(x => x.GetString()!).ToList();
            return null;
        }

        private static string? SummarizeAttributes(string? attributesJson)
        {
            if (string.IsNullOrEmpty(attributesJson)) return null;
            using var doc = System.Text.Json.JsonDocument.Parse(attributesJson);
            var props = doc.RootElement.EnumerateObject().Select(p => $"{p.Name}: {p.Value}");

            return string.Join(", ", props);
        }
    }
}


