using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace DMAegis.Services;

public class CadastrarUsuarioServiceModel
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class UsuarioServiceModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public interface IUsuarioService
{
    Task<UsuarioServiceModel> CadastrarAsync(CadastrarUsuarioServiceModel dados);

    /// <summary>E-mail inexistente ou senha errada → CredenciaisInvalidas (mesma mensagem).</summary>
    Task<UsuarioServiceModel> AutenticarAsync(string email, string senha);

    /// <summary>Null se o usuário não existe mais.</summary>
    Task<UsuarioServiceModel?> ObterAsync(int id);
}

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    // Hash usado quando o e-mail não existe: a verificação roda do mesmo jeito,
    // para o tempo de resposta não revelar quais e-mails estão cadastrados.
    private readonly Lazy<string> _hashFicticio;

    public UsuarioService(IUsuarioRepository usuarioRepository, IPasswordHasher<Usuario> passwordHasher)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _hashFicticio = new Lazy<string>(() => _passwordHasher.HashPassword(new Usuario(), Guid.NewGuid().ToString()));
    }

    public static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();

    public async Task<UsuarioServiceModel> CadastrarAsync(CadastrarUsuarioServiceModel dados)
    {
        var usuario = new Usuario
        {
            Nome = dados.Nome.Trim(),
            Email = NormalizarEmail(dados.Email),
            DataCriacao = DateTime.Now,
        };
        usuario.HashSenha = _passwordHasher.HashPassword(usuario, dados.Senha);

        try
        {
            usuario.Id = await _usuarioRepository.InserirAsync(usuario);
        }
        catch (RegistroDuplicadoException)
        {
            throw new BusinessRuleException(BusinessErrors.EmailJaCadastrado);
        }

        return ParaServiceModel(usuario);
    }

    public async Task<UsuarioServiceModel> AutenticarAsync(string email, string senha)
    {
        var usuario = await _usuarioRepository.ObterPorEmailAsync(NormalizarEmail(email));

        if (usuario is null)
        {
            _passwordHasher.VerifyHashedPassword(new Usuario(), _hashFicticio.Value, senha);
            throw new BusinessRuleException(BusinessErrors.CredenciaisInvalidas);
        }

        var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.HashSenha, senha);
        if (resultado == PasswordVerificationResult.Failed)
            throw new BusinessRuleException(BusinessErrors.CredenciaisInvalidas);

        return ParaServiceModel(usuario);
    }

    public async Task<UsuarioServiceModel?> ObterAsync(int id)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(id);
        return usuario is null ? null : ParaServiceModel(usuario);
    }

    private static UsuarioServiceModel ParaServiceModel(Usuario usuario) => new()
    {
        Id = usuario.Id,
        Nome = usuario.Nome,
        Email = usuario.Email,
    };
}
