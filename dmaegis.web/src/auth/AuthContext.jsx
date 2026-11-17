import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import * as auth from '../api/auth.js';
import { api } from '../api/cliente.js';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [usuario, setUsuario] = useState(null);
  const [carregando, setCarregando] = useState(true);

  // Ao abrir o app: pega o token CSRF e descobre se já existe uma sessão.
  useEffect(() => {
    auth
      .renovarCsrf()
      .then(() => auth.obterUsuarioAtual())
      .then(setUsuario)
      .catch(() => setUsuario(null))
      .finally(() => setCarregando(false));
  }, []);

  // Sessão expirada no meio do uso: qualquer 401 fora das rotas de auth desloga.
  useEffect(() => {
    const id = api.interceptors.response.use(
      (resposta) => resposta,
      (erro) => {
        const url = erro.config?.url ?? '';
        if (erro.response?.status === 401 && !url.startsWith('/auth/')) {
          setUsuario(null);
        }
        return Promise.reject(erro);
      },
    );
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
