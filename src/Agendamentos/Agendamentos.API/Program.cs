using Agendamentos.Infrastructure.Persistence;
using Agendamentos.Infrastructure.Repositories;
using Agendamentos.Application.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Agendamentos.Application.Validators;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Extensions;
using Shared.Contracts.Messaging;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerWithJwt(
    title: "Agendamentos API",
    version: "v1",
    description: "API para gerenciamento de agendamentos de consultas em um sistema hospitalar.");

var conn = builder.Configuration.GetConnectionString("AgendamentosDb");
builder.Services.AddDbContext<AgendamentosDbContext>(opt => opt.UseMySql(conn, ServerVersion.AutoDetect(conn)));

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<PacienteDtoValidator>();

builder.Services.AddScoped<IAgendamentoRepository, AgendamentoRepository>();
builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();
builder.Services.AddScoped<IAvailabilityService, Agendamentos.API.Services.AvailabilityHttpService>();
builder.Services.AddScoped<ISchedulingService, Agendamentos.API.Services.SchedulingService>();
builder.Services.AddScoped<IAgendamentoService, Agendamentos.API.Services.AgendamentoService>();

builder.Services.Configure<Shared.Contracts.Messaging.RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<Shared.Contracts.Messaging.IRabbitPublisher, Shared.Contracts.Messaging.RabbitMqPublisher>();

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

builder.Services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddKeycloakJwtAuth(builder.Configuration);

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
var disableHttps = builder.Configuration.GetValue<bool>("DisableHttpsRedirection");
if (!disableHttps)
    app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AgendamentosDbContext>();
    try { db.Database.Migrate(); } catch(Exception e) { Console.WriteLine("Erro" + e.Message); }
}
app.Run();
