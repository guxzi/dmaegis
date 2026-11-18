// Formas dos dados em SVG (viewBox 0 0 100 100), no estilo do rolador do Dungeons and Newbies,
// com as cores do tema cósmico. `elementos` são trechos de SVG estáticos.
const FORMAS = {
  4: { cor: '#f87171', textoY: 68, elementos: ['<polygon points="50,7 95,89 5,89"/>'] },
  6: { cor: '#c4b5fd', textoY: 54, elementos: ['<rect x="11" y="11" width="78" height="78" rx="14"/>'] },
  8: { cor: '#34d399', textoY: 54, elementos: ['<polygon points="50,3 95,50 50,97 5,50"/>', '<path class="faceta" d="M5 50h90"/>'] },
  10: { cor: '#60a5fa', textoY: 44, elementos: ['<polygon points="50,3 93,40 50,97 7,40"/>', '<path class="faceta" d="M7 40l43 14 43-14M50 54v43"/>'] },
  12: { cor: '#a78bfa', textoY: 54, elementos: ['<polygon points="50,4 96,37 78,93 22,93 4,37"/>', '<polygon class="faceta" points="50,24 72,40 64,68 36,68 28,40"/>'] },
  20: {
    cor: '#e6e9ff', textoY: 54,
    elementos: [
      '<polygon points="50,3 93,27 93,73 50,97 7,73 7,27"/>',
      '<path class="faceta" d="M50 22 75 64H25z M50 3v19M93 27 75 64M7 27l18 37M50 97 25 64M50 97l25-33M7 73l18-9M93 73l-18-9"/>',
    ],
  },
  100: { cor: '#f472b6', textoY: 44, elementos: ['<polygon points="50,3 93,40 50,97 7,40"/>', '<path class="faceta" d="M7 40l43 14 43-14M50 54v43"/>'] },
};

const GENERICA = { cor: '#9aa3d4', textoY: 54, elementos: ['<circle cx="50" cy="50" r="44"/>'] };

export function formaDoDado(faces) {
  return { faces, ...(FORMAS[faces] ?? GENERICA) };
}
