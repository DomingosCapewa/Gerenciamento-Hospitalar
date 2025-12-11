using Microsoft.EntityFrameworkCore;
using Clinica.Application.Interfaces;
using Clinica.Infrastructure.Repositories;
using Clinica.Infrastructure.Persistence;
using System.Net.Http;

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
builder.Services.AddHttpClient("ClinicaApi", c =>
{
    var baseUrl = builder.Configuration["ExternalApis:ClinicaBaseUrl"] ?? "https://localhost:7265";
    c.BaseAddress = new Uri(baseUrl);
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
