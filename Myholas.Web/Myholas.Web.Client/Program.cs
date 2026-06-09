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

     
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddAuthorizationCore();

            //Провайдер состояния аутентификации
            builder.Services.AddScoped<CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());

           

            //перехватчик токена как Transient
            builder.Services.AddTransient<JwtHandler>();

            //  HttpClient. 
            // .AddHttpMessageHandler<JwtHandler>() 
            builder.Services.AddHttpClient("ApiHttpClient", client =>
            {
                client.BaseAddress = new Uri("http://localhost:5174/");
            })
            .AddHttpMessageHandler<JwtHandler>();

            //  ApiClient
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
