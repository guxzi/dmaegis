// Constelação determinística para a capa de uma campanha: a mesma semente (o id) gera sempre o mesmo desenho.
// Área de 400x220, com margem de 20. A matiz fica entre cobalto (215) e violeta (275).

// PRNG pequeno e determinístico (mulberry32).
function gerador(semente) {
  let a = (semente * 2654435761) >>> 0;
  return () => {
    a = (a + 0x6d2b79f5) >>> 0;
    let t = a;
    t = Math.imul(t ^ (t >>> 15), t | 1);
    t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

export function constelacaoDaSemente(semente) {
  const aleatorio = gerador(Number(semente) || 1);
  const quantidade = 6 + Math.floor(aleatorio() * 4); // 6 a 9 estrelas

  // Espalha da esquerda para a direita, com altura livre: parece uma constelação, não uma nuvem.
  const estrelas = Array.from({ length: quantidade }, (_, i) => ({
    x: Math.round(20 + ((i + 0.2 + aleatorio() * 0.6) / quantidade) * 360),
    y: Math.round(20 + aleatorio() * 180),
    brilho: 0.55 + aleatorio() * 0.45,
    raio: 1.4 + aleatorio() * 1.8,
  }));

  // Caminho principal ligando todas em ordem, mais um ou dois ramos.
  const linhas = estrelas.slice(1).map((_, i) => [i, i + 1]);
  const ramos = 1 + Math.floor(aleatorio() * 2);
  for (let r = 0; r < ramos; r++) {
    const a = Math.floor(aleatorio() * quantidade);
    const b = Math.floor(aleatorio() * quantidade);
    if (Math.abs(a - b) > 1) linhas.push([a, b]);
  }

  const matiz = Math.round(215 + aleatorio() * 60);
  return { estrelas, linhas, matiz };
}
