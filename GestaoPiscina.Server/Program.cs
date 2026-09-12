using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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

// Configuração do Entity Framework
builder.Services.AddDbContext<GestaoPiscinaContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração do CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:7000", "https://localhost:7000", "http://localhost:5000")
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazorApp");
app.UseStaticFiles(); // Serve as fotos enviadas em wwwroot/uploads (ver UploadsController)
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GestaoPiscinaContext>();
    await SeedData.SeedAsync(context);
}

app.Run();
