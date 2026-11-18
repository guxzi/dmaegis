import { useCallback, useEffect, useRef, useState } from 'react';
import { mensagemDeErro } from '../api/erros.js';
import { enviarImagem, excluirImagem, listarImagens, urlDaImagem } from '../api/imagens.js';
import ConfirmarExclusao from '../componentes/ConfirmarExclusao.jsx';
import { formatarTamanho, tituloDoArquivo, validarArquivo } from './arquivos.js';
import VisualizadorImagem from './VisualizadorImagem.jsx';

// Mapas e imagens de um registro. Com `editavel`, permite enviar (vários de uma vez) e excluir.
// Sem `editavel` e sem imagens, não mostra nada (cartão do escudo).
export default function GaleriaImagens({ idRegistro, editavel = false, colunas = 3, aoMudar }) {
  const [imagens, setImagens] = useState(null);

  // Avisa a página (ex.: o detalhe do registro mostra o primeiro mapa em destaque).
  useEffect(() => {
    if (imagens) aoMudar?.(imagens);
  }, [imagens, aoMudar]);
  const [aberta, setAberta] = useState(null);
  const [paraExcluir, setParaExcluir] = useState(null);
  const [progresso, setProgresso] = useState('');
  const [erros, setErros] = useState([]);
  const seletor = useRef(null);

  useEffect(() => {
    let ativo = true;
    listarImagens(idRegistro)
      .then((lista) => ativo && setImagens(lista))
      .catch((e) => ativo && setErros([mensagemDeErro(e)]));
    return () => {
      ativo = false;
    };
  }, [idRegistro]);

  async function enviar(arquivos) {
    const lista = [...arquivos];
    const novosErros = [];
    for (let i = 0; i < lista.length; i++) {
      const arquivo = lista[i];
      setProgresso(`Enviando ${i + 1} de ${lista.length}…`);
      const aviso = validarArquivo(arquivo);
      if (aviso) {
        novosErros.push(`${arquivo.name}: ${aviso}`);
        continue;
      }
      try {
        const nova = await enviarImagem(idRegistro, arquivo, tituloDoArquivo(arquivo.name));
        setImagens((atuais) => [...(atuais ?? []), nova]);
      } catch (e) {
        novosErros.push(`${arquivo.name}: ${mensagemDeErro(e)}`);
      }
    }
    setProgresso('');
    setErros(novosErros);
    if (seletor.current) seletor.current.value = '';
  }

  async function excluir() {
    try {
      await excluirImagem(paraExcluir.id);
      setImagens((atuais) => atuais.filter((i) => i.id !== paraExcluir.id));
    } catch (e) {
      setErros([mensagemDeErro(e)]);
    }
    setParaExcluir(null);
  }

  const fecharExclusao = useCallback(() => setParaExcluir(null), []);
  const fecharVisualizador = useCallback(() => setAberta(null), []);

  if (!editavel && (!imagens || imagens.length === 0)) return null;

  return (
    <section className="mb-3">
      <div className="d-flex align-items-center gap-2 mb-2">
        <h3 className="h6 text-uppercase mb-0" style={{ letterSpacing: '0.12em', color: 'var(--dm-estrela)' }}>
          Mapas e imagens
        </h3>
        {editavel && (
          <>
            <button type="button" className="btn btn-sm btn-outline-primary ms-auto" disabled={Boolean(progresso)}
              onClick={() => seletor.current?.click()}>
              {progresso || 'Adicionar mapas'}
            </button>
            <input ref={seletor} type="file" accept="image/jpeg,image/png,image/webp,image/gif" multiple hidden
              onChange={(e) => e.target.files.length && enviar(e.target.files)} />
          </>
        )}
      </div>

      {erros.length > 0 && (
        <div className="alert alert-danger py-2 small">
          {erros.map((erro) => <div key={erro}>{erro}</div>)}
        </div>
      )}

      {editavel && imagens?.length === 0 && (
        <p className="text-secondary small mb-0">Nenhum mapa ainda. Envie imagens JPEG, PNG, WebP ou GIF de até 15 MB.</p>
      )}

      <div className="galeria-imagens" style={{ gridTemplateColumns: `repeat(${colunas}, minmax(0, 1fr))` }}>
        {imagens?.map((imagem, indice) => (
          <figure key={imagem.id} className="miniatura-imagem mb-0">
            <button type="button" className="miniatura-botao" onClick={() => setAberta(indice)}
              aria-label={`Abrir ${imagem.titulo}`}>
              <img src={urlDaImagem(imagem.id)} alt="" loading="lazy" decoding="async" />
            </button>
            <figcaption className="d-flex align-items-center gap-1">
              <span className="text-truncate flex-grow-1" title={imagem.titulo}>{imagem.titulo}</span>
              {editavel && (
                <>
                  <span className="text-secondary">{formatarTamanho(imagem.tamanho)}</span>
                  <button type="button" className="btn-close flex-shrink-0" style={{ fontSize: '0.55rem' }}
                    aria-label={`Excluir ${imagem.titulo}`} onClick={() => setParaExcluir(imagem)} />
                </>
              )}
            </figcaption>
          </figure>
        ))}
      </div>

      {aberta !== null && imagens?.[aberta] && (
        <VisualizadorImagem imagens={imagens} indice={aberta} aoMudar={setAberta} aoFechar={fecharVisualizador} />
      )}

      <ConfirmarExclusao
        aberto={paraExcluir !== null}
        titulo="Excluir imagem"
        texto={paraExcluir ? `Excluir "${paraExcluir.titulo}"? Isso não pode ser desfeito.` : ''}
        aoConfirmar={excluir}
        aoCancelar={fecharExclusao}
      />
    </section>
  );
}
