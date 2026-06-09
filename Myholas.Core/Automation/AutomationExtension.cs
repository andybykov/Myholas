using Myholas.Core.Dtos.Automations;
using System.Text.Json;

namespace Myholas.Core.Automation
{

    // Для работы с AutomationEntityDto

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


        // Получить список AutomationActionDto
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