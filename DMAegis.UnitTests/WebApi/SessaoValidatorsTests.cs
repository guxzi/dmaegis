using DMAegis.WebApi.ApiModels.Sessoes;
using DMAegis.WebApi.Validators;

namespace DMAegis.UnitTests.WebApi;

public class SessaoValidatorsTests
{
    private static SalvarSessaoRequestApiModel Preparo() => new() { Numero = 1, Cenas = ["Ponte"], IdsRegistro = [1] };

    private static bool Valida(SalvarSessaoRequestApiModel m) => new SalvarSessaoValidator().Validate(m).IsValid;

    [Fact]
    public void Preparo_Valido_Passa() => Assert.True(Valida(Preparo()));

    [Fact]
    public void Numero0_Falha()
    {
        var m = Preparo();
        m.Numero = 0;
        Assert.False(Valida(m));
    }

    [Fact]
    public void Numero10000_Falha()
    {
        var m = Preparo();
        m.Numero = 10000;
        Assert.False(Valida(m));
    }

    [Fact]
    public void InserirSemNumero_Passa() =>
        Assert.True(new InserirSessaoValidator().Validate(new InserirSessaoRequestApiModel()).IsValid);

    [Fact]
    public void InserirNumero0_Falha() =>
        Assert.False(new InserirSessaoValidator().Validate(new InserirSessaoRequestApiModel { Numero = 0 }).IsValid);

    [Fact]
    public void CenaCom501Caracteres_Falha()
    {
        var m = Preparo();
        m.Cenas = [new string('c', 501)];
        Assert.False(Valida(m));
    }

    [Fact]
    public void Com31Cenas_Falha()
    {
        var m = Preparo();
        m.Cenas = Enumerable.Range(1, 31).Select(i => $"Cena {i}").ToList();
        Assert.False(Valida(m));
    }

    [Fact]
    public void Com201Vinculos_Falha()
    {
        var m = Preparo();
        m.IdsRegistro = Enumerable.Range(1, 201).ToList();
        Assert.False(Valida(m));
    }

    [Fact]
    public void SegredoVazio_Falha() =>
        Assert.False(new SalvarSegredoValidator().Validate(new SalvarSegredoRequestApiModel { Texto = "  " }).IsValid);

    [Fact]
    public void SegredoCom501Caracteres_Falha() =>
        Assert.False(new SalvarSegredoValidator().Validate(new SalvarSegredoRequestApiModel { Texto = new string('s', 501) }).IsValid);
}
