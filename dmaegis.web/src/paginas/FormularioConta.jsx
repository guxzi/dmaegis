// Moldura comum às telas de entrar e de cadastro.
export default function FormularioConta({ titulo, erro, enviando, textoBotao, aoEnviar, rodape, children }) {
  return (
    <main className="container py-5">
      <div className="card shadow-sm mx-auto" style={{ maxWidth: 420 }}>
        <div className="card-body p-4">
          <h1 className="h3 mb-1">DMAegis</h1>
          <p className="text-secondary mb-4">{titulo}</p>

          {erro && (
            <div className="alert alert-danger" role="alert">
              {erro}
            </div>
          )}

          <form onSubmit={aoEnviar} noValidate>
            {children}
            <button type="submit" className="btn btn-primary w-100" disabled={enviando}>
              {enviando ? 'Aguarde…' : textoBotao}
            </button>
          </form>

          <p className="text-center mt-3 mb-0">{rodape}</p>
        </div>
      </div>
    </main>
  );
}
