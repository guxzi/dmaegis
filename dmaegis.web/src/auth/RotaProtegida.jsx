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

// Só mostra a página para quem está logado.
export function RotaProtegida({ children }) {
  const { usuario, carregando } = useAuth();
  if (carregando) return <Carregando />;
  if (!usuario) return <Navigate to="/entrar" replace />;
  return children;
}

// Páginas de entrar/cadastro: quem já está logado vai para o início.
export function RotaPublica({ children }) {
  const { usuario, carregando } = useAuth();
  if (carregando) return <Carregando />;
  if (usuario) return <Navigate to="/" replace />;
  return children;
}
