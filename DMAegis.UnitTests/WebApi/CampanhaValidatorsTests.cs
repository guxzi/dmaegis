using DMAegis.WebApi.ApiModels.Campanhas;
using DMAegis.WebApi.Validators;

namespace DMAegis.UnitTests.WebApi;

public class CampanhaValidatorsTests
{
    private static InserirCampanhaRequestApiModel InsercaoValida() => new()
    {
        IdSistema = 1, Nome = "A Mina Perdida", Descricao = "Uma vila some.",
    };

    [Fact]
    public void Insercao_Valida_Passa() =>
        Assert.True(new InserirCampanhaValidator().Validate(InsercaoValida()).IsValid);

    [Fact]
    public void Insercao_NomeSoComEspacos_FalhaComMensagem()
    {
        var modelo = InsercaoValida();
        modelo.Nome = "   ";

        var resultado = new InserirCampanhaValidator().Validate(modelo);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.ErrorMessage == "Informe o nome da campanha.");
    }

    [Fact]
    public void Insercao_NomeCom101Caracteres_Falha()
    {
        var modelo = InsercaoValida();
        modelo.Nome = new string('a', 101);
        Assert.False(new InserirCampanhaValidator().Validate(modelo).IsValid);
    }

    [Fact]
    public void Insercao_DescricaoCom5001Caracteres_Falha()
    {
        var modelo = InsercaoValida();
        modelo.Descricao = new string('a', 5001);
        Assert.False(new InserirCampanhaValidator().Validate(modelo).IsValid);
    }

    [Fact]
    public void Insercao_SemSistema_Falha()
    {
        var modelo = InsercaoValida();
        modelo.IdSistema = 0;
        Assert.False(new InserirCampanhaValidator().Validate(modelo).IsValid);
    }

    [Fact]
    public void Atualizacao_Valida_Passa() =>
        Assert.True(new AtualizarCampanhaValidator()
            .Validate(new AtualizarCampanhaRequestApiModel { Nome = "Novo nome", Descricao = null }).IsValid);

    [Fact]
    public void Atualizacao_NomeVazio_Falha() =>
        Assert.False(new AtualizarCampanhaValidator()
            .Validate(new AtualizarCampanhaRequestApiModel { Nome = "" }).IsValid);
}
