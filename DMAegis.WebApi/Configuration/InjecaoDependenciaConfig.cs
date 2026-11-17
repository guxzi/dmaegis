using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services;
using DMAegis.WebApi.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace DMAegis.WebApi.Configuration;

public static class InjecaoDependenciaConfig
{
    public static void Registrar(IServiceCollection services)
    {
        services.AddSingleton<IConexaoFactory, ConexaoFactory>();
        services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

        // Repositories
        services.AddScoped<ISaudeRepository, SaudeRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        // Services
        services.AddScoped<ISaudeService, SaudeService>();
        services.AddScoped<IUsuarioService, UsuarioService>();

        // Validators (todos os AbstractValidator deste projeto)
        services.AddValidatorsFromAssemblyContaining<LoginValidator>();
    }
}
