import { useEffect, useRef } from 'react';

const CORES = ['196,181,253', '169,187,255', '255,255,255', '139,92,246'];

// Poeira de estrelas subindo devagar num <canvas> (adaptado das brasas do Dungeons and Newbies).
// Não anima se o sistema pede menos movimento, e pausa quando sai da tela.
export default function PoeiraEstelar({ quantidade = 55 }) {
  const tela = useRef(null);

  useEffect(() => {
    const canvas = tela.current;
    if (!canvas || window.matchMedia('(prefers-reduced-motion: reduce)').matches) return undefined;
    const ctx = canvas.getContext('2d');
    let largura = 0;
    let altura = 0;
    let rodando = true;
    let quadro = 0;

    const redimensionar = () => {
      const dpr = Math.min(window.devicePixelRatio || 1, 2);
      largura = canvas.clientWidth;
      altura = canvas.clientHeight;
      canvas.width = largura * dpr;
      canvas.height = altura * dpr;
      ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    };

    const nova = (inicial) => ({
      x: Math.random() * largura,
      y: inicial ? Math.random() * altura : altura + 10,
      r: Math.random() * 1.6 + 0.4,
      vy: Math.random() * 0.35 + 0.08,
      vx: (Math.random() - 0.5) * 0.15,
      fase: Math.random() * Math.PI * 2,
      cor: CORES[Math.floor(Math.random() * CORES.length)],
    });

    redimensionar();
    const particulas = Array.from({ length: quantidade }, () => nova(true));

    const desenhar = (t) => {
      if (!rodando) return;
      ctx.clearRect(0, 0, largura, altura);
      ctx.globalCompositeOperation = 'lighter';
      for (const p of particulas) {
        p.y -= p.vy;
        p.x += p.vx + Math.sin(t / 1400 + p.fase) * 0.12;
        const desbotar = Math.min(1, p.y / (altura * 0.85));
        const cintilar = 0.55 + Math.sin(t / 420 + p.fase * 3) * 0.45;
        const alfa = Math.max(0, desbotar) * cintilar;
        const g = ctx.createRadialGradient(p.x, p.y, 0, p.x, p.y, p.r * 4);
        g.addColorStop(0, `rgba(${p.cor},${alfa})`);
        g.addColorStop(1, `rgba(${p.cor},0)`);
        ctx.fillStyle = g;
        ctx.beginPath();
        ctx.arc(p.x, p.y, p.r * 4, 0, Math.PI * 2);
        ctx.fill();
        if (p.y < -10) Object.assign(p, nova(false));
      }
      quadro = requestAnimationFrame(desenhar);
    };

    const visibilidade = new IntersectionObserver(([entrada]) => {
      const estava = rodando;
      rodando = entrada.isIntersecting;
      if (rodando && !estava) quadro = requestAnimationFrame(desenhar);
    });
    visibilidade.observe(canvas);
    window.addEventListener('resize', redimensionar);
    quadro = requestAnimationFrame(desenhar);

    return () => {
      rodando = false;
      cancelAnimationFrame(quadro);
      visibilidade.disconnect();
      window.removeEventListener('resize', redimensionar);
    };
  }, [quantidade]);

  return <canvas ref={tela} className="poeira-estelar" aria-hidden="true" />;
}
