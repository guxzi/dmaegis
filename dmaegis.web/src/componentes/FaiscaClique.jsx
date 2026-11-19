import { useEffect } from 'react';
import { alvoDaFaisca, raiosDaFaisca } from '../visual/faisca.js';

const RAIOS = raiosDaFaisca(6, 22);
const DURACAO_MS = 650;

// Um brilho de estrela onde o mestre clica num botão ou link. Não desenha nada se o sistema pedir menos movimento.
export default function FaiscaClique() {
  useEffect(() => {
    const menosMovimento = window.matchMedia?.('(prefers-reduced-motion: reduce)');

    function aoClicar(evento) {
      if (evento.button !== 0 || menosMovimento?.matches || !alvoDaFaisca(evento.target)) return;
      const faisca = document.createElement('span');
      faisca.className = 'faisca';
      faisca.setAttribute('aria-hidden', 'true');
      faisca.style.left = `${evento.clientX}px`;
      faisca.style.top = `${evento.clientY}px`;
      const anel = document.createElement('span');
      anel.className = 'faisca-anel';
      faisca.append(anel);
      for (const { x, y } of RAIOS) {
        const raio = document.createElement('span');
        raio.className = 'faisca-raio';
        raio.style.setProperty('--x', `${x}px`);
        raio.style.setProperty('--y', `${y}px`);
        faisca.append(raio);
      }
      document.body.append(faisca);
      setTimeout(() => faisca.remove(), DURACAO_MS);
    }

    document.addEventListener('pointerdown', aoClicar);
    return () => document.removeEventListener('pointerdown', aoClicar);
  }, []);

  return null;
}
