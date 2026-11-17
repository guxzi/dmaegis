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
