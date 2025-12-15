using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Cirurgico.Infrastructure.Persistence;
using Cirurgico.Infrastructure.Repositories;
using Cirurgico.Application.Interfaces;

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
});

// DB Context (MySQL)
var conn = builder.Configuration.GetConnectionString("CirurgicoDb");
if (!string.IsNullOrWhiteSpace(conn))
{
    builder.Services.AddDbContext<CirurgicoDbContext>(opt => opt.UseMySql(conn, ServerVersion.AutoDetect(conn)));
    builder.Services.AddScoped<IExameRepository, ExameRepository>();
}

// Consumers RabbitMQ
builder.Services.AddHostedService<Cirurgico.Infrastructure.Messaging.ExameSolicitadoBackgroundService>();
builder.Services.AddHostedService<Cirurgico.Infrastructure.Messaging.ProcedimentoAtualizacaoBackgroundService>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Cultura padrão pt-BR
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
app.UseSwaggerUI();
app.UseSwagger();

// Ensure database exists (no migrations yet for Cirúrgico)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<CirurgicoDbContext>();
        db.Database.EnsureCreated();
    }
    catch { }
}

app.Run();