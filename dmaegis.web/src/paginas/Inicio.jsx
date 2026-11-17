import { useEffect, useState } from 'react';
import { api } from '../api/cliente.js';
import { useAuth } from '../auth/AuthContext.jsx';

// Página provisória: saudação, botão de sair e o estado da API e do banco.
export default function Inicio() {
  const { usuario, sair } = useAuth();
  const [estado, setEstado] = useState({ carregando: true });

  useEffect(() => {
    api
      .get('/saude')
      .then((resposta) => setEstado({ carregando: false, saude: resposta.data.data }))
      .catch(() => setEstado({ carregando: false, erro: true }));
  }, []);

  let mensagem;
  let cor;
  if (estado.carregando) {
    mensagem = 'Verificando…';
    cor = 'secondary';
  } else if (estado.erro) {
    mensagem = 'API fora do ar';
    cor = 'danger';
  } else {
    mensagem = `API ok · Banco ${estado.saude.banco ? 'ok' : 'indisponível'}`;
    cor = estado.saude.banco ? 'success' : 'warning';
  }

  return (
    <main className="container py-5">
      <div className="d-flex justify-content-between align-items-start">
        <div>
          <h1 className="display-5">DMAegis</h1>
          <p className="lead mb-2">Olá, {usuario.nome}!</p>
        </div>
        <button type="button" className="btn btn-outline-secondary" onClick={sair}>
          Sair
        </button>
      </div>
      <span className={`badge text-bg-${cor} fs-6`}>{mensagem}</span>
    </main>
  );
}
