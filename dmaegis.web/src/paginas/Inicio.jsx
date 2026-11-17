import { useEffect, useState } from 'react';
import { api } from '../api/cliente.js';

// Página provisória: mostra se a API e o banco estão respondendo.
export default function Inicio() {
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
      <h1 className="display-5">DMAegis</h1>
      <p className="lead">Escudo do mestre para D&amp;D 5e e Daggerheart.</p>
      <span className={`badge text-bg-${cor} fs-6`}>{mensagem}</span>
    </main>
  );
}
