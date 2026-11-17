import { Navigate, Route, Routes } from 'react-router';
import { RotaProtegida, RotaPublica } from './auth/RotaProtegida.jsx';
import Caderno from './paginas/Caderno.jsx';
import CampanhaDetalhe from './paginas/CampanhaDetalhe.jsx';
import Campanhas from './paginas/Campanhas.jsx';
import Cadastro from './paginas/Cadastro.jsx';
import Entrar from './paginas/Entrar.jsx';
import RegistroDetalhe from './paginas/RegistroDetalhe.jsx';
import RegistroEditar from './paginas/RegistroEditar.jsx';

const protegida = (pagina) => <RotaProtegida>{pagina}</RotaProtegida>;

export default function App() {
  return (
    <Routes>
      <Route path="/entrar" element={<RotaPublica><Entrar /></RotaPublica>} />
      <Route path="/cadastro" element={<RotaPublica><Cadastro /></RotaPublica>} />
      <Route path="/" element={protegida(<Campanhas />)} />
      <Route path="/campanhas/:id" element={protegida(<CampanhaDetalhe />)} />
      <Route path="/campanhas/:id/caderno" element={protegida(<Caderno />)} />
      <Route path="/campanhas/:id/caderno/novo" element={protegida(<RegistroEditar />)} />
      <Route path="/campanhas/:id/caderno/:idRegistro" element={protegida(<RegistroDetalhe />)} />
      <Route path="/campanhas/:id/caderno/:idRegistro/editar" element={protegida(<RegistroEditar />)} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
