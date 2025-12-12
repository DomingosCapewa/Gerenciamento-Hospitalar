using Agendamentos.Infrastructure.Persistence;
using Agendamentos.Infrastructure.Repositories;
using Agendamentos.Application.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Agendamentos.Application.Validators;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(config =>
{
    config.EnableAnnotations();
    config.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Agendamentos API",
        Version = "v1",
        Description = "API para gerenciamento de agendamentos de consultas em um sistema hospitalar.",
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
    var cfg = builder.Configuration.GetSection("Rabbit").Get<RabbitOptions>() ?? new RabbitOptions();
    return new RabbitPublisher(cfg.Host, cfg.Port, cfg.User, cfg.Password);
});

builder.Services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors("AllowAll");
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseHttpsRedirection();
app.MapControllers();
app.Run();

public record RabbitOptions { public string Host { get; init; } = "localhost"; public int Port { get; init; } = 5672; public string User { get; init; } = "guest"; public string Password { get; init; } = "guest"; }
