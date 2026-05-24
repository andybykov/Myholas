using Myholas.Web.Client.Services;
using Myholas.Web.Components;

namespace Myholas.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Blazor компоненты
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            // HTTPCLIENT ДЛЯ API 
            builder.Services.AddHttpClient("ApiHttpClient", client =>
            {
                client.BaseAddress = new Uri("http://localhost:5174/");
            });

            // Регистрируем 
            builder.Services.AddScoped<ApiClient>(sp =>
            {
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = factory.CreateClient("ApiHttpClient");
                return new ApiClient(httpClient);
            });
            

            var app = builder.Build();

            // Статические файлы 
            app.UseStaticFiles();

            // Антифоргерийный токен
            app.UseAntiforgery();

            // Маршруты Blazor
            app.MapRazorComponents<App>()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Myholas.Web.Client._Imports).Assembly);

            app.Run();
        }
    }
}