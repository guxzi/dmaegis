using System.Net.Http.Json;
using DMAegis.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace DMAegis.UnitTests.WebApi;

/// <summary>
/// API em memória para testes de integração: troca os Services por mocks e ajuda a
/// lidar com o token CSRF e com o login (usuário 7, "Mestre").
/// </summary>
public class ApiDeTeste
{
    public static readonly UsuarioServiceModel Mestre = new() { Id = 7, Nome = "Mestre", Email = "mestre@x.com" };

    public Mock<IUsuarioService> Usuarios { get; } = new();
    public Mock<ICampanhaService> Campanhas { get; } = new();
    public Mock<IRegistroService> Registros { get; } = new();
    public Mock<ISessaoService> Sessoes { get; } = new();
    public Mock<IImagemService> Imagens { get; } = new();
    public Mock<IMesaService> Mesas { get; } = new();
    public Mock<IJogadorService> Jogadores { get; } = new();
    public Mock<IRolagemMesaService> Rolagens { get; } = new();

    public HttpClient CriarCliente()
    {
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => Usuarios.Object);
                services.AddScoped(_ => Campanhas.Object);
                services.AddScoped(_ => Registros.Object);
                services.AddScoped(_ => Sessoes.Object);
                services.AddScoped(_ => Imagens.Object);
                services.AddScoped(_ => Mesas.Object);
                services.AddScoped(_ => Jogadores.Object);
                services.AddScoped(_ => Rolagens.Object);
            }));
        return factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
    }

    /// <summary>Pede um token CSRF e já o coloca no cabeçalho X-XSRF-TOKEN do cliente.</summary>
    public static async Task RenovarCsrfAsync(HttpClient cliente)
    {
        var resposta = await cliente.GetAsync("/api/auth/csrf");
        resposta.EnsureSuccessStatusCode();
        var cookie = resposta.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("XSRF-TOKEN="));
        var token = Uri.UnescapeDataString(cookie.Split(';')[0]["XSRF-TOKEN=".Length..]);
        cliente.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
        cliente.DefaultRequestHeaders.Add("X-XSRF-TOKEN", token);
    }

    /// <summary>Entra como o usuário 7 e deixa o cliente com um token CSRF válido para ele.</summary>
    public async Task<HttpClient> CriarClienteLogadoAsync()
    {
        Usuarios.Setup(s => s.AutenticarAsync("mestre@x.com", "senha-segura")).ReturnsAsync(Mestre);
        Usuarios.Setup(s => s.ObterAsync(7)).ReturnsAsync(Mestre);
        var cliente = CriarCliente();
        await RenovarCsrfAsync(cliente);
        var login = await cliente.PostAsJsonAsync("/api/auth/login", new { email = "mestre@x.com", senha = "senha-segura" });
        login.EnsureSuccessStatusCode();
        await RenovarCsrfAsync(cliente);
        return cliente;
    }
}
