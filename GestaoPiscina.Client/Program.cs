using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GestaoPiscina.Client;
using GestaoPiscina.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configurar HttpClient com a URL da API
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:7001/";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// Registrar serviços
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

// Configurar logging para debug
builder.Logging.SetMinimumLevel(LogLevel.Information);

await builder.Build().RunAsync();
