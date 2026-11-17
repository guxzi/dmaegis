import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import * as auth from '../api/auth.js';
import { api } from '../api/cliente.js';
import { criarTratadorDe401 } from '../api/sessao.js';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [usuario, setUsuario] = useState(null);
  const [carregando, setCarregando] = useState(true);

  // Ao abrir o app: primeiro descobre se a sessão vale (o /eu pode encerrá-la),
  // depois pega o token CSRF, que é ligado ao usuário. Nessa ordem, o token sempre
  // corresponde à sessão que sobrou.
  useEffect(() => {
    auth
      .obterUsuarioAtual()
      .then(async (atual) => {
        await auth.renovarCsrf();
        setUsuario(atual);
      })
      .catch(() => setUsuario(null))
      .finally(() => setCarregando(false));
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
    () => ({ usuario, carregando, entrar, cadastrar, sair }),
    [usuario, carregando, entrar, cadastrar, sair],
  );

  return <AuthContext.Provider value={valor}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const contexto = useContext(AuthContext);
  if (!contexto) throw new Error('useAuth precisa estar dentro de <AuthProvider>.');
  return contexto;
}
