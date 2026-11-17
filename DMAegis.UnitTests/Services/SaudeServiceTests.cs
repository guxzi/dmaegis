using DMAegis.Repository;
using DMAegis.Services;
using Moq;

namespace DMAegis.UnitTests.Services;

public class SaudeServiceTests
{
    [Fact]
    public async Task VerificarAsync_BancoResponde_RetornaApiEBancoTrue()
    {
        var repositorio = new Mock<ISaudeRepository>();
        repositorio.Setup(r => r.BancoRespondeAsync()).ReturnsAsync(true);
        var service = new SaudeService(repositorio.Object);

        var resultado = await service.VerificarAsync();

        Assert.True(resultado.Api);
        Assert.True(resultado.Banco);
    }

    [Fact]
    public async Task VerificarAsync_BancoNaoResponde_RetornaBancoFalse()
    {
        var repositorio = new Mock<ISaudeRepository>();
        repositorio.Setup(r => r.BancoRespondeAsync()).ReturnsAsync(false);
        var service = new SaudeService(repositorio.Object);

        var resultado = await service.VerificarAsync();

        Assert.True(resultado.Api);
        Assert.False(resultado.Banco);
    }
}
