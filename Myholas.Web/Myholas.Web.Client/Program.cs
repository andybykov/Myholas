using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Myholas.Web.Client.Services;

namespace Myholas.Web.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            //  HttpClient 
            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5174/")
            });

            // Регистрируем
            builder.Services.AddScoped<ApiClient>();

            await builder.Build().RunAsync();
        }
    }
}