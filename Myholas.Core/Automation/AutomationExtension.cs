using Myholas.Core.Dtos;
using System.Text.Json;

namespace Myholas.Core.Automation
{
    /// <summary>
    /// Для работы с AutomationEntityDto
    /// 
    /// DTO хранит automation в JSON:
    /// - TriggersJson
    /// - ConditionsJson
    /// - ActionsJson
    ///
    ///  AutomationService работает с
    /// - List<AutomationTrigger>
    /// - List<AutomationCondition>
    /// - List<AutomationAction>
    /// </summary>
    public static class AutomationExtension
    {

        // Получить список AutomationTriggerDto

        public static List<AutomationTriggerDto> GetTriggers(this AutomationEntityDto dto)
        {
            //  JSON пустой
            if (string.IsNullOrWhiteSpace(dto.TriggersJson))

                return new List<AutomationTriggerDto>();

            // Десериализуем JSON 
            return JsonSerializer.Deserialize<List<AutomationTriggerDto>>(dto.TriggersJson)
                ?? new List<AutomationTriggerDto>();
        }


        /// Получить список AutomationActionDto
        public static List<AutomationActionDto> GetActions(this AutomationEntityDto dto)
        {
            //  JSON пустой
            if (string.IsNullOrWhiteSpace(dto.ActionsJson))
                return new List<AutomationActionDto>();

            // Десериализуем JSON 
            return JsonSerializer.Deserialize<List<AutomationActionDto>>(dto.ActionsJson) 
                ?? new List<AutomationActionDto>();
        }


        // Получить список conditions automation
        
        public static List<AutomationConditionDto> GetConditions(this AutomationEntityDto dto)
        {
            // Conditions необязательны
            if (string.IsNullOrWhiteSpace(dto.ConditionsJson))
            {
                return new List<AutomationConditionDto>();
            }

            // Десериализуем JSON
            return JsonSerializer.Deserialize<List<AutomationConditionDto>>(dto.ConditionsJson) 
                ?? new List<AutomationConditionDto>();
        }
    }
}