using DMAegis.WebApi.ApiModels.Registros;
using DMAegis.WebApi.Validators;

namespace DMAegis.UnitTests.WebApi;

public class RegistroValidatorTests
{
    private static SalvarRegistroRequestApiModel Valido() => new()
    {
        IdTipoRegistro = 3,
        Nome = "Goblin",
        Resumo = "Batedor",
        Atributos = [new() { Chave = "CA", Valor = "15" }],
        Tags = ["estrada"],
    };

    private static bool Valida(SalvarRegistroRequestApiModel modelo) =>
        new SalvarRegistroValidator().Validate(modelo).IsValid;

    [Fact]
    public void Valido_Passa() => Assert.True(Valida(Valido()));

    [Fact]
    public void NomeVazio_Falha()
    {
        var m = Valido();
        m.Nome = " ";
        Assert.False(Valida(m));
    }

    [Fact]
    public void ChaveRepetida_FalhaComMensagem()
    {
        var m = Valido();
        m.Atributos.Add(new() { Chave = "ca", Valor = "16" });

        var resultado = new SalvarRegistroValidator().Validate(m);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.ErrorMessage == "Atributo repetido: ca.");
    }

    [Fact]
    public void ValorCom501Caracteres_Falha()
    {
        var m = Valido();
        m.Atributos[0].Valor = new string('x', 501);
        Assert.False(Valida(m));
    }

    [Fact]
    public void Com51Atributos_Falha()
    {
        var m = Valido();
        m.Atributos = Enumerable.Range(1, 51).Select(i => new AtributoApiModel { Chave = $"A{i}", Valor = "1" }).ToList();
        Assert.False(Valida(m));
    }

    [Fact]
    public void TagCom51Caracteres_Falha()
    {
        var m = Valido();
        m.Tags = [new string('t', 51)];
        Assert.False(Valida(m));
    }

    [Fact]
    public void Com21Tags_Falha()
    {
        var m = Valido();
        m.Tags = Enumerable.Range(1, 21).Select(i => $"t{i}").ToList();
        Assert.False(Valida(m));
    }

    [Fact]
    public void AtributoComValorESemChave_Falha()
    {
        var m = Valido();
        m.Atributos.Add(new() { Chave = " ", Valor = "algo" });
        Assert.False(Valida(m));
    }

    [Fact]
    public void AtributoSemChaveESemValor_Passa()
    {
        var m = Valido();
        m.Atributos.Add(new() { Chave = "", Valor = "" });
        Assert.True(Valida(m));
    }
}
