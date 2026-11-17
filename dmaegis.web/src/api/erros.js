// Transforma um erro do Axios numa mensagem para mostrar ao mestre.
export function mensagemDeErro(erro) {
  const resposta = erro?.response;

  if (!resposta) {
    return 'Não foi possível falar com o servidor.';
  }
  if (resposta.status === 429) {
    return 'Muitas tentativas. Aguarde um minuto e tente de novo.';
  }
  return resposta.data?.errorMessage || 'Algo deu errado. Tente de novo.';
}
