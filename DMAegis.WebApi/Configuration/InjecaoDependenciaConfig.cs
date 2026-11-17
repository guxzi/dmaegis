using DMAegis.Repository;
using DMAegis.Services;

namespace DMAegis.WebApi.Configuration;

public static class InjecaoDependenciaConfig
{
    public static void Registrar(IServiceCollection services)
    {
        services.AddSingleton<IConexaoFactory, ConexaoFactory>();

        // Repositories
        services.AddScoped<ISaudeRepository, SaudeRepository>();

        // Services
        services.AddScoped<ISaudeService, SaudeService>();
    }
}
