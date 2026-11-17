using DMAegis.WebApi.Configuration;
using DMAegis.WebApi.Middlewares;

// Carrega o .env da raiz do repositório (procura nas pastas acima do projeto).
DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

InjecaoDependenciaConfig.Registrar(builder.Services);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(opcoes => opcoes.SwaggerEndpoint("/openapi/v1.json", "DMAegis API"));
}

app.UseAuthorization();

app.MapControllers();

app.Run();
