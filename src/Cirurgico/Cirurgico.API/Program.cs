using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Cirurgico.Infrastructure.Persistence;
using Cirurgico.Infrastructure.Repositories;
using Cirurgico.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;

using Shared.Contracts.Security;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(settings =>
{
    settings.EnableAnnotations();
    settings.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Cirurgico API",
        Version = "v1",
        Description = "API para gerenciamento de procedimentos cirúrgicos em um sistema hospitalar.",
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

var conn = builder.Configuration.GetConnectionString("CirurgicoDb");
if (!string.IsNullOrWhiteSpace(conn))
{
    builder.Services.AddDbContext<CirurgicoDbContext>(opt => opt.UseMySql(conn, ServerVersion.AutoDetect(conn)));
    builder.Services.AddScoped<IExameRepository, ExameRepository>();
}

builder.Services.AddHostedService<Cirurgico.Infrastructure.Messaging.ExameSolicitadoBackgroundService>();
builder.Services.AddHostedService<Cirurgico.Infrastructure.Messaging.ProcedimentoAtualizacaoBackgroundService>();

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

var disableHttps = builder.Configuration.GetValue<bool>("DisableHttpsRedirection");
if (!disableHttps)
    app.UseHttpsRedirection();

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

app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwaggerUI(c => c.EnablePersistAuthorization());
app.UseSwagger();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<CirurgicoDbContext>();
        db.Database.Migrate();
    }
    catch { }
}

app.Run();