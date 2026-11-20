import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import * as auth from '../api/auth.js';
import { api } from '../api/cliente.js';
import { criarTratadorDe401, semConexao } from '../api/sessao.js';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [usuario, setUsuario] = useState(null);
  const [carregando, setCarregando] = useState(true);
  const [reconectando, setReconectando] = useState(false);

  // Ao abrir o app: primeiro descobre se a sessão vale (o /eu pode encerrá-la),
  // depois pega o token CSRF, que é ligado ao usuário. Nessa ordem, o token sempre
  // corresponde à sessão que sobrou. Se a API estiver fora do ar, tenta de novo a cada
  // 3 segundos em vez de mandar para o login (a sessão pode muito bem continuar valendo).
  useEffect(() => {
    let ativo = true;
    let espera = null;
    async function verificar() {
      try {
        const atual = await auth.obterUsuarioAtual();
        await auth.renovarCsrf();
        if (!ativo) return;
        setUsuario(atual);
        setReconectando(false);
        setCarregando(false);
      } catch (erro) {
        if (!ativo) return;
        if (semConexao(erro)) {
          setReconectando(true);
          espera = setTimeout(verificar, 3000);
        } else {
          setUsuario(null);
          setReconectando(false);
          setCarregando(false);
        }
      }
    }
    verificar();
    return () => {
      ativo = false;
      clearTimeout(espera);
    };
  }, []);

  // Sessão expirada no meio do uso: qualquer 401 fora das rotas de auth renova o token
  // CSRF (agora anônimo, para o próximo login funcionar) e desloga.
  useEffect(() => {
    const aoExpirar = async () => {
      setUsuario(null);
      await auth.renovarCsrf();
    };
    const id = api.interceptors.response.use((resposta) => resposta, criarTratadorDe401(aoExpirar));
    return () => api.interceptors.response.eject(id);
  }, []);

  const entrar = useCallback(async (dados) => setUsuario(await auth.entrar(dados)), []);
  const cadastrar = useCallback(async (dados) => setUsuario(await auth.cadastrar(dados)), []);
  const sair = useCallback(async () => {
    await auth.sair();
    setUsuario(null);
  }, []);

  const valor = useMemo(
    () => ({ usuario, carregando, reconectando, entrar, cadastrar, sair }),
    [usuario, carregando, reconectando, entrar, cadastrar, sair],
  );

  return <AuthContext.Provider value={valor}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const contexto = useContext(AuthContext);
  if (!contexto) throw new Error('useAuth precisa estar dentro de <AuthProvider>.');
  return contexto;
}
