import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { Link, useParams } from 'react-router';
import { criarRegistro, listarRegistros } from '../api/caderno.js';
import { mensagemDeErro } from '../api/erros.js';
import { enviarImagem, listarImagensDaCampanha, urlDaImagem } from '../api/imagens.js';
import { Carregando } from '../auth/RotaProtegida.jsx';
import { COR_TIPO } from '../caderno/tipos.js';
import { useCaderno } from '../caderno/useCaderno.js';
import Cabecalho from '../componentes/Cabecalho.jsx';
import AcoesImagem from '../imagens/AcoesImagem.jsx';
import { tituloDoArquivo, validarArquivo } from '../imagens/arquivos.js';
import { acharRegistroPeloNome, agruparPorRegistro } from '../imagens/galeria.js';
import VisualizadorImagem from '../imagens/VisualizadorImagem.jsx';
import { abrirProjecao, limparProjecao } from '../projecao/canal.js';
import CabecalhoSecao from '../visual/CabecalhoSecao.jsx';

const ID_TIPO_NPC = 1;

// "Adicionar imagens": o mestre escreve o nome do NPC ou do local. Se o registro não existe, ele é criado
// com o tipo escolhido, e as imagens ficam ligadas a ele.
function EnvioPorNome({ idCampanha, tipos, rotulo, registros, aoEnviar, aoCriarRegistro }) {
  const [nome, setNome] = useState('');
  const [idTipo, setIdTipo] = useState(ID_TIPO_NPC);
  const [arquivos, setArquivos] = useState([]);
  const [progresso, setProgresso] = useState('');
  const [erros, setErros] = useState([]);
  const seletor = useRef(null);
  const existente = acharRegistroPeloNome(registros, nome);

  async function enviar(evento) {
    evento.preventDefault();
    if (!nome.trim() || arquivos.length === 0) return;
    const novosErros = [];
    try {
      let registro = existente;
      if (!registro) {
        setProgresso('Criando o registro…');
        registro = await criarRegistro(idCampanha, {
          idTipoRegistro: Number(idTipo), nome: nome.trim(), resumo: '', descricao: '', atributos: [], tags: [],
        });
        aoCriarRegistro(registro);
      }
      for (let i = 0; i < arquivos.length; i++) {
        const arquivo = arquivos[i];
        setProgresso(`Enviando ${i + 1} de ${arquivos.length}…`);
        const aviso = validarArquivo(arquivo);
        if (aviso) {
          novosErros.push(`${arquivo.name}: ${aviso}`);
          continue;
        }
        try {
          const nova = await enviarImagem(registro.id, arquivo, tituloDoArquivo(arquivo.name));
          aoEnviar({ ...nova, idRegistro: registro.id, nomeRegistro: registro.nome, idTipoRegistro: registro.idTipoRegistro });
        } catch (e) {
          novosErros.push(`${arquivo.name}: ${mensagemDeErro(e)}`);
        }
      }
      if (novosErros.length === 0) {
        setNome('');
        setArquivos([]);
        if (seletor.current) seletor.current.value = '';
      }
    } catch (e) {
      novosErros.push(mensagemDeErro(e));
    }
    setProgresso('');
    setErros(novosErros);
  }

  return (
    <form className="card cartao-astral mb-4 surgir" onSubmit={enviar}>
      <div className="card-body">
        <div className="row g-2 align-items-end">
          <div className="col-md-5">
            <label htmlFor="nomeGaleria" className="form-label small text-secondary mb-1">NPC ou local</label>
            <input id="nomeGaleria" className="form-control" list="registrosGaleria" maxLength={150} required
              placeholder="Escreva o nome…" value={nome} onChange={(e) => setNome(e.target.value)} />
            <datalist id="registrosGaleria">
              {registros.map((r) => <option key={r.id} value={r.nome} />)}
            </datalist>
          </div>
          <div className="col-md-3">
            <label htmlFor="tipoGaleria" className="form-label small text-secondary mb-1">Tipo</label>
            <select id="tipoGaleria" className="form-select" disabled={Boolean(existente)}
              value={existente ? existente.idTipoRegistro : idTipo} onChange={(e) => setIdTipo(e.target.value)}>
              {tipos.map((t) => <option key={t.id} value={t.id}>{rotulo(t.id)}</option>)}
            </select>
          </div>
          <div className="col-md-4">
            <label htmlFor="arquivosGaleria" className="form-label small text-secondary mb-1">Imagens</label>
            <input id="arquivosGaleria" ref={seletor} type="file" className="form-control" multiple required
              accept="image/jpeg,image/png,image/webp,image/gif" onChange={(e) => setArquivos([...e.target.files])} />
          </div>
        </div>
        <div className="d-flex flex-wrap align-items-center gap-2 mt-3">
          <small className="text-secondary me-auto">
            {nome.trim() && (existente
              ? <>As imagens vão para <strong>{existente.nome}</strong> ({rotulo(existente.idTipoRegistro)}).</>
              : <>“{nome.trim()}” ainda não existe: será criado no caderno como {rotulo(Number(idTipo))}.</>)}
          </small>
          <button type="submit" className="btn btn-primary" disabled={Boolean(progresso) || !nome.trim() || arquivos.length === 0}>
            {progresso || 'Enviar'}
          </button>
        </div>
        {erros.length > 0 && (
          <div className="alert alert-danger py-2 small mt-3 mb-0">
            {erros.map((erro) => <div key={erro}>{erro}</div>)}
          </div>
        )}
      </div>
    </form>
  );
}

// Galeria: todas as imagens da campanha num lugar só, agrupadas pelo NPC, local ou outro registro.
export default function Galeria() {
  const { id } = useParams();
  const { campanha, tipos, rotulo, erro: erroCaderno, naoEncontrada } = useCaderno(id);
  const [imagens, setImagens] = useState(null);
  const [registros, setRegistros] = useState([]);
  const [erro, setErro] = useState('');
  const [idTipo, setIdTipo] = useState('');
  const [termo, setTermo] = useState('');
  const [mostrarEnvio, setMostrarEnvio] = useState(false);
  const [aberta, setAberta] = useState(null);

  useEffect(() => {
    listarImagensDaCampanha(id).then(setImagens).catch((e) => setErro(mensagemDeErro(e)));
    listarRegistros(id).then(setRegistros).catch(() => {});
  }, [id]);

  const grupos = useMemo(() => agruparPorRegistro(imagens ?? [], { idTipo, termo }), [imagens, idTipo, termo]);
  // O visualizador percorre as imagens visíveis na ordem da tela (← → passam de um registro para o outro).
  const visiveis = useMemo(() => grupos.flatMap((g) => g.imagens), [grupos]);
  const tiposComImagem = tipos.filter((t) => imagens?.some((i) => i.idTipoRegistro === t.id));

  const adicionarImagem = useCallback((nova) => setImagens((atuais) => [...(atuais ?? []), nova]), []);
  const adicionarRegistro = useCallback((novo) => setRegistros((atuais) => [...atuais, novo]), []);
  const fecharVisualizador = useCallback(() => setAberta(null), []);

  if (naoEncontrada) {
    return (
      <>
        <Cabecalho />
        <main className="container"><div className="alert alert-warning">Campanha não encontrada. <Link to="/">Voltar</Link></div></main>
      </>
    );
  }

  return (
    <>
      <Cabecalho idCampanha={id} />
      <main className="container pb-5">
        <CabecalhoSecao
          chamada={campanha?.nome ?? 'Campanha'}
          titulo="Galeria"
          subtitulo="Retratos de NPCs, locais e mapas, tudo num lugar só. Mostre aos jogadores com um clique."
          acoes={(
            <>
              <button type="button" className="btn btn-fantasma" onClick={abrirProjecao}>Abrir projeção</button>
              <button type="button" className="btn btn-fantasma" onClick={limparProjecao}>Limpar projeção</button>
              <button type="button" className="btn btn-primary" aria-expanded={mostrarEnvio}
                onClick={() => setMostrarEnvio((v) => !v)}>
                {mostrarEnvio ? 'Fechar envio' : 'Adicionar imagens'}
              </button>
            </>
          )}
        />

        {mostrarEnvio && campanha && (
          <EnvioPorNome idCampanha={id} tipos={tipos} rotulo={rotulo} registros={registros}
            aoEnviar={adicionarImagem} aoCriarRegistro={adicionarRegistro} />
        )}

        {(erro || erroCaderno) && <div className="alert alert-danger" role="alert">{erro || erroCaderno}</div>}

        {imagens?.length > 0 && (
          <div className="d-flex flex-wrap align-items-center gap-3 mb-4">
            <div className="segmentado" role="group" aria-label="Filtrar por tipo">
              <button type="button" aria-pressed={idTipo === ''} onClick={() => setIdTipo('')}>Todas</button>
              {tiposComImagem.map((t) => (
                <button key={t.id} type="button" aria-pressed={idTipo === t.id} onClick={() => setIdTipo(t.id)}>
                  {rotulo(t.id)}
                </button>
              ))}
            </div>
            <input type="search" className="form-control flex-grow-1" style={{ maxWidth: 360 }}
              placeholder="Procurar pelo nome ou título…" aria-label="Procurar na galeria"
              value={termo} onChange={(e) => setTermo(e.target.value)} />
            <span className="small text-secondary ms-auto">{visiveis.length} de {imagens.length} imagens</span>
          </div>
        )}

        {imagens === null && !erro && <Carregando />}

        {imagens?.length === 0 && (
          <div className="aviso-astral">
            Nenhuma imagem ainda. Use <strong>Adicionar imagens</strong> e escreva o nome do NPC ou do local:
            se ele não estiver no caderno, é criado na hora.
          </div>
        )}
        {imagens?.length > 0 && grupos.length === 0 && <div className="aviso-astral">Nenhuma imagem encontrada.</div>}

        {grupos.map((grupo) => (
          <section key={grupo.idRegistro} className="grupo-galeria mb-4"
            style={{ '--cor-tipo': COR_TIPO[grupo.idTipoRegistro] ?? '#9aa3d4' }}>
            <header className="d-flex flex-wrap align-items-baseline gap-2 mb-2">
              <h2 className="h5 mb-0">{grupo.nomeRegistro}</h2>
              <span className="grupo-galeria-tipo">{rotulo(grupo.idTipoRegistro)}</span>
              <Link to={`/campanhas/${id}/caderno/${grupo.idRegistro}`} className="small ms-auto">Abrir no caderno</Link>
            </header>
            <div className="galeria-campanha">
              {grupo.imagens.map((imagem) => (
                <figure key={imagem.id} className="miniatura-imagem mb-0">
                  <button type="button" className="miniatura-botao" aria-label={`Abrir ${imagem.titulo}`}
                    onClick={() => setAberta(visiveis.indexOf(imagem))}>
                    <img src={urlDaImagem(imagem.id)} alt="" loading="lazy" decoding="async" />
                  </button>
                  <figcaption className="text-truncate" title={imagem.titulo}>{imagem.titulo}</figcaption>
                  <AcoesImagem imagem={imagem} compacto />
                </figure>
              ))}
            </div>
          </section>
        ))}
      </main>

      {aberta !== null && visiveis[aberta] && (
        <VisualizadorImagem imagens={visiveis} indice={aberta} aoMudar={setAberta} aoFechar={fecharVisualizador} />
      )}
    </>
  );
}
