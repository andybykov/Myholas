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


        protected override async Task OnInitializedAsync()
        {
            await CheckAuthentication();
            await base.OnInitializedAsync();
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
                    // Запрашиваем полные данные пользователя из API
                    var userData = await Api.GetUserByUsernameAsync(username);
                    if (userData != null)
                    {
                        CurrentUser = userData;
                        // Проверяем роль
                        IsAdmin = userData.Role.ToString() == "Admin";
                    }
                }
            }
        }
    }
}
