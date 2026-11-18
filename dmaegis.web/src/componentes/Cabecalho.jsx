import { useState } from 'react';
import { Link } from 'react-router';
import { useAuth } from '../auth/AuthContext.jsx';
import BuscaRapida from '../escudo/BuscaRapida.jsx';
import Emblema from './Emblema.jsx';

// Cabeçalho de todas as páginas logadas. Com `idCampanha`, ganha a busca rápida (Ctrl+K).
export default function Cabecalho({ idCampanha, aoEscolherRegistro, className = 'mb-4' }) {
  const { usuario, sair } = useAuth();
  const [buscaAberta, setBuscaAberta] = useState(false);
  const ehMac = typeof navigator !== 'undefined' && /Mac|iPhone|iPad/.test(navigator.platform);

  return (
    <nav className={`navbar navbar-astral sticky-top ${className}`}>
      <div className="container-fluid px-3 px-lg-4">
        <Link to="/" className="navbar-brand d-flex align-items-center gap-2">
          <Emblema tamanho={30} />
          DMAegis
        </Link>
        <div className="d-flex align-items-center gap-3">
          {idCampanha && (
            <button type="button" className="btn btn-sm btn-outline-primary d-flex align-items-center gap-2"
              onClick={() => setBuscaAberta(true)}>
              Buscar <kbd className="small">{ehMac ? '⌘' : 'Ctrl'} K</kbd>
            </button>
          )}
          <span className="text-secondary d-none d-md-inline">Olá, {usuario.nome}</span>
          <button type="button" className="btn btn-sm btn-outline-secondary" onClick={sair}>
            Sair
          </button>
        </div>
      </div>
      {idCampanha && (
        <BuscaRapida idCampanha={idCampanha} aoEscolherRegistro={aoEscolherRegistro}
          aberta={buscaAberta} aoMudarAberta={setBuscaAberta} />
      )}
    </nav>
  );
}
