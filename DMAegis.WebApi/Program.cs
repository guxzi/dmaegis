using DMAegis.Services.Exceptions;
using DMAegis.WebApi.ApiModels;
using DMAegis.WebApi.Configuration;
using DMAegis.WebApi.Middlewares;
using Microsoft.AspNetCore.Mvc;

// Carrega o .env da raiz do repositório (procura nas pastas acima do projeto).
DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().ConfigureApiBehaviorOptions(opcoes =>
{
    // JSON malformado ou com tipo errado: responde no envelope (código 2), como qualquer erro de dados.
    opcoes.InvalidModelStateResponseFactory = _ => new BadRequestObjectResult(
        ApiResponse<object>.Erro(BusinessErrors.DadosInvalidos.Code, BusinessErrors.DadosInvalidos.DefaultMessage));
});
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

AutenticacaoConfig.Registrar(builder.Services, builder.Environment);
InjecaoDependenciaConfig.Registrar(builder.Services);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(opcoes => opcoes.SwaggerEndpoint("/openapi/v1.json", "DMAegis API"));
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Permite que os testes de integração usem WebApplicationFactory<Program>.
public partial class Program;
