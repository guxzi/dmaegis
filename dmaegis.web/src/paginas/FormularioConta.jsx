import Emblema from '../componentes/Emblema.jsx';
import PoeiraEstelar from '../visual/PoeiraEstelar.jsx';

const DESTAQUES = [
  { titulo: 'O escudo da sessão', texto: 'Começo forte, cenas, segredos e NPCs à vista, sem trocar de aba.' },
  { titulo: 'Um caderno vivo', texto: 'NPCs, locais, facções e mapas da campanha, a um Ctrl+K de distância.' },
  { titulo: 'Dados na mão', texto: 'Notação livre, vantagem e desvantagem, direto no navegador.' },
];

// Moldura das telas de entrar e de cadastro: hero com o nome do app à esquerda e o formulário à direita.
export default function FormularioConta({ titulo, erro, enviando, textoBotao, aoEnviar, rodape, children }) {
  return (
    <main className="position-relative overflow-hidden" style={{ minHeight: '100vh' }}>
      <PoeiraEstelar quantidade={70} />
      <div className="container position-relative py-5" style={{ zIndex: 1 }}>
        <div className="row align-items-center g-5" style={{ minHeight: 'calc(100vh - 6rem)' }}>
          <div className="col-lg-7 surgir">
            <span className="chamada">O escudo do mestre</span>
            <h1 className="hero-titulo titulo-estelar" style={{ fontSize: 'clamp(3.2rem, 8vw, 6.2rem)' }}>
              DM<span className="destaque">Aegis</span>
            </h1>
            <p className="fs-5 text-secondary mb-4" style={{ maxWidth: 560 }}>
              Prepare a sessão, conduza o jogo e improvise sem perder o fio: tudo o que você precisa atrás de um só escudo.
            </p>
            <ul className="list-unstyled d-grid gap-3 mb-0" style={{ maxWidth: 560 }}>
              {DESTAQUES.map((d) => (
                <li key={d.titulo} className="d-flex gap-3 align-items-start">
                  <span className="selo-escudo flex-shrink-0 mt-1" aria-hidden="true">✦</span>
                  <span>
                    <strong className="d-block titulo-runico" style={{ color: '#fff' }}>{d.titulo}</strong>
                    <span className="text-secondary">{d.texto}</span>
                  </span>
                </li>
              ))}
            </ul>
          </div>

          <div className="col-lg-5 surgir surgir-2">
            <div className="card cartao-astral mx-auto" style={{ maxWidth: 440 }}>
              <div className="card-body p-4 p-md-5">
                <div className="text-center mb-4">
                  <Emblema tamanho={64} className="mb-3" />
                  <p className="text-secondary mb-0">{titulo}</p>
                </div>
                <div className="divisor-estelar mb-4">✦</div>

                {erro && <div className="alert alert-danger" role="alert">{erro}</div>}

                <form onSubmit={aoEnviar} noValidate>
                  {children}
                  <button type="submit" className="btn btn-primary btn-lg w-100" disabled={enviando}>
                    {enviando ? 'Aguarde…' : textoBotao}
                  </button>
                </form>

                <p className="text-center mt-4 mb-0">{rodape}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>
  );
}
