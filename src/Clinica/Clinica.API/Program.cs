using Microsoft.EntityFrameworkCore;
using Clinica.Application.Interfaces;
using Clinica.Infrastructure.Repositories;
using Clinica.Infrastructure.Persistence;
using System.Net.Http;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Shared.Contracts.Extensions;
using Shared.Contracts.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerWithJwt("Clinica API");
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

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<IRabbitPublisher, RabbitMqPublisher>();

// Consumer baseado no padrão compartilhado
builder.Services.AddHostedService<Clinica.Infrastructure.Messaging.ConsultaCriadaConsumer>();

builder.Services.AddKeycloakJwtAuth(builder.Configuration);

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

var disableHttps = builder.Configuration.GetValue<bool>("DisableHttpsRedirection");
if (!disableHttps)
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
