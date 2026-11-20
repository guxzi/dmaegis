using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using DMAegis.Services;

namespace DMAegis.WebApi.Configuration;

/// <summary>
/// Fluxo de Server-Sent Events da mesa: um evento "mesa" a cada mudança (só a versão, sem dados),
/// um "sinal" para os efeitos passageiros (ping no mapa, reação, arrasto ao vivo)
/// e um "ping" a cada 20 s, para túneis e proxies não fecharem a conexão parada.
/// </summary>
public static class EventosMesa
{
    public static readonly TimeSpan IntervaloPing = TimeSpan.FromSeconds(20);

    public static IResult Fluxo(ICanalMesa canal, int idCampanha, bool mestre, CancellationToken cancelamento) =>
        TypedResults.ServerSentEvents(Eventos(canal, idCampanha, mestre, cancelamento));

    private static async IAsyncEnumerable<SseItem<string>> Eventos(
        ICanalMesa canal, int idCampanha, bool mestre, [EnumeratorCancellation] CancellationToken cancelamento)
    {
        var (leitor, inscricao) = canal.Assinar(idCampanha, mestre);
        using (inscricao)
        {
            yield return new SseItem<string>("0", "pronto");
            while (!cancelamento.IsCancellationRequested)
            {
                AvisoMesa? aviso = null;
                using var espera = CancellationTokenSource.CreateLinkedTokenSource(cancelamento);
                espera.CancelAfter(IntervaloPing);
                try
                {
                    aviso = await leitor.ReadAsync(espera.Token);
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

                yield return aviso is null
                    ? new SseItem<string>("", "ping")
                    : new SseItem<string>(aviso.Dados, aviso.Evento);
            }
        }
    }
}
