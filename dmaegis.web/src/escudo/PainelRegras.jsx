import { useState } from 'react';
import { filtrarRegras } from '../regras/filtrar.js';
import { regrasDoSistema } from '../regras/regras.js';

// Regras de consulta rápida, só do sistema da campanha. A busca abre todas as seções que têm resultado.
export default function PainelRegras({ idSistema }) {
  const [termo, setTermo] = useState('');
  const { secoes, rodape } = regrasDoSistema(idSistema);
  const buscando = termo.trim().length > 0;
  const filtradas = secoes
    .map((secao) => ({ ...secao, itens: filtrarRegras(secao.itens, termo) }))
    .filter((secao) => secao.itens.length > 0);

  return (
    <div className="d-flex flex-column h-100">
      <input type="search" className="form-control form-control-sm mb-2" placeholder="Procurar regra… (ex.: caído, CD, esconder)"
        aria-label="Procurar regra" value={termo} onChange={(e) => setTermo(e.target.value)} />
      <div className="flex-grow-1 overflow-auto" style={{ minHeight: 0 }}>
        {filtradas.length === 0 && <p className="text-secondary small">Nenhuma regra encontrada.</p>}
        {filtradas.map((secao, indice) => (
          // A key muda com a busca para o <details> reabrir quando o termo muda.
          <details key={`${secao.titulo}-${buscando}`} className="secao-regras" open={buscando || indice === 0}>
            <summary>{secao.titulo} <span className="text-secondary small">({secao.itens.length})</span></summary>
            <dl className="mb-0">
              {secao.itens.map((item) => (
                <div key={item.nome} className="item-regra">
                  <dt>
                    {item.nome}
                    {item.termo && <span className="termo-regra">{item.termo}</span>}
                  </dt>
                  <dd>{item.texto}</dd>
                </div>
              ))}
            </dl>
          </details>
        ))}
        {rodape && <p className="rodape-regras">{rodape}</p>}
      </div>
    </div>
  );
}
