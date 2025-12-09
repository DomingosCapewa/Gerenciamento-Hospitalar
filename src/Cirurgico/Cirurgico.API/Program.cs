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
    
    
    return "Cirurgico API is running, access swagger with /swagger!";
});

app.Run();