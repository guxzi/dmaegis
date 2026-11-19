// Faísca de luz estelar que aparece onde o mestre clica (só em botões e links).

const CLICAVEIS = 'button, a, [role="button"], summary, label';

// Deslocamento final de cada raio, começando no topo e girando no sentido horário.
export function raiosDaFaisca(quantidade, distancia) {
  return Array.from({ length: quantidade }, (_, i) => {
    const angulo = (i / quantidade) * 2 * Math.PI;
    return { x: Math.sin(angulo) * distancia, y: -Math.cos(angulo) * distancia };
  });
}

// O elemento clicável que recebeu o clique, ou null quando não deve haver faísca.
export function alvoDaFaisca(elemento) {
  const alvo = elemento?.closest?.(CLICAVEIS) ?? null;
  if (!alvo || alvo.disabled || alvo.getAttribute('aria-disabled') === 'true') return null;
  return alvo;
}
