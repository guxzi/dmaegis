import { useState } from 'react';
import { Link } from 'react-router';
import { mensagemDeErro } from '../api/erros.js';
import { useAuth } from '../auth/AuthContext.jsx';
import FormularioConta from './FormularioConta.jsx';

export default function Cadastro() {
  const { cadastrar } = useAuth();
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [erro, setErro] = useState('');
  const [enviando, setEnviando] = useState(false);

  async function aoEnviar(evento) {
    evento.preventDefault();
    setErro('');
    setEnviando(true);
    try {
      await cadastrar({ nome, email, senha });
    } catch (e) {
      setErro(mensagemDeErro(e));
      setEnviando(false);
    }
  }

  return (
    <FormularioConta
      titulo="Crie a sua conta de mestre."
      erro={erro}
      enviando={enviando}
      textoBotao="Criar conta"
      aoEnviar={aoEnviar}
      rodape={<>Já tem conta? <Link to="/entrar">Entrar</Link></>}
    >
      <div className="mb-3">
        <label htmlFor="nome" className="form-label">Nome</label>
        <input id="nome" type="text" className="form-control" autoComplete="name" maxLength={100} required
          value={nome} onChange={(e) => setNome(e.target.value)} />
      </div>
      <div className="mb-3">
        <label htmlFor="email" className="form-label">E-mail</label>
        <input id="email" type="email" className="form-control" autoComplete="email" maxLength={254} required
          value={email} onChange={(e) => setEmail(e.target.value)} />
      </div>
      <div className="mb-4">
        <label htmlFor="senha" className="form-label">Senha</label>
        <input id="senha" type="password" className="form-control" autoComplete="new-password"
          minLength={8} maxLength={100} required aria-describedby="ajudaSenha"
          value={senha} onChange={(e) => setSenha(e.target.value)} />
        <div id="ajudaSenha" className="form-text">Pelo menos 8 caracteres.</div>
      </div>
    </FormularioConta>
  );
}
