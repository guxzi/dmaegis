import { useEffect, useRef, useState } from 'react';
import { useNavigate, useParams } from 'react-router';
import { mensagemDeErro } from '../api/erros.js';
import { entrarComConvite, renovarCsrfJogador } from '../api/jogador.js';
import Emblema from '../componentes/Emblema.jsx';

// /convite/:codigo — o jogador abre o link que o mestre mandou e entra na mesa, sem criar conta.
export default function Convite() {
  const { codigo } = useParams();
  const navegar = useNavigate();
  const [erro, setErro] = useState('');
  const tentou = useRef(false);

  useEffect(() => {
    if (tentou.current) return;
    tentou.current = true;
    (async () => {
      try {
        await renovarCsrfJogador();
        await entrarComConvite(codigo);
        navegar('/jogar', { replace: true });
      } catch (e) {
        setErro(mensagemDeErro(e));
      }
    })();
  }, [codigo, navegar]);

  return (
    <main className="tela-jogador-espera">
      <div className="cartao-astral tela-jogador-cartao text-center">
        <Emblema tamanho={96} className="projecao-emblema mb-3" />
        {erro ? (
          <>
            <h1 className="h4 titulo-estelar">Não deu para entrar</h1>
            <p className="text-secondary mb-0">{erro}</p>
          </>
        ) : (
          <>
            <h1 className="h4 titulo-estelar">Abrindo a mesa…</h1>
            <p className="text-secondary mb-0">Conferindo o seu convite.</p>
          </>
        )}
      </div>
    </main>
  );
}
