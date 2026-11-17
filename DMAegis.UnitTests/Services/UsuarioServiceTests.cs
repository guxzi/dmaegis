using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services;
using DMAegis.Services.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace DMAegis.UnitTests.Services;

public class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _repositorio = new();
    private readonly PasswordHasher<Usuario> _hasher = new();

    private UsuarioService CriarService() => new(_repositorio.Object, _hasher);

    private Usuario UsuarioComSenha(string senha) => new()
    {
        Id = 7,
        Nome = "Mestre",
        Email = "mestre@x.com",
        HashSenha = _hasher.HashPassword(new Usuario(), senha),
    };

    [Fact]
    public async Task CadastrarAsync_EmailComMaiusculasEEspacos_GravaNormalizado()
    {
        Usuario? gravado = null;
        _repositorio.Setup(r => r.InserirAsync(It.IsAny<Usuario>()))
            .Callback<Usuario>(u => gravado = u).ReturnsAsync(1);

        var resultado = await CriarService().CadastrarAsync(new CadastrarUsuarioServiceModel
        {
            Nome = "Mestre", Email = "  Mestre@X.com ", Senha = "senha-segura",
        });

        Assert.Equal("mestre@x.com", gravado!.Email);
        Assert.Equal("mestre@x.com", resultado.Email);
        Assert.Equal(1, resultado.Id);
    }

    [Fact]
    public async Task CadastrarAsync_GravaHashENaoASenha()
    {
        Usuario? gravado = null;
        _repositorio.Setup(r => r.InserirAsync(It.IsAny<Usuario>()))
            .Callback<Usuario>(u => gravado = u).ReturnsAsync(1);

        await CriarService().CadastrarAsync(new CadastrarUsuarioServiceModel
        {
            Nome = "Mestre", Email = "mestre@x.com", Senha = "senha-segura",
        });

        Assert.NotEqual("senha-segura", gravado!.HashSenha);
        Assert.Equal(PasswordVerificationResult.Success,
            _hasher.VerifyHashedPassword(gravado, gravado.HashSenha, "senha-segura"));
    }

    [Fact]
    public async Task CadastrarAsync_EmailDuplicado_LancaEmailJaCadastrado()
    {
        _repositorio.Setup(r => r.InserirAsync(It.IsAny<Usuario>()))
            .ThrowsAsync(new RegistroDuplicadoException());

        var erro = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CriarService().CadastrarAsync(new CadastrarUsuarioServiceModel
            {
                Nome = "Mestre", Email = "mestre@x.com", Senha = "senha-segura",
            }));

        Assert.Equal(BusinessErrors.EmailJaCadastrado.Code, erro.Code);
        Assert.Equal("Este e-mail já está cadastrado.", erro.Mensagem);
    }

    [Fact]
    public async Task AutenticarAsync_SenhaCorreta_RetornaUsuario()
    {
        _repositorio.Setup(r => r.ObterPorEmailAsync("mestre@x.com"))
            .ReturnsAsync(UsuarioComSenha("senha-segura"));

        var resultado = await CriarService().AutenticarAsync(" MESTRE@x.com", "senha-segura");

        Assert.Equal(7, resultado.Id);
        Assert.Equal("Mestre", resultado.Nome);
        Assert.Equal("mestre@x.com", resultado.Email);
    }

    [Fact]
    public async Task AutenticarAsync_SenhaErrada_LancaCredenciaisInvalidas()
    {
        _repositorio.Setup(r => r.ObterPorEmailAsync("mestre@x.com"))
            .ReturnsAsync(UsuarioComSenha("senha-segura"));

        var erro = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CriarService().AutenticarAsync("mestre@x.com", "outra-senha"));

        Assert.Equal(BusinessErrors.CredenciaisInvalidas.Code, erro.Code);
        Assert.Equal("E-mail ou senha inválidos.", erro.Mensagem);
    }

    [Fact]
    public async Task AutenticarAsync_EmailInexistente_LancaCredenciaisInvalidas()
    {
        _repositorio.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);

        var erro = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CriarService().AutenticarAsync("ninguem@x.com", "qualquer-senha"));

        Assert.Equal(BusinessErrors.CredenciaisInvalidas.Code, erro.Code);
        Assert.Equal("E-mail ou senha inválidos.", erro.Mensagem);
    }
}
