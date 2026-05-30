using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace Myholas.Web.Client.Auth
{
    // Автоматическое добавление токена в заголовок
    // Чтобы не писать Authorization: Bearer...в каждом методе ApiClient...
    public class JwtHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;

        private readonly NavigationManager _navigationManager;

        public JwtHandler(ILocalStorageService localStorage, NavigationManager navigationManager)
        {

            _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
            _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // запрос дальше по цепочке
            var response = await base.SendAsync(request, cancellationToken);

            // ОТВЕТ СЕРВЕРА
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 401
            {
                // Очищаем токен
                await _localStorage.RemoveItemAsync("authToken");

                // redirect to 401 page
                _navigationManager.NavigateTo("/unauthorized");
            }

            return response;
        }
    }
}
