// Tratador de erro para o interceptor do Axios: quando uma chamada fora de /auth
// recebe 401, a sessão expirou. Chama aoExpirar (que renova o token CSRF e limpa o usuário)
// e repassa o erro original para quem fez a chamada.
export function criarTratadorDe401(aoExpirar) {
  return async (erro) => {
    const url = erro?.config?.url ?? '';
    if (erro?.response?.status === 401 && !url.startsWith('/auth/')) {
      try {
        await aoExpirar();
      } catch {
        // Mesmo se a renovação falhar, quem chamou recebe o erro original.
      }
    }
    return Promise.reject(erro);
  };
}

// O servidor não respondeu (API fora do ar, rede caiu) ou deu erro interno: vale tentar de novo.
// Um 401 é diferente: a sessão não vale mais, e aí o certo é ir para o login.
export function semConexao(erro) {
  if (!erro) return false;
  const status = erro.response?.status;
  return status === undefined || status >= 500;
}
