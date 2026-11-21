using DMAegis.Services.Exceptions;
using DMAegis.WebApi.ApiModels;
using DMAegis.WebApi.Configuration;
using DMAegis.WebApi.Middlewares;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

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

// Atrás do túnel (cloudflared no próprio PC), a API enxerga o https e o IP de quem acessa.
// Só confia nesses cabeçalhos vindos do loopback (o padrão do ASP.NET Core).
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
});

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

// Modo mesa (túnel): a API também serve o site compilado, para o túnel apontar para uma origem só.
// Liga com DMAEGIS_SERVIR_SITE=1 (o "Abrir mesa para os jogadores.cmd" faz isso).
if (Environment.GetEnvironmentVariable("DMAEGIS_SERVIR_SITE") == "1")
{
    // Com "dotnet run --project" a raiz é a pasta do projeto; rodando a DLL da raiz do repositório, é o repositório.
    var raiz = app.Environment.ContentRootPath;
    var pastaSite = Environment.GetEnvironmentVariable("DMAEGIS_SITE") is { Length: > 0 } configurada
        ? Path.GetFullPath(configurada)
        : new[] { Path.Combine(raiz, "..", "dmaegis.web", "dist"), Path.Combine(raiz, "dmaegis.web", "dist") }
            .Select(Path.GetFullPath)
            .FirstOrDefault(Directory.Exists) ?? Path.GetFullPath(Path.Combine(raiz, "..", "dmaegis.web", "dist"));
    if (Directory.Exists(pastaSite))
    {
        var arquivos = new PhysicalFileProvider(pastaSite);
        app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = arquivos });
        app.UseStaticFiles(new StaticFileOptions { FileProvider = arquivos });
        // Rotas do React (/jogar, /convite/…) voltam o index.html; as da API continuam com 404 no envelope.
        // "nonfile" deixa de fora endereços de arquivo (/assets/x.js): se a rota pegasse esses também,
        // ela venceria os arquivos estáticos e o navegador receberia HTML no lugar do JavaScript.
        app.MapFallbackToFile("{*caminho:regex(^(?!api/).*$):nonfile}", "index.html", new StaticFileOptions { FileProvider = arquivos });
    }
    else
    {
        app.Logger.LogWarning("DMAEGIS_SERVIR_SITE=1, mas a pasta do site não existe: {Pasta}. Rode npm run build.", pastaSite);
    }
}

app.Run();

// Permite que os testes de integração usem WebApplicationFactory<Program>.
public partial class Program;
