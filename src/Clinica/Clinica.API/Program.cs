using Microsoft.EntityFrameworkCore;
using Clinica.Application.Interfaces;
using Clinica.Infrastructure.Repositories;
using Clinica.Infrastructure.Persistence;
using System.Net.Http;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Clinica.Infrastructure.Messaging;

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
});

builder.Services.AddHostedService<ConsultaCriadaBackgroundService>();

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
app.MapControllers();

// Apply DB migrations automatically
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
app.UseSwaggerUI();

app.Run();
