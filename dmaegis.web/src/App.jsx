import { Navigate, Route, Routes } from 'react-router';
import { RotaProtegida, RotaPublica } from './auth/RotaProtegida.jsx';
import CeuEstrelado from './componentes/CeuEstrelado.jsx';
import FaiscaClique from './componentes/FaiscaClique.jsx';
import Caderno from './paginas/Caderno.jsx';
import CampanhaDetalhe from './paginas/CampanhaDetalhe.jsx';
import Campanhas from './paginas/Campanhas.jsx';
import Cadastro from './paginas/Cadastro.jsx';
import Entrar from './paginas/Entrar.jsx';
import RegistroDetalhe from './paginas/RegistroDetalhe.jsx';
import RegistroEditar from './paginas/RegistroEditar.jsx';
import Escudo from './paginas/Escudo.jsx';
import Galeria from './paginas/Galeria.jsx';
import Mesa from './paginas/Mesa.jsx';
import Convite from './paginas/Convite.jsx';
import Jogar from './paginas/Jogar.jsx';
import Projecao from './paginas/Projecao.jsx';
import SessaoPreparo from './paginas/SessaoPreparo.jsx';
import Sessoes from './paginas/Sessoes.jsx';

const protegida = (pagina) => <RotaProtegida>{pagina}</RotaProtegida>;

export default function App() {
  return (
    <>
      <CeuEstrelado />
      <FaiscaClique />
      <Routes>
        <Route path="/entrar" element={<RotaPublica><Entrar /></RotaPublica>} />
        <Route path="/cadastro" element={<RotaPublica><Cadastro /></RotaPublica>} />
        <Route path="/" element={protegida(<Campanhas />)} />
        <Route path="/campanhas/:id" element={protegida(<CampanhaDetalhe />)} />
        <Route path="/campanhas/:id/caderno" element={protegida(<Caderno />)} />
        <Route path="/campanhas/:id/caderno/novo" element={protegida(<RegistroEditar />)} />
        <Route path="/campanhas/:id/caderno/:idRegistro" element={protegida(<RegistroDetalhe />)} />
        <Route path="/campanhas/:id/caderno/:idRegistro/editar" element={protegida(<RegistroEditar />)} />
        <Route path="/campanhas/:id/sessoes" element={protegida(<Sessoes />)} />
        <Route path="/campanhas/:id/sessoes/:idSessao" element={protegida(<SessaoPreparo />)} />
        <Route path="/campanhas/:id/escudo" element={protegida(<Escudo />)} />
        <Route path="/campanhas/:id/galeria" element={protegida(<Galeria />)} />
        <Route path="/campanhas/:id/mesa" element={protegida(<Mesa />)} />
        {/* Jogadores: entram pelo link de convite, sem conta. */}
        <Route path="/convite/:codigo" element={<Convite />} />
        <Route path="/jogar" element={<Jogar />} />
        <Route path="/projecao" element={protegida(<Projecao />)} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </>
  );
}
