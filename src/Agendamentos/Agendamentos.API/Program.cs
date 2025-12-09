var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(settings =>
{
    settings.EnableAnnotations();
    settings.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Agendamentos API",
        Version = "v1",
        Description = "API para gerenciamento de agendamentos em um sistema hospitalar.",
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseSwaggerUI();
app.UseSwagger();

app.MapGet("/", () =>
{
   
    return "Agendaementos API is running!";
});

app.Run();

