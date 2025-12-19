using Agendamentos.Infrastructure.Persistence;
using Agendamentos.Infrastructure.Repositories;
using Agendamentos.Application.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Agendamentos.Application.Validators;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Shared.Contracts.Security;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(config =>
{
    config.EnableAnnotations();
    config.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Agendamentos API",
        Version = "v1",
        Description = "API para gerenciamento de agendamentos de consultas em um sistema hospitalar.",
    });
    config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header usando Bearer.
                                Entre com 'Bearer ' [espaço] então coloque seu token.
                                Exemplo: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
});
            config.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
    });
});

var conn = builder.Configuration.GetConnectionString("AgendamentosDb");
builder.Services.AddDbContext<AgendamentosDbContext>(opt => opt.UseMySql(conn, ServerVersion.AutoDetect(conn)));

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<PacienteDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<PacienteDtoValidator>();

builder.Services.AddScoped<IAgendamentoRepository, AgendamentoRepository>();
builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();

builder.Services.AddSingleton<IRabbitPublisher>(sp =>
{
    var cfg = builder.Configuration.GetSection("RabbitMQ").Get<RabbitOptions>() ?? new RabbitOptions();
    return new RabbitPublisher(cfg.Host, cfg.Port, cfg.User, cfg.Password);
});

builder.Services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

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
app.UseCors("AllowAll");
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(c => c.EnablePersistAuthorization()); }
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AgendamentosDbContext>();
    try { db.Database.EnsureCreated(); } catch(Exception e) { Console.WriteLine("Erro" + e.Message); }
}
app.Run();

public record RabbitOptions { public string Host { get; init; } = "localhost"; public int Port { get; init; } = 5672; public string User { get; init; } = "guest"; public string Password { get; init; } = "guest"; }
