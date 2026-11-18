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
        services.AddScoped<ISistemaRepository, SistemaRepository>();
        services.AddScoped<ICampanhaRepository, CampanhaRepository>();
        services.AddScoped<ITipoRegistroRepository, TipoRegistroRepository>();
        services.AddScoped<IModeloAtributoRepository, ModeloAtributoRepository>();
        services.AddScoped<IRegistroRepository, RegistroRepository>();
        services.AddScoped<IAtributoRegistroRepository, AtributoRegistroRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IRegistroTagRepository, RegistroTagRepository>();
        services.AddScoped<ISessaoRepository, SessaoRepository>();
        services.AddScoped<ICenaSessaoRepository, CenaSessaoRepository>();
        services.AddScoped<ISessaoRegistroRepository, SessaoRegistroRepository>();
        services.AddScoped<ISegredoRepository, SegredoRepository>();
        services.AddScoped<IAnotacaoSessaoRepository, AnotacaoSessaoRepository>();

        // Services
        services.AddScoped<ISaudeService, SaudeService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<ICampanhaService, CampanhaService>();
        services.AddScoped<IRegistroService, RegistroService>();
        services.AddScoped<ISessaoService, SessaoService>();

        // Validators (todos os AbstractValidator deste projeto)
        services.AddValidatorsFromAssemblyContaining<LoginValidator>();
    }
}
