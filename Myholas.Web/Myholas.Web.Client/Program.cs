using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Myholas.Web.Client.Auth;
using Myholas.Web.Client.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http; 

namespace Myholas.Web.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            // 1. Хранилище и Базовая авторизация
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddAuthorizationCore();

            // 2. Провайдер состояния аутентификации
            // Регистрируем наш класс и связываем его со стандартным интерфейсом Blazor
            builder.Services.AddScoped<CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());

            // 3. Сетевой стек (Исправляем ошибку Inner Handler)

            // Регистрируем перехватчик токена как Transient
            builder.Services.AddTransient<JwtHandler>();

            // Регистрируем именованный HttpClient. 
            // .AddHttpMessageHandler<JwtHandler>() автоматически создаст правильную цепочку (Inner Handlers)
            builder.Services.AddHttpClient("ApiHttpClient", client =>
            {
                client.BaseAddress = new Uri("http://localhost:5174/");
            })
            .AddHttpMessageHandler<JwtHandler>();

            // 4. Регистрация ApiClient
            // Теперь ApiClient получает уже полностью настроенный HttpClient из фабрики
            builder.Services.AddScoped<ApiClient>(sp =>
            {
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = factory.CreateClient("ApiHttpClient");
                return new ApiClient(httpClient);
            });

            await builder.Build().RunAsync();
        }
    }
}
