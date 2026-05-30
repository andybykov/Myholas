using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Myholas.Core.Models.Output;
using Myholas.Web.Client.Services;

namespace Myholas.Web.Client.Auth
{
    public class SecureComponentBase : ComponentBase
    {
        [Inject] protected CustomAuthStateProvider AuthProvider { get; set; } = default!;

        [Inject] protected ApiClient Api { get; set; } = default!;


        // Свойства доступны на всех страницах наследующих этот класс
        public UserEntityOutputModel? CurrentUser { get; private set; }

        public bool IsAuthenticated { get; private set; }

        public bool IsAdmin { get; private set; }

        // страница загружается
        public bool IsLoading { get; private set; } = true;


        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            try
            {
                await CheckAuthentication();
                await base.OnInitializedAsync();
            }
            finally
            {
                IsLoading = false; // загрузка завершена
            }
        }

        protected virtual async Task CheckAuthentication()
        {  
            var authState = await AuthProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            IsAuthenticated = user.Identity?.IsAuthenticated == true;

            if (IsAuthenticated)
            {
                var username = user.Identity?.Name;
                if (!string.IsNullOrEmpty(username))
                {
                    try
                    {
                        // Запрашиваем полные данные пользователя из API
                        var userData = await Api.GetUserByUsernameAsync(username);
                        if (userData != null)
                        {
                            CurrentUser = userData;
                            // Проверяем роль
                            IsAdmin = userData.Role.ToString() == "Admin";
                        }
                    }
                    catch (Exception e)
                    {
                        // Если API выбросил исключение (например, из-за 401), 
                        // JwtHandler уже сделал редирект, но здесь можно добавить лог.
                        Console.WriteLine($"[SECURE]: Exception {e}");
                    }
                }
            }
        }
    }
}
