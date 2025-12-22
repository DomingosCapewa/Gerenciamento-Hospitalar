using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Cirurgico.Infrastructure.Persistence;
using Cirurgico.Infrastructure.Repositories;
using Cirurgico.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Shared.Contracts.Extensions;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt(
    title: "Cirurgico API",
    version: "v1",
    description: "API para gerenciamento de procedimentos cirúrgicos em um sistema hospitalar.");

var conn = builder.Configuration.GetConnectionString("CirurgicoDb");
if (!string.IsNullOrWhiteSpace(conn))
{
    builder.Services.AddDbContext<CirurgicoDbContext>(opt => opt.UseMySql(conn, ServerVersion.AutoDetect(conn)));
    builder.Services.AddScoped<IExameRepository, ExameRepository>();
}

builder.Services.Configure<Shared.Contracts.Messaging.RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddHostedService<Cirurgico.Infrastructure.Messaging.ExameSolicitadoConsumer>();
builder.Services.AddHostedService<Cirurgico.Infrastructure.Messaging.ProcedimentoAtualizacaoConsumer>();

builder.Services.AddKeycloakJwtAuth(builder.Configuration);

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