using System.Net.Http.Json;
using Myholas.Core.Models.Output;
using Myholas.Core.Dtos;

namespace Myholas.Web.Client.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;

        public ApiClient(HttpClient http) => _http = http;


        // DevicesController
        public async Task<List<EntityOutputModel>> GetAllEntitiesAsync(bool includeUnavailable = false)
        {
            var url = $"/api/devices/entities?includeUnavailable={includeUnavailable}";
            return await _http.GetFromJsonAsync<List<EntityOutputModel>>(url) ?? new();
        }

        public async Task<EntityOutputModel?> GetEntityByIdAsync(string entityId) =>
            await _http.GetFromJsonAsync<EntityOutputModel>($"/api/devices/entities/{entityId}");

        public async Task<List<EntityOutputModel>> GetEntitiesByDomainAsync(string domain) =>
            await _http.GetFromJsonAsync<List<EntityOutputModel>>($"/api/devices/entities/by-domain/{domain}") ?? new();

        public async Task<List<EntityOutputModel>> GetEntitiesByDeviceIdAsync(string deviceId) =>
            await _http.GetFromJsonAsync<List<EntityOutputModel>>($"/api/devices/entities/by-device/{deviceId}") ?? new();

        public async Task<List<DeviceOutputModels>> GetGroupedDevicesAsync() =>
            await _http.GetFromJsonAsync<List<DeviceOutputModels>>("/api/devices/groups") ?? new();

        public async Task<bool> DeleteDeviceAsync(string entityId)
        {
            var response = await _http.DeleteAsync($"/api/devices/{entityId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<DeviceEntityDto?> AddOrUpdateDeviceAsync(DeviceEntityDto device)
        {
            var response = await _http.PostAsJsonAsync("/api/devices", device);
            return await response.Content.ReadFromJsonAsync<DeviceEntityDto>();
        }


        // StatesController

        public async Task<EntityOutputModel?> GetCurrentStateAsync(string entityId) =>
            await _http.GetFromJsonAsync<EntityOutputModel>($"/api/states/{entityId}/current");

        public async Task<List<DeviceHistoryOutputModel>> GetHistoryAsync(
            string entityId,
            DateTime? from = null,
            DateTime? to = null,
            int limit = 100)
        {
            var query = $"/api/states/{entityId}/history?limit={limit}";
            if (from.HasValue) query += $"&from={from.Value:O}";
            if (to.HasValue) query += $"&to={to.Value:O}";
            return await _http.GetFromJsonAsync<List<DeviceHistoryOutputModel>>(query) ?? new();
        }

        public async Task SendCommandAsync(string entityId, string command, object? parameters = null)
        {
            if (parameters != null && command == "brightness")
                await _http.PostAsJsonAsync($"/api/states/{entityId}/brightness", parameters);
            else
                await _http.PostAsJsonAsync($"/api/states/{entityId}/command", command);
        }

        public async Task TurnOnAsync(string entityId) => await SendCommandAsync(entityId, "on");

        public async Task TurnOffAsync(string entityId) => await SendCommandAsync(entityId, "off");

        public async Task ToggleAsync(string entityId) => await SendCommandAsync(entityId, "toggle");

        public async Task SetBrightnessAsync(string entityId, int brightness) =>
            await SendCommandAsync(entityId, "brightness", new { brightness });


        // AutomationsController
        public async Task<List<AutomationEntityDto>> GetAutomationsAsync(bool includeDisabled = false)
        {
            var url = $"/api/automations?includeDisabled={includeDisabled}";
            return await _http.GetFromJsonAsync<List<AutomationEntityDto>>(url) ?? new();
        }

        public async Task<AutomationEntityDto?> GetAutomationAsync(int id) =>
            await _http.GetFromJsonAsync<AutomationEntityDto>($"/api/automations/{id}");

        public async Task<AutomationEntityDto?> CreateAutomationAsync(AutomationEntityDto dto)
        {
            var response = await _http.PostAsJsonAsync("/api/automations", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AutomationEntityDto>();
        }

        public async Task<AutomationEntityDto?> UpdateAutomationAsync(AutomationEntityDto dto)
        {
            var response = await _http.PutAsJsonAsync($"/api/automations/{dto.Id}", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AutomationEntityDto>();
        }

        public async Task<bool> DeleteAutomationAsync(int id)
        {
            var response = await _http.DeleteAsync($"/api/automations/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ToggleAutomationAsync(int id, bool enabled)
        {
            var response = await _http.PatchAsync(
                $"/api/automations/{id}/toggle?enabled={enabled}",
                null);
            return response.IsSuccessStatusCode;
        }
    }
}
