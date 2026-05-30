using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Myholas.Web.Client.Auth
{
    /// <summary>
    /// Кастомный провайдер состояния аутентификации.
    /// Этот класс отвечает за то, чтобы приложение знало: залогинен пользователь или нет, 
    /// и какие у него есть права (роли).
    /// </summary>
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _http;

        public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient http)
        {
            _localStorage = localStorage;
            _http = http;
        }

        /// <summary>
        /// Основной метод Blazor, который вызывается каждый раз, когда системе 
        /// нужно узнать текущее состояние пользователя (например, при загрузке страницы).
        /// </summary>
        /// <returns>Объект AuthenticationState, содержащий данные о пользователе.</returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Пытаемся достать токен из локального хранилища браузера
            var token = await _localStorage.GetItemAsync<string>("authToken");

            // Если токена нет или он пустой — значит пользователь не авторизован
            if (string.IsNullOrWhiteSpace(token))
            {
                // Возвращаем "пустую" личность (анонимного пользователя)
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            try
            {
                // Если токен есть, нам нужно «раскодировать» его, чтобы узнать имя и роль пользователя
                var claims = ParseClaimsFromJwt(token);

                // Создаем "личность" (Identity) на основе данных из токена
                var identity = new ClaimsIdentity(claims, "Jwt");

                // Создаем "принципала" (Principal) — это объект, который объединяет личность и права доступа
                var user = new ClaimsPrincipal(identity);

                // Возвращаем состояние: "Вот этот пользователь сейчас активен в системе"
                return new AuthenticationState(user);
            }
            catch (Exception ex) {
                Console.WriteLine($"[AuthProvicer]: error while token parsing: {ex.Message}");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        /// <summary>
        /// Метод для входа в систему. Вызывай его после того, как API вернул успешный токен.
        /// </summary>
        /// <param name="token">JWT токен, полученный от сервера</param>
        public async Task MarkUserAsAuthenticated(string token)
        {
            // 1. Сохраняем токен в браузер, чтобы пользователь не разлогинился после перезагрузки страницы
            await _localStorage.SetItemAsync("authToken", token);

            // 2. Создаем личность пользователя на основе токена
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "Jwt");
            var user = new ClaimsPrincipal(identity);

            // 3. Уведомляем всё приложение (все компоненты), что состояние пользователя изменилось.
            // Благодаря этому <AuthorizeView> мгновенно обновит интерфейс (скроет/покажет кнопки).
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        /// <summary>
        /// Метод для выхода из системы (Logout).
        /// </summary>
        public async Task MarkUserAsUnauthenticated()
        {
            // 1. Удаляем токен из локального хранилища
            await _localStorage.RemoveItemAsync("authToken");

            // 2. Уведомляем приложение, что пользователь теперь анонимный
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
        }

        /// <summary>
        /// Вспомогательный метод для разбора JWT токена.
        /// JWT состоит из трех частей, разделенных точками: Header.Payload.Signature
        /// Нам нужна вторая часть (Payload), где хранятся данные пользователя в формате JSON.
        /// </summary_
        private List<Claim> ParseClaimsFromJwt(string jwt)
        {
            // Разделяем токен по точкам и берем вторую часть (индекс 1) — Payload
            var payload = jwt.Split('.')[1];

            // Декодируем Base64 строку в массив байтов
            var jsonBytes = ParseBase64WithoutPadding(payload);

            // Превращаем байты в строку JSON и десериализуем её в словарь (ключ-значение)
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(Encoding.UTF8.GetString(jsonBytes));

            // Превращаем каждую пару "ключ-значение" из JSON в объект Claim (заявку о праве)
            // Например: "role" -> "Admin" превратится в Claim { Type: "role", Value: "Admin" }
            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())).ToList();
        }

        /// <summary>
        /// JWT токены используют Base64Url, который отличается от стандартного Base64.
        /// Этот метод восстанавливает недостающие символы '=' в конце строки, 
        /// чтобы стандартный декодер .NET смог прочитать данные.
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

        /// <summary>
        /// Вспомогательный метод для проверки текущего статуса в консоли разработчика.
        /// </summary>
        private async Task CheckStatus()
        {
            // Вызываем метод GetAuthenticationStateAsync, чтобы получить актуальные данные
            var authState = await GetAuthenticationStateAsync();
            var user = authState.User;

            // Проверяем свойство IsAuthenticated (оно станет true, если в токене есть данные)
            if (user.Identity?.IsAuthenticated == true)
            {
                Console.WriteLine($"Пользователь {user.Identity.Name} залогинен");
            }
            else
            {
                Console.WriteLine("Пользователь НЕ залогинен");
            }
        }
    }
}
