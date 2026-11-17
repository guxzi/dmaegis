import { Navigate, Route, Routes } from 'react-router';
import { RotaProtegida, RotaPublica } from './auth/RotaProtegida.jsx';
import CampanhaDetalhe from './paginas/CampanhaDetalhe.jsx';
import Campanhas from './paginas/Campanhas.jsx';
import Cadastro from './paginas/Cadastro.jsx';
import Entrar from './paginas/Entrar.jsx';

export default function App() {
  return (
    <Routes>
      <Route path="/entrar" element={<RotaPublica><Entrar /></RotaPublica>} />
      <Route path="/cadastro" element={<RotaPublica><Cadastro /></RotaPublica>} />
      <Route path="/" element={<RotaProtegida><Campanhas /></RotaProtegida>} />
      <Route path="/campanhas/:id" element={<RotaProtegida><CampanhaDetalhe /></RotaProtegida>} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
