var builder = WebApplication.CreateBuilder(args);
using System.Globalization;
using Microsoft.AspNetCore.Localization;


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

app.Run();