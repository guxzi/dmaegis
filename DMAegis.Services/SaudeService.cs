using DMAegis.Repository;

namespace DMAegis.Services;

public class SaudeServiceModel
{
    public bool Api { get; set; }
    public bool Banco { get; set; }
}

public interface ISaudeService
{
    Task<SaudeServiceModel> VerificarAsync();
}

public class SaudeService : ISaudeService
{
    private readonly ISaudeRepository _saudeRepository;

    public SaudeService(ISaudeRepository saudeRepository)
    {
        _saudeRepository = saudeRepository;
    }

    public async Task<SaudeServiceModel> VerificarAsync() => new()
    {
        Api = true,
        Banco = await _saudeRepository.BancoRespondeAsync(),
    };
}
