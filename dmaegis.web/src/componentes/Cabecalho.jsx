import { Link } from 'react-router';
import { useAuth } from '../auth/AuthContext.jsx';
import Emblema from './Emblema.jsx';

export default function Cabecalho() {
  const { usuario, sair } = useAuth();

  return (
    <nav className="navbar navbar-astral sticky-top mb-4">
      <div className="container">
        <Link to="/" className="navbar-brand d-flex align-items-center gap-2">
          <Emblema tamanho={30} />
          DMAegis
        </Link>
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
