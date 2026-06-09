using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication; 
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens; 
using Myholas.Web.Client.Auth;
using Myholas.Web.Client.Services;
using Myholas.Web.Components;
using System.Text;

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

           
            // Настройка авторизации на сервере 
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // те же настройки что и в API
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true, // validate JWT token
                        ValidIssuer = builder.Configuration["MyholasServer"],
                        ValidAudience = builder.Configuration["MyholasClient"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("mysupersecretkey_32bytes_long!!!!!"))
                    };
                });
            

            // HTTPCLIENT ДЛЯ API 
            builder.Services.AddHttpClient("ApiHttpClient", client =>
            {
                client.BaseAddress = new Uri("http://localhost:5174/");
            });

            builder.Services.AddScoped<ApiClient>(sp =>
            {
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = factory.CreateClient("ApiHttpClient");
                return new ApiClient(httpClient);
            });

            var app = builder.Build();

            // Статические файлы 
            app.UseStaticFiles();

            app.UseAntiforgery();

            
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapRazorComponents<App>()
                .AddInteractiveWebAssemblyRenderMode() //CLIENT!!!!!!!
                .AddInteractiveServerRenderMode()
                .AddAdditionalAssemblies(typeof(Myholas.Web.Client._Imports).Assembly);

            app.Run();
        }
    }
}
