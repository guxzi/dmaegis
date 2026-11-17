using DMAegis.WebApi.ApiModels.Auth;
using DMAegis.WebApi.Validators;

namespace DMAegis.UnitTests.WebApi;

public class AuthValidatorsTests
{
    private static CadastrarUsuarioRequestApiModel CadastroValido() => new()
    {
        Nome = "Mestre", Email = "mestre@x.com", Senha = "12345678",
    };

    [Fact]
    public void Cadastro_Valido_Passa()
    {
        Assert.True(new CadastrarUsuarioValidator().Validate(CadastroValido()).IsValid);
    }

    [Fact]
    public void Cadastro_SenhaCom7Caracteres_Falha()
    {
        var modelo = CadastroValido();
        modelo.Senha = "1234567";
        Assert.False(new CadastrarUsuarioValidator().Validate(modelo).IsValid);
    }

    [Fact]
    public void Cadastro_EmailInvalido_Falha()
    {
        var modelo = CadastroValido();
        modelo.Email = "abc";
        Assert.False(new CadastrarUsuarioValidator().Validate(modelo).IsValid);
    }

    [Fact]
    public void Cadastro_NomeVazio_Falha()
    {
        var modelo = CadastroValido();
        modelo.Nome = "  ";
        Assert.False(new CadastrarUsuarioValidator().Validate(modelo).IsValid);
    }

    [Fact]
    public void Login_SemSenha_Falha()
    {
        var modelo = new LoginRequestApiModel { Email = "mestre@x.com", Senha = "" };
        Assert.False(new LoginValidator().Validate(modelo).IsValid);
    }
}
