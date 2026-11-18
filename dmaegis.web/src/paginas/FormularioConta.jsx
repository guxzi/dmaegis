import Emblema from '../componentes/Emblema.jsx';

// Moldura comum às telas de entrar e de cadastro.
export default function FormularioConta({ titulo, erro, enviando, textoBotao, aoEnviar, rodape, children }) {
  return (
    <main className="container py-5">
      <div className="card cartao-astral mx-auto mt-md-4" style={{ maxWidth: 420 }}>
        <div className="card-body p-4 p-md-5">
          <div className="text-center mb-4">
            <Emblema tamanho={76} className="mb-3" />
            <h1 className="h2 mb-1">DMAegis</h1>
            <p className="text-secondary mb-0">{titulo}</p>
          </div>
          <div className="divisor-estelar mb-4">✦</div>

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

          <p className="text-center mt-4 mb-0">{rodape}</p>
        </div>
      </div>
    </main>
  );
}
