import { useState } from 'react';
import { Link, NavLink } from 'react-router';
import { useAuth } from '../auth/AuthContext.jsx';
import BuscaRapida from '../escudo/BuscaRapida.jsx';
import Emblema from './Emblema.jsx';

// Cabeçalho de todas as páginas logadas. Com `idCampanha`, ganha a navegação da campanha
// (sublinhado brilhante na página ativa) e a busca rápida (Ctrl+K).
export default function Cabecalho({ idCampanha, aoEscolherRegistro, className = 'mb-4' }) {
  const { usuario, sair } = useAuth();
  const [buscaAberta, setBuscaAberta] = useState(false);
  const ehMac = typeof navigator !== 'undefined' && /Mac|iPhone|iPad/.test(navigator.platform);
  const base = `/campanhas/${idCampanha}`;

  return (
    <nav className={`navbar navbar-astral sticky-top py-2 ${className}`}>
      <div className="container-fluid px-3 px-lg-4 gap-2 flex-nowrap">
        <Link to="/" className="navbar-brand d-flex align-items-center gap-2 me-2 flex-shrink-0">
          <Emblema tamanho={30} />
          <span className="d-none d-sm-inline">DMAegis</span>
        </Link>

        {idCampanha && (
          <div className="nav-campanha mx-lg-auto flex-grow-1 flex-lg-grow-0">
            <NavLink to={base} end>Visão geral</NavLink>
            <NavLink to={`${base}/escudo`}>Escudo</NavLink>
            <NavLink to={`${base}/caderno`}>Caderno</NavLink>
            <NavLink to={`${base}/galeria`}>Galeria</NavLink>
            <NavLink to={`${base}/sessoes`}>Sessões</NavLink>
          </div>
        )}

        <div className={`d-flex align-items-center gap-2 flex-shrink-0 ${idCampanha ? '' : 'ms-auto'}`}>
          {idCampanha && (
            <button type="button" className="botao-busca" onClick={() => setBuscaAberta(true)} aria-label="Buscar (Ctrl+K)">
              <span className="d-none d-md-inline">Buscar</span>
              <kbd className="small">{ehMac ? '⌘' : 'Ctrl'} K</kbd>
            </button>
          )}
          <span className="text-secondary d-none d-xl-inline ms-2">{usuario.nome}</span>
          <button type="button" className="btn btn-sm btn-fantasma" onClick={sair}>Sair</button>
        </div>
      </div>
      {idCampanha && (
        <BuscaRapida idCampanha={idCampanha} aoEscolherRegistro={aoEscolherRegistro}
          aberta={buscaAberta} aoMudarAberta={setBuscaAberta} />
      )}
    </nav>
  );
}
