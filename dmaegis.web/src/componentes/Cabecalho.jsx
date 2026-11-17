import { Link } from 'react-router';
import { useAuth } from '../auth/AuthContext.jsx';

export default function Cabecalho() {
  const { usuario, sair } = useAuth();

  return (
    <nav className="navbar bg-body-tertiary border-bottom mb-4">
      <div className="container">
        <Link to="/" className="navbar-brand fw-semibold">DMAegis</Link>
        <div className="d-flex align-items-center gap-3">
          <span className="text-secondary d-none d-sm-inline">Olá, {usuario.nome}</span>
          <button type="button" className="btn btn-sm btn-outline-secondary" onClick={sair}>
            Sair
          </button>
        </div>
      </div>
    </nav>
  );
}
