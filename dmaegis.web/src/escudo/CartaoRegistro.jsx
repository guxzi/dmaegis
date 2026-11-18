import { useEffect, useState } from 'react';
import { Link } from 'react-router';
import { obterRegistro } from '../api/caderno.js';
import { mensagemDeErro } from '../api/erros.js';
import { Carregando } from '../auth/RotaProtegida.jsx';
import SeloTipo from '../caderno/SeloTipo.jsx';
import GaleriaImagens from '../imagens/GaleriaImagens.jsx';

// Cartão lateral com o registro inteiro, para consultar sem sair do escudo. Esc ou × fecham.
export default function CartaoRegistro({ idCampanha, idRegistro, rotulo, aoFechar }) {
  const [registro, setRegistro] = useState(null);
  const [erro, setErro] = useState('');

  useEffect(() => {
    // Trocar de registro recria o componente (key={idRegistro} no escudo), então o estado já começa vazio.
    let ativo = true;
    obterRegistro(idRegistro)
      .then((r) => ativo && setRegistro(r))
      .catch((e) => ativo && setErro(mensagemDeErro(e)));
    return () => {
      ativo = false;
    };
  }, [idRegistro]);

  useEffect(() => {
    const aoTeclar = (e) => e.key === 'Escape' && aoFechar();
    window.addEventListener('keydown', aoTeclar);
    return () => window.removeEventListener('keydown', aoTeclar);
  }, [aoFechar]);

  return (
    <aside className="cartao-lateral p-4" role="dialog" aria-label={registro?.nome ?? 'Registro'}>
      <div className="d-flex justify-content-between align-items-start gap-2 mb-2">
        <h2 className="h4 mb-0">{registro?.nome ?? '…'}</h2>
        <button type="button" className="btn-close" aria-label="Fechar" onClick={aoFechar} />
      </div>

      {erro && <div className="alert alert-danger">{erro}</div>}
      {!registro && !erro && <Carregando />}

      {registro && (
        <>
          <div className="d-flex flex-wrap gap-2 mb-3">
            <SeloTipo idTipo={registro.idTipoRegistro} rotulo={rotulo(registro.idTipoRegistro)} />
            {registro.tags.map((t) => <span key={t.id} className="badge rounded-pill text-bg-light border">{t.nome}</span>)}
          </div>
          {registro.resumo && <p className="lead fs-6">{registro.resumo}</p>}

          {registro.atributos.length > 0 && (
            <dl className="row g-0 border rounded-3 p-2 mb-3 bg-body-tertiary">
              {registro.atributos.map((a) => (
                <div className="col-6 p-1" key={a.chave}>
                  <dt className="small text-secondary fw-normal">{a.chave}</dt>
                  <dd className="mb-0 fw-semibold">{a.valor}</dd>
                </div>
              ))}
            </dl>
          )}

          {registro.descricao
            ? <p style={{ whiteSpace: 'pre-line' }}>{registro.descricao}</p>
            : <p className="text-secondary fst-italic small">Sem descrição.</p>}

          <GaleriaImagens idRegistro={registro.id} colunas={2} />

          <Link to={`/campanhas/${idCampanha}/caderno/${registro.id}/editar`} className="small">Editar no caderno</Link>
        </>
      )}
    </aside>
  );
}
