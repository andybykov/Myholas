using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Myholas.Core.Models.Output;
using Myholas.Web.Client.Services;

namespace Myholas.Web.Client.Auth
{
    public class SecureComponentBase : ComponentBase
    {
        [Inject] protected CustomAuthStateProvider AuthProvider { get; set; } = default!;

        [Inject] protected ApiClient Api { get; set; } = default!;

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
                        var userData = await Api.GetUserByNameAsync(username);
                        if (userData != null)
                        {
                            CurrentUser = userData;
                            IsAdmin = userData.Role.ToString() == "Admin";
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"[SECURE]: Auth failed: {e.Message}");

                        //401 при проверке..                        
                        IsAuthenticated = false;
                 
                        await AuthProvider.MarkUserAsUnauthenticated();
                        
                    }
                }
            }
        }

    }
}
