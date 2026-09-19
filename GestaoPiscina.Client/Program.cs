using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GestaoPiscina.Client;
using GestaoPiscina.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Escolhe a API correta conforme o ambiente:
// - localhost/127.0.0.1 => API local do desenvolvimento
// - qualquer outro host (domínio fixo, túnel Cloudflare, e futuramente Azure) => API pública
var currentHost = new Uri(builder.HostEnvironment.BaseAddress).Host;
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:7001/";
if (currentHost.Contains("localhost", StringComparison.OrdinalIgnoreCase) || currentHost.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase))
{
    apiBaseUrl = builder.Configuration["ApiBaseUrlLocal"] ?? "http://localhost:7001/";
}
else
{
    apiBaseUrl = builder.Configuration["ApiBaseUrlPublic"] ?? apiBaseUrl;
}

builder.Services.AddScoped(sp =>
{
    var httpClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    return httpClient;
});

// Registrar serviços
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<ClienteStateService>();

// Configurar logging para debug
builder.Logging.SetMinimumLevel(LogLevel.Information);

await builder.Build().RunAsync();
