using Myholas.Core.Dtos.Automations;
using Myholas.Core.Dtos.Devices;
using Myholas.Core.Models.Input;
using Myholas.Core.Models.Output;
using System.Net.Http.Json;

namespace Myholas.Web.Client.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;
        public ApiClient(HttpClient http) => _http = http;

        //Devices
        public async Task<List<EntityOutputModel>> GetEntitiesAsync(bool includeUnavailable = false) =>
            await _http.GetFromJsonAsync<List<EntityOutputModel>>($"/api/devices/entities?includeUnavailable={includeUnavailable}") ?? new();

        public async Task<EntityOutputModel?> GetEntityAsync(string entityId) =>
            await _http.GetFromJsonAsync<EntityOutputModel>($"/api/devices/entities/{entityId}");

        public async Task<List<EntityOutputModel>> GetByDomainAsync(string domain) =>
            await _http.GetFromJsonAsync<List<EntityOutputModel>>($"/api/devices/entities/by-domain/{domain}") ?? new();

        public async Task<List<EntityOutputModel>> GetByDeviceAsync(string deviceId) =>
            await _http.GetFromJsonAsync<List<EntityOutputModel>>($"/api/devices/entities/by-device/{deviceId}") ?? new();

        public async Task<List<DeviceOutputModel>> GetGroupedDevicesAsync() =>
            await _http.GetFromJsonAsync<List<DeviceOutputModel>>("/api/devices/groups") ?? new();

        public async Task<bool> DeleteEntityAsync(string entityId) =>
            (await _http.DeleteAsync($"/api/devices/{entityId}")).IsSuccessStatusCode;

        public async Task<EntityDto?> SaveDeviceAsync(DeviceEntityRequestModel request)
        {
            var response = await _http.PostAsJsonAsync("/api/devices", request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<EntityDto>() : null;
        }

        // States
        public async Task<EntityOutputModel?> GetStateAsync(string entityId) =>
            await _http.GetFromJsonAsync<EntityOutputModel>($"/api/states/{entityId}/current");

        public async Task<List<DeviceHistoryOutputModel>> GetHistoryAsync(string entityId, DateTime? from = null, DateTime? to = null, int limit = 100)
        {
            var query = $"/api/states/{entityId}/history?limit={limit}";
            if (from.HasValue) query += $"&from={from.Value:O}";
            if (to.HasValue) query += $"&to={to.Value:O}";

            return await _http.GetFromJsonAsync<List<DeviceHistoryOutputModel>>(query) ?? new();
        }

        public async Task SendCmdAsync(string entityId, string command, object? parameters = null)
        {
            if (parameters != null && command == "brightness")
                await _http.PostAsJsonAsync($"/api/states/{entityId}/brightness", parameters);
            else
                await _http.PostAsJsonAsync($"/api/states/{entityId}/command", command);
        }

        public async Task TurnOnAsync(string entityId) => 
            await SendCmdAsync(entityId, "on");

        public async Task TurnOffAsync(string entityId) => 
            await SendCmdAsync(entityId, "off");

        public async Task ToggleAsync(string entityId) => 
            await SendCmdAsync(entityId, "toggle");

        public async Task SetBrightnessAsync(string entityId, int brightness) => 
            await SendCmdAsync(entityId, "brightness", new { brightness });

        /// --- Automations ---

        // Теперь получаем OutputModel (с распарсенными триггерами и именами)
        public async Task<List<AutomationOutputModel>> GetAutomationsAsync(bool includeDisabled = false) =>
            await _http.GetFromJsonAsync<List<AutomationOutputModel>>($"/api/automations?includeDisabled={includeDisabled}") ?? new();

        public async Task<AutomationOutputModel?> GetAutomationAsync(int id) =>
            await _http.GetFromJsonAsync<AutomationOutputModel>($"/api/automations/{id}");

        // ОТПРАВЛЯЕМ InputModel (со строковым EntityId)
        public async Task<AutomationOutputModel?> CreateAutomationAsync(AutomationInputModel dto)
        {
            var response = await _http.PostAsJsonAsync("/api/automations", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AutomationOutputModel>();
        }

        public async Task<AutomationOutputModel?> UpdateAutomationAsync(AutomationInputModel dto)
        {
            var response = await _http.PutAsJsonAsync($"/api/automations/{dto.Id}", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AutomationOutputModel>();
        }

        public async Task<bool> DeleteAutomationAsync(int id) =>
            (await _http.DeleteAsync($"/api/automations/{id}")).IsSuccessStatusCode;

        public async Task<bool> ToggleAutomationAsync(int id, bool enabled) =>
            (await _http.PatchAsync($"/api/automations/{id}/toggle?enabled={enabled}", null)).IsSuccessStatusCode;


        // Auth
        public async Task<string?> LoginAsync(string username, string password)
        {
            var response = await _http.PostAsJsonAsync("/api/Auth/login", new { username, password });
            if (!response.IsSuccessStatusCode) return null;
            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            return result?.Token;
        }

        // Users
        public async Task<UserEntityOutputModel?> GetUserAsync(int id) =>
            await _http.GetFromJsonAsync<UserEntityOutputModel>($"/api/Users/{id}");

        public async Task<UserEntityOutputModel?> GetUserByNameAsync(string username) =>
            await _http.GetFromJsonAsync<UserEntityOutputModel>($"/api/Users/by-username/{username}");

        public async Task<bool> CreateUserAsync(UserEntityInputModel user) =>
            (await _http.PostAsJsonAsync("/api/Users", user)).IsSuccessStatusCode;

        public async Task<bool> UpdatePasswordAsync(UserLoginInputModel model) =>
            (await _http.PutAsJsonAsync("/api/Users/update/password", model)).IsSuccessStatusCode;
    }

    // Обертка для POST-запросов
    public class DeviceEntityRequestModel
    {
        public DeviceDtoInputModel Device { get; set; } = null!;
        public EntityDtoInputModel Entity { get; set; } = null!;
    }
}
