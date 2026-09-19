using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Azure.Storage.Blobs;
using GestaoPiscina.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
    {
        // Sem isso, o ASP.NET Core trata propriedades de navegação do EF (não anuláveis, ex: OrdemDeServico.Piscina)
        // como implicitamente [Required] no binding do corpo da requisição, mesmo quando o cliente
        // só envia a chave estrangeira (ex: IDPiscina) e não o objeto aninhado.
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// O Cloudflare Tunnel (e futuramente o Azure App Service) fica na frente da API fazendo
// proxy reverso: a conexão real do cloudflared para o Kestrel é HTTP em localhost, mesmo
// quando o cliente acessou via HTTPS. Sem isso, Request.Scheme/Request.Host (usados em
// UploadsController para montar a URL da foto) voltam "http://localhost:7001" em vez do
// domínio público, e UseHttpsRedirection tenta redirecionar a conexão local incorretamente.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
});

// Configuração do Entity Framework
builder.Services.AddDbContext<GestaoPiscinaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cliente do Azure Blob Storage, usado pelo UploadsController para gravar as fotos
// enviadas (substitui o disco local, que não é confiável no App Service do Azure).
builder.Services.AddSingleton(new BlobServiceClient(builder.Configuration["AzureStorage:ConnectionString"]));

// Configuração do CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp",
        policy =>
        {
            // Além do localhost e do domínio fixo (app.blup.ia.br), libera qualquer túnel
            // *.trycloudflare.com: durante testes rápidos a URL do túnel muda a cada sessão,
            // então travar só no hostname fixo quebraria esse fluxo.
            policy.SetIsOriginAllowed(origin =>
                      origin is "http://localhost:7000" or "https://localhost:7000" or "http://localhost:5000" or "https://app.blup.ia.br"
                      || (Uri.TryCreate(origin, UriKind.Absolute, out var uri) && uri.Host.EndsWith(".trycloudflare.com")))
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Configuração do JWT
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Configuração 'Jwt:SecretKey' não encontrada. Configure via 'dotnet user-secrets' (dev) ou variável de ambiente (produção).");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "GestaoPiscina",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "GestaoPiscinaUsers",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Registrar serviços
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();

var app = builder.Build();

// Precisa vir antes de qualquer outro middleware: é o que corrige Scheme/Host
// a partir dos cabeçalhos X-Forwarded-* enviados pelo proxy (Cloudflare Tunnel/Azure).
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazorApp");
app.UseStaticFiles(); // Serve fotos antigas que ainda existam fisicamente em wwwroot/uploads (pré-migração para o Blob Storage)
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Compatibilidade com fotos enviadas antes da migração para o Blob Storage: URLs antigas
// salvas no banco no formato /uploads/{pasta}/{arquivo} (servidas via UseStaticFiles) agora
// redirecionam para o blob correspondente, sem precisar alterar os registros já gravados.
app.MapGet("/uploads/{pasta}/{arquivo}", (string pasta, string arquivo, BlobServiceClient blobServiceClient, IConfiguration config) =>
{
    var containerName = config["AzureStorage:ContainerName"] ?? "uploads";
    var blobClient = blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient($"{pasta}/{arquivo}");
    return Results.Redirect(blobClient.Uri.ToString());
});

// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GestaoPiscinaContext>();
    await SeedData.SeedAsync(context);
}

app.Run();
