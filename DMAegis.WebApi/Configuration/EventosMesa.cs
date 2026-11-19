using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using DMAegis.Services;

namespace DMAegis.WebApi.Configuration;

/// <summary>
/// Fluxo de Server-Sent Events da mesa: um evento "mesa" a cada mudança (só a versão, sem dados)
/// e um "ping" a cada 20 s, para túneis e proxies não fecharem a conexão parada.
/// </summary>
public static class EventosMesa
{
    public static readonly TimeSpan IntervaloPing = TimeSpan.FromSeconds(20);

    public static IResult Fluxo(ICanalMesa canal, int idCampanha, CancellationToken cancelamento) =>
        TypedResults.ServerSentEvents(Eventos(canal, idCampanha, cancelamento));

    private static async IAsyncEnumerable<SseItem<string>> Eventos(
        ICanalMesa canal, int idCampanha, [EnumeratorCancellation] CancellationToken cancelamento)
    {
        var (leitor, inscricao) = canal.Assinar(idCampanha);
        using (inscricao)
        {
            yield return new SseItem<string>("0", "pronto");
            while (!cancelamento.IsCancellationRequested)
            {
                long? versao = null;
                using var espera = CancellationTokenSource.CreateLinkedTokenSource(cancelamento);
                espera.CancelAfter(IntervaloPing);
                try
                {
                    versao = await leitor.ReadAsync(espera.Token);
                }
                catch (OperationCanceledException) when (!cancelamento.IsCancellationRequested)
                {
                    // Passou o intervalo sem mudança: manda um ping.
                }
                catch (OperationCanceledException)
                {
                    yield break;
                }
                catch (System.Threading.Channels.ChannelClosedException)
                {
                    yield break;
                }

                yield return versao is null
                    ? new SseItem<string>("", "ping")
                    : new SseItem<string>(versao.Value.ToString(), "mesa");
            }
        }
    }
}
