using AutoMapper;
using Myholas.Core.Dtos;
using Myholas.Core.Models.Input;
using Myholas.Core.Models.Output;

namespace Myholas.Core.MappingProfiles
{
    public class GeneralMappingProfile : Profile
    {
        public GeneralMappingProfile()
        {
            // DeviceEntityDto to EntityOutputModel 

                CreateMap<DeviceEntityDto, EntityOutputModel>()
                .ForMember(dest => dest.EntityId, opt => opt.MapFrom(src => src.EntityId))
                .ForMember(dest => dest.Domain, opt => opt.MapFrom(src => src.Domain))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FriendlyName ?? src.EntityId))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.CurrentState))
                .ForMember(dest => dest.LastSeen, opt => opt.MapFrom(src => src.LastSeen))
                .ForMember(dest => dest.UnitOfMeasurement, opt => opt.MapFrom(src => src.UnitOfMeasurement))
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => ExtractOptionsFromAttributes(src.AttributesJson)))
                // string to bool? / switch/light
                .ForMember(dest => dest.IsOn, opt => opt.MapFrom(src =>
                (src.Domain == "switch" || src.Domain == "light") && src.CurrentState.Equals("on", StringComparison.OrdinalIgnoreCase)));


            // StateEntityDto to DeviceHistoryOutputModel
            CreateMap<StateEntityDto, DeviceHistoryOutputModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.EntityId, opt => opt.MapFrom(src => src.EntityId))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                // for del!
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


