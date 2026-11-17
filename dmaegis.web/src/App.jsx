import { Navigate, Route, Routes } from 'react-router';
import { RotaProtegida, RotaPublica } from './auth/RotaProtegida.jsx';
import Cadastro from './paginas/Cadastro.jsx';
import Entrar from './paginas/Entrar.jsx';
import Inicio from './paginas/Inicio.jsx';

export default function App() {
  return (
    <Routes>
      <Route path="/entrar" element={<RotaPublica><Entrar /></RotaPublica>} />
      <Route path="/cadastro" element={<RotaPublica><Cadastro /></RotaPublica>} />
      <Route path="/" element={<RotaProtegida><Inicio /></RotaProtegida>} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
