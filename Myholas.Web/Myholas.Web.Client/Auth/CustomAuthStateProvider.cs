using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Myholas.Web.Client.Auth
{

    // Кастомный провайдер состояния аутентификации
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _http;

        public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient http)
        {
            _localStorage = localStorage;
            _http = http;
        }

        // Состояние аутентификации
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // токен из локального хранилища
            var token = await _localStorage.GetItemAsync<string>("authToken");


            if (string.IsNullOrWhiteSpace(token))
            {
                // аноними
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            try
            {
               
                var claims = ParseClaimsFromJwt(token);

                // Identity на основе данных из токена
                var identity = new ClaimsIdentity(claims, "Jwt");

                // Principal объединяет личность и права доступа
                var user = new ClaimsPrincipal(identity);

                
                return new AuthenticationState(user);
            }
            catch (Exception ex) {
                Console.WriteLine($"[AuthProvicer]: error while token parsing: {ex.Message}");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }


        // Метод для входа в систему     

        public async Task MarkUserAsAuthenticated(string token)
        {
            // токен в браузер
            await _localStorage.SetItemAsync("authToken", token);

            //  личность пользователя на основе токена
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "Jwt");
            var user = new ClaimsPrincipal(identity);

            // Уведомляем  приложение
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }


        // Logout
        public async Task MarkUserAsUnauthenticated()
        {
            // Удаляем токен 
            await _localStorage.RemoveItemAsync("authToken");

            //  Уведомляем приложение
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
        }

        /// <summary>
        /// Вспомогательный метод для разбора JWT токена
        /// JWT состоит из трех частей: Header.Payload.Signature      
        /// </summary_
        private List<Claim> ParseClaimsFromJwt(string jwt)
        {

            var payload = jwt.Split('.')[1];

            // Декодируем Base64 
            var jsonBytes = ParseBase64WithoutPadding(payload);

            // байты в строку JSON 
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(Encoding.UTF8.GetString(jsonBytes));

            // ключ-значение из JSON в объект Claim

            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())).ToList();
        }

        /// <summary>
        /// JWT токены используют Base64Url
        /// метод добавляет недостающие символы = в конце строки
        /// </summary>
        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }        
    }
}
