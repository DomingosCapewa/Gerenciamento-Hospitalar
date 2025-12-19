using Microsoft.EntityFrameworkCore;
using Clinica.Application.Interfaces;
using Clinica.Infrastructure.Repositories;
using Clinica.Infrastructure.Persistence;
using System.Net.Http;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Clinica.Infrastructure.Messaging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Shared.Contracts.Security;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var conn = builder.Configuration.GetConnectionString("ClinicaDb");
builder.Services.AddDbContext<ClinicaDbContext>(opt => opt.UseMySql(conn, ServerVersion.AutoDetect(conn)));

builder.Services.AddScoped<IMedicoRepository, MedicoRepository>();
builder.Services.AddScoped<IConsultaClinicaRepository, ConsultaClinicaRepository>();
builder.Services.AddScoped<ISintomaRepository, SintomaRepository>();
builder.Services.AddHttpClient("ClinicaApi", c =>
{
    var baseUrl = builder.Configuration["ExternalApis:ClinicaBaseUrl"] ?? "https://localhost:7265";
    c.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddHttpClient("CirurgicoApi", c =>
{
    var baseUrl = builder.Configuration["ExternalApis:CirurgicoBaseUrl"] ?? "https://localhost:7108";
    c.BaseAddress = new Uri(baseUrl);
});


builder.Services.AddSingleton<EventPublisher>(sp =>
{
    var cfg = builder.Configuration.GetSection("RabbitMQ");
    var host = cfg["Host"] ?? "localhost";
    var port = int.TryParse(cfg["Port"], out var p) ? p : 5672;
    var user = cfg["User"] ?? "guest";
    var pass = cfg["Password"] ?? "guest";
    return new EventPublisher(host, port, user, pass);
});


builder.Services.AddSwaggerGen(settings =>
{
    settings.EnableAnnotations();
    settings.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Clinica API",
        Version = "v1",
        Description = "API para gerenciamento de clínicas do sistema hospitalar.",
    });
    settings.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header usando Bearer.
                                Entre com 'Bearer ' [espaço] então coloque seu token.
                                Exemplo: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    settings.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHostedService<ConsultaCriadaBackgroundService>();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.Audience = builder.Configuration["Authentication:Audience"];
        options.RequireHttpsMetadata = builder.Configuration.GetValue<bool>("Authentication:RequireHttpsMetadata");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            RoleClaimType = "roles",
            NameClaimType = "preferred_username"
        };
    });
builder.Services.AddSingleton<IClaimsTransformation, KeycloakRoleClaimsTransformer>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Usuario", p => p.RequireRole("Usuario", "usuario", "user"));
    options.AddPolicy("Medico", p => p.RequireRole("Medico", "medico", "doctor"));
    options.AddPolicy("Admin", p => p.RequireRole("Admin", "admin"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var culture = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;
var locOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = new[] { culture },
    SupportedUICultures = new[] { culture }
};
app.UseRequestLocalization(locOptions);

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ClinicaDbContext>();
        db.Database.Migrate();
    }
    catch { }
}

app.UseSwagger();
app.UseSwaggerUI(c => c.EnablePersistAuthorization());

app.Run();
