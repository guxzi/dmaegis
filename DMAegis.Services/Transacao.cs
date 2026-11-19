using System.Transactions;

namespace DMAegis.Services;

public static class Transacao
{
    /// <summary>
    /// Abre uma TransactionScope no formato das convenções (docs/CONVENCOES.md). Uso:
    /// <c>using var transacao = Transacao.Iniciar(); … transacao.Complete();</c>
    /// As conexões abertas em sequência dentro dela usam a mesma sessão do MySQL
    /// (conferido em 17/11/2026), e sem Complete() tudo é desfeito.
    /// </summary>
    public static TransactionScope Iniciar() => new(
        TransactionScopeOption.Required,
        new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted,
            Timeout = TransactionManager.DefaultTimeout,
        },
        TransactionScopeAsyncFlowOption.Enabled);
}
