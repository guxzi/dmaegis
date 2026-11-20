// Clima por cima do mapa, como uma janela: chuva, neve, neblina ou brasas. Só CSS (camadas com
// gradientes que deslizam), sem custo de desenho por partícula. Com "menos movimento", fica parado.
export default function CamadaClima({ clima }) {
  if (!clima) return null;
  return (
    <div className={`camada-clima clima-${clima}`} aria-hidden="true">
      <span className="clima-camada clima-perto" />
      <span className="clima-camada clima-longe" />
    </div>
  );
}
