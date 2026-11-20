import { Navigate } from 'react-router';
import { useAuth } from './AuthContext.jsx';

export function Carregando() {
  return (
    <div className="d-flex justify-content-center py-5">
      <div className="spinner-border text-secondary" role="status">
        <span className="visually-hidden">Carregando…</span>
      </div>
    </div>
  );
}

// Enquanto a API não responde: avisa e espera, sem mandar para o login.
export function Reconectando() {
  return (
    <div className="tela-jogador-espera">
      <div className="cartao-astral tela-jogador-cartao text-center">
        <div className="spinner-border text-secondary mb-3" role="status" aria-hidden="true" />
        <h1 className="h5 titulo-estelar">Reconectando ao servidor…</h1>
        <p className="text-secondary mb-0">A API não respondeu. Assim que ela voltar, a página continua sozinha.</p>
      </div>
    </div>
  );
}

// Só mostra a página para quem está logado.
export function RotaProtegida({ children }) {
  const { usuario, carregando, reconectando } = useAuth();
  if (reconectando) return <Reconectando />;
  if (carregando) return <Carregando />;
  if (!usuario) return <Navigate to="/entrar" replace />;
  return children;
}

// Páginas de entrar/cadastro: quem já está logado vai para o início.
export function RotaPublica({ children }) {
  const { usuario, carregando, reconectando } = useAuth();
  if (reconectando) return <Reconectando />;
  if (carregando) return <Carregando />;
  if (usuario) return <Navigate to="/" replace />;
  return children;
}
