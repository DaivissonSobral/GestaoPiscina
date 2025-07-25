using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using GestaoPiscina.Client.Models;

namespace GestaoPiscina.Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly NavigationManager _navigationManager;

        public event Action? AuthenticationStateChanged;

        public AuthService(HttpClient httpClient, ILocalStorageService localStorage, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _navigationManager = navigationManager;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result?.Sucesso == true)
                    {
                        await _localStorage.SetItemAsync("token", result.Token);
                        await _localStorage.SetItemAsync("refreshToken", result.RefreshToken);
                        await _localStorage.SetItemAsync("user", result.Usuario);
                        
                        AuthenticationStateChanged?.Invoke();
                        return result;
                    }
                }
                
                var errorResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return errorResponse ?? new LoginResponse { Sucesso = false, Mensagem = "Erro no login" };
            }
            catch (Exception ex)
            {
                return new LoginResponse { Sucesso = false, Mensagem = "Erro de conexão: " + ex.Message };
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _httpClient.PostAsync("api/auth/logout", null);
            }
            catch
            {
                // Ignorar erros no logout
            }
            finally
            {
                await _localStorage.RemoveItemAsync("token");
                await _localStorage.RemoveItemAsync("refreshToken");
                await _localStorage.RemoveItemAsync("user");
                
                AuthenticationStateChanged?.Invoke();
                _navigationManager.NavigateTo("/login");
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("token");
            return !string.IsNullOrEmpty(token);
        }

        public async Task<UsuarioInfo?> GetCurrentUserAsync()
        {
            try
            {
                var user = await _localStorage.GetItemAsync<UsuarioInfo>("user");
                if (user != null)
                {
                    return user;
                }

                // Se não tem no localStorage, buscar da API
                var response = await _httpClient.GetAsync("api/auth/me");
                if (response.IsSuccessStatusCode)
                {
                    user = await response.Content.ReadFromJsonAsync<UsuarioInfo>();
                    if (user != null)
                    {
                        await _localStorage.SetItemAsync("user", user);
                        return user;
                    }
                }
            }
            catch
            {
                // Em caso de erro, fazer logout
                await LogoutAsync();
            }

            return null;
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _localStorage.GetItemAsync<string>("token");
        }

        public async Task<bool> HasPermissionAsync(string permission)
        {
            var user = await GetCurrentUserAsync();
            if (user?.Permissoes == null) return false;
            
            return user.Permissoes.ContainsKey(permission) && user.Permissoes[permission];
        }

        public async Task<bool> HasAnyPermissionAsync(params string[] permissions)
        {
            var user = await GetCurrentUserAsync();
            if (user?.Permissoes == null) return false;
            
            return permissions.Any(p => user.Permissoes.ContainsKey(p) && user.Permissoes[p]);
        }
    }

    public interface ILocalStorageService
    {
        Task<T?> GetItemAsync<T>(string key);
        Task SetItemAsync<T>(string key, T value);
        Task RemoveItemAsync(string key);
    }

    public class LocalStorageService : ILocalStorageService
    {
        private readonly IJSRuntime _jsRuntime;

        public LocalStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<T?> GetItemAsync<T>(string key)
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
                if (string.IsNullOrEmpty(json)) return default;
                return JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                return default;
            }
        }

        public async Task SetItemAsync<T>(string key, T value)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
            }
            catch
            {
                // Ignorar erros
            }
        }

        public async Task RemoveItemAsync(string key)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            }
            catch
            {
                // Ignorar erros
            }
        }
    }
} 