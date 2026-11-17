using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DMAegis.Services;
using DMAegis.Services.Exceptions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace DMAegis.UnitTests.WebApi;

/// <summary>
/// Testes de integração do /api/auth com a API real em memória e o IUsuarioService trocado por mock.
/// Cada teste cria a própria factory para o limite de tentativas não vazar entre testes.
/// </summary>
public class AuthControllerTests
{
    private static readonly UsuarioServiceModel Mestre = new() { Id = 7, Nome = "Mestre", Email = "mestre@x.com" };

    private readonly Mock<IUsuarioService> _service = new();

    private HttpClient CriarCliente()
    {
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => _service.Object);
            }));
        return factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
    }

    /// <summary>Pede um token CSRF e já o coloca no cabeçalho X-XSRF-TOKEN do cliente.</summary>
    private static async Task RenovarCsrfAsync(HttpClient cliente)
    {
        var resposta = await cliente.GetAsync("/api/auth/csrf");
        resposta.EnsureSuccessStatusCode();
        var cookie = resposta.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("XSRF-TOKEN="));
        var token = Uri.UnescapeDataString(cookie.Split(';')[0]["XSRF-TOKEN=".Length..]);
        cliente.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
        cliente.DefaultRequestHeaders.Add("X-XSRF-TOKEN", token);
    }

    private static async Task<JsonElement> LerEnvelopeAsync(HttpResponseMessage resposta) =>
        JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement;

    private async Task<HttpResponseMessage> EntrarAsync(HttpClient cliente)
    {
        _service.Setup(s => s.AutenticarAsync("mestre@x.com", "senha-segura")).ReturnsAsync(Mestre);
        _service.Setup(s => s.ObterAsync(7)).ReturnsAsync(Mestre);
        await RenovarCsrfAsync(cliente);
        return await cliente.PostAsJsonAsync("/api/auth/login", new { email = "mestre@x.com", senha = "senha-segura" });
    }

    [Fact]
    public async Task Login_SemTokenCsrf_Responde400()
    {
        var cliente = CriarCliente();

        var resposta = await cliente.PostAsJsonAsync("/api/auth/login", new { email = "mestre@x.com", senha = "senha-segura" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        _service.Verify(s => s.AutenticarAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Login_ComCsrf_Responde200EGravaCookieHttpOnlyStrict()
    {
        var cliente = CriarCliente();

        var resposta = await EntrarAsync(cliente);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var cookie = resposta.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("dmaegis.sessao="));
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);
        var envelope = await LerEnvelopeAsync(resposta);
        Assert.Equal(0, envelope.GetProperty("responseCode").GetInt32());
        Assert.Equal("Mestre", envelope.GetProperty("data").GetProperty("nome").GetString());
    }

    [Fact]
    public async Task Eu_SemCookie_Responde401ComEnvelope()
    {
        var cliente = CriarCliente();

        var resposta = await cliente.GetAsync("/api/auth/eu");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        var envelope = await LerEnvelopeAsync(resposta);
        Assert.Equal(1, envelope.GetProperty("responseCode").GetInt32());
        Assert.Equal("Faça login para continuar.", envelope.GetProperty("errorMessage").GetString());
    }

    [Fact]
    public async Task Eu_DepoisDoLogin_RetornaUsuario()
    {
        var cliente = CriarCliente();
        await EntrarAsync(cliente);

        var resposta = await cliente.GetAsync("/api/auth/eu");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var envelope = await LerEnvelopeAsync(resposta);
        Assert.Equal("Mestre", envelope.GetProperty("data").GetProperty("nome").GetString());
        Assert.Equal("mestre@x.com", envelope.GetProperty("data").GetProperty("email").GetString());
    }

    [Fact]
    public async Task Eu_UsuarioApagado_Responde401()
    {
        var cliente = CriarCliente();
        await EntrarAsync(cliente);
        _service.Setup(s => s.ObterAsync(7)).ReturnsAsync((UsuarioServiceModel?)null);

        var resposta = await cliente.GetAsync("/api/auth/eu");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Login_CredenciaisInvalidas_Responde401Codigo4()
    {
        var cliente = CriarCliente();
        _service.Setup(s => s.AutenticarAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new BusinessRuleException(BusinessErrors.CredenciaisInvalidas));
        await RenovarCsrfAsync(cliente);

        var resposta = await cliente.PostAsJsonAsync("/api/auth/login", new { email = "mestre@x.com", senha = "errada" });

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        var envelope = await LerEnvelopeAsync(resposta);
        Assert.Equal(4, envelope.GetProperty("responseCode").GetInt32());
        Assert.Equal("E-mail ou senha inválidos.", envelope.GetProperty("errorMessage").GetString());
    }

    [Fact]
    public async Task Cadastro_EmailDuplicado_Responde409Codigo3()
    {
        var cliente = CriarCliente();
        _service.Setup(s => s.CadastrarAsync(It.IsAny<CadastrarUsuarioServiceModel>()))
            .ThrowsAsync(new BusinessRuleException(BusinessErrors.EmailJaCadastrado));
        await RenovarCsrfAsync(cliente);

        var resposta = await cliente.PostAsJsonAsync("/api/auth/cadastro",
            new { nome = "Mestre", email = "mestre@x.com", senha = "senha-segura" });

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        Assert.Equal(3, (await LerEnvelopeAsync(resposta)).GetProperty("responseCode").GetInt32());
    }

    [Fact]
    public async Task Cadastro_Valido_Responde201EJaEntra()
    {
        var cliente = CriarCliente();
        _service.Setup(s => s.CadastrarAsync(It.IsAny<CadastrarUsuarioServiceModel>())).ReturnsAsync(Mestre);
        _service.Setup(s => s.ObterAsync(7)).ReturnsAsync(Mestre);
        await RenovarCsrfAsync(cliente);

        var resposta = await cliente.PostAsJsonAsync("/api/auth/cadastro",
            new { nome = "Mestre", email = "mestre@x.com", senha = "senha-segura" });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync("/api/auth/eu")).StatusCode);
    }

    [Fact]
    public async Task Cadastro_SenhaCurta_Responde400Codigo2()
    {
        var cliente = CriarCliente();
        await RenovarCsrfAsync(cliente);

        var resposta = await cliente.PostAsJsonAsync("/api/auth/cadastro",
            new { nome = "Mestre", email = "mestre@x.com", senha = "curta" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var envelope = await LerEnvelopeAsync(resposta);
        Assert.Equal(2, envelope.GetProperty("responseCode").GetInt32());
        Assert.Contains("8 caracteres", envelope.GetProperty("errorMessage").GetString());
        _service.Verify(s => s.CadastrarAsync(It.IsAny<CadastrarUsuarioServiceModel>()), Times.Never);
    }

    [Fact]
    public async Task Login_SextaTentativaNoMinuto_Responde429()
    {
        var cliente = CriarCliente();
        _service.Setup(s => s.AutenticarAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new BusinessRuleException(BusinessErrors.CredenciaisInvalidas));
        await RenovarCsrfAsync(cliente);

        for (var i = 0; i < 5; i++)
        {
            var tentativa = await cliente.PostAsJsonAsync("/api/auth/login", new { email = "a@x.com", senha = "errada" });
            Assert.Equal(HttpStatusCode.Unauthorized, tentativa.StatusCode);
        }
        var sexta = await cliente.PostAsJsonAsync("/api/auth/login", new { email = "a@x.com", senha = "errada" });

        Assert.Equal((HttpStatusCode)429, sexta.StatusCode);
        Assert.Equal("Muitas tentativas. Aguarde um minuto e tente de novo.",
            (await LerEnvelopeAsync(sexta)).GetProperty("errorMessage").GetString());
    }

    [Fact]
    public async Task Logout_DepoisDoLogin_EuVolta401()
    {
        var cliente = CriarCliente();
        await EntrarAsync(cliente);
        await RenovarCsrfAsync(cliente); // o token anterior era de um usuário anônimo

        var resposta = await cliente.PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync("/api/auth/eu")).StatusCode);
    }
}
