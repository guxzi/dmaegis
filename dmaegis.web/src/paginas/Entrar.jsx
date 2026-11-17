import { useState } from 'react';
import { Link } from 'react-router';
import { mensagemDeErro } from '../api/erros.js';
import { useAuth } from '../auth/AuthContext.jsx';
import FormularioConta from './FormularioConta.jsx';

export default function Entrar() {
  const { entrar } = useAuth();
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [erro, setErro] = useState('');
  const [enviando, setEnviando] = useState(false);

  async function aoEnviar(evento) {
    evento.preventDefault();
    setErro('');
    setEnviando(true);
    try {
      await entrar({ email, senha });
      // A RotaPublica leva para o início assim que o usuário fica definido.
    } catch (e) {
      setErro(mensagemDeErro(e));
      setEnviando(false);
    }
  }

  return (
    <FormularioConta
      titulo="Entre para abrir o seu escudo."
      erro={erro}
      enviando={enviando}
      textoBotao="Entrar"
      aoEnviar={aoEnviar}
      rodape={<>Ainda não tem conta? <Link to="/cadastro">Criar conta</Link></>}
    >
      <div className="mb-3">
        <label htmlFor="email" className="form-label">E-mail</label>
        <input id="email" type="email" className="form-control" autoComplete="email" required
          value={email} onChange={(e) => setEmail(e.target.value)} />
      </div>
      <div className="mb-4">
        <label htmlFor="senha" className="form-label">Senha</label>
        <input id="senha" type="password" className="form-control" autoComplete="current-password" required
          value={senha} onChange={(e) => setSenha(e.target.value)} />
      </div>
    </FormularioConta>
  );
}
