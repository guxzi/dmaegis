import { useEffect, useState } from 'react';
import { urlDaImagem } from '../api/imagens.js';
import { mostrarNaProjecao } from '../projecao/canal.js';
import { nomeArquivoDownload } from './arquivos.js';

// A área de transferência só aceita PNG com segurança: a imagem passa por um canvas antes de ser copiada.
async function imagemEmPng(id) {
  const resposta = await fetch(urlDaImagem(id), { credentials: 'same-origin' });
  if (!resposta.ok) throw new Error('falha ao baixar');
  const original = await resposta.blob();
  if (original.type === 'image/png') return original;
  const bitmap = await createImageBitmap(original);
  const canvas = document.createElement('canvas');
  canvas.width = bitmap.width;
  canvas.height = bitmap.height;
  canvas.getContext('2d').drawImage(bitmap, 0, 0);
  return new Promise((resolver, rejeitar) =>
    canvas.toBlob((png) => (png ? resolver(png) : rejeitar(new Error('falha ao converter'))), 'image/png'));
}

// Mostrar aos jogadores, Copiar e Baixar. `compacto` usa só ícones (cartões e miniaturas).
export default function AcoesImagem({ imagem, comOpcaoTitulo = false, compacto = false }) {
  const [comTitulo, setComTitulo] = useState(false);
  const [aviso, setAviso] = useState('');

  useEffect(() => {
    if (!aviso) return undefined;
    const temporizador = setTimeout(() => setAviso(''), 2500);
    return () => clearTimeout(temporizador);
  }, [aviso]);

  async function copiar() {
    if (!navigator.clipboard?.write || typeof ClipboardItem === 'undefined') {
      setAviso('Este navegador não deixa copiar imagens. Use o Baixar.');
      return;
    }
    try {
      // A promessa vai direto para o ClipboardItem: o Safari exige isso dentro do clique.
      await navigator.clipboard.write([new ClipboardItem({ 'image/png': imagemEmPng(imagem.id) })]);
      setAviso('Copiada! Cole no chat dos jogadores.');
    } catch {
      setAviso('Não deu para copiar. Use o Baixar.');
    }
  }

  function mostrar() {
    mostrarNaProjecao(imagem, comTitulo);
    setAviso('Na tela dos jogadores.');
  }

  const classe = `btn btn-sm ${compacto ? 'btn-fantasma px-2' : 'btn-fantasma'}`;

  return (
    <div className="acoes-imagem">
      <button type="button" className={`btn btn-sm btn-primary${compacto ? ' px-2' : ''}`} onClick={mostrar}
        title="Mostrar na janela de projeção" aria-label={`Mostrar ${imagem.titulo} aos jogadores`}>
        {compacto ? '📽' : '📽 Mostrar aos jogadores'}
      </button>
      <button type="button" className={classe} onClick={copiar} title="Copiar a imagem"
        aria-label={`Copiar ${imagem.titulo}`}>
        {compacto ? '⧉' : '⧉ Copiar'}
      </button>
      <a className={classe} href={urlDaImagem(imagem.id)} download={nomeArquivoDownload(imagem.titulo, imagem.tipoConteudo)}
        title="Baixar a imagem" aria-label={`Baixar ${imagem.titulo}`}>
        {compacto ? '⤓' : '⤓ Baixar'}
      </a>
      {comOpcaoTitulo && (
        <div className="form-check form-check-inline small mb-0 ms-1">
          <input className="form-check-input" type="checkbox" id={`comTitulo${imagem.id}`}
            checked={comTitulo} onChange={(e) => setComTitulo(e.target.checked)} />
          <label className="form-check-label" htmlFor={`comTitulo${imagem.id}`}>com o título</label>
        </div>
      )}
      <span className="acoes-imagem-aviso small" role="status">{aviso}</span>
    </div>
  );
}
