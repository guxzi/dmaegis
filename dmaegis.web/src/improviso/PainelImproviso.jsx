import { useState } from 'react';
import { criarRegistro } from '../api/caderno.js';
import { mensagemDeErro } from '../api/erros.js';
import { CHANCES, consultarOraculo, gerarNpc, registroDoNpc } from './improviso.js';

// Aba "Improviso" do escudo: oráculo de sim ou não e NPC rápido, que vai para o caderno com um clique.
export default function PainelImproviso({ idCampanha, aoSalvarRegistro }) {
  const [chance, setChance] = useState('meio');
  const [resposta, setResposta] = useState(null);
  const [pergunta, setPergunta] = useState('');
  const [npc, setNpc] = useState(null);
  const [salvo, setSalvo] = useState(null);
  const [erro, setErro] = useState('');

  function perguntar(evento) {
    evento.preventDefault();
    setResposta({ ...consultarOraculo(chance), pergunta: pergunta.trim(), chave: Date.now() });
  }

  function novoNpc() {
    setNpc({ ...gerarNpc(), chave: Date.now() });
    setSalvo(null);
    setErro('');
  }

  async function salvarNpc() {
    setErro('');
    try {
      const registro = await criarRegistro(idCampanha, registroDoNpc(npc));
      setSalvo(registro);
      aoSalvarRegistro?.(registro);
    } catch (e) {
      setErro(mensagemDeErro(e));
    }
  }

  return (
    <div className="painel-improviso">
      <section className="mb-3">
        <p className="rotulo-painel">Oráculo</p>
        <form onSubmit={perguntar}>
          <input className="form-control form-control-sm mb-2" placeholder="Pergunta (opcional): o guarda aceita o suborno?"
            value={pergunta} onChange={(e) => setPergunta(e.target.value)} maxLength={160} aria-label="Pergunta ao oráculo" />
          <div className="d-flex flex-wrap gap-2 align-items-center">
            <div className="segmentado" role="group" aria-label="Chance">
              {Object.entries(CHANCES).map(([id, { rotulo }]) => (
                <button key={id} type="button" aria-pressed={chance === id} onClick={() => setChance(id)}>{rotulo}</button>
              ))}
            </div>
            <button type="submit" className="btn btn-sm btn-primary ms-auto">Perguntar</button>
          </div>
        </form>
        {resposta && (
          <div className="resposta-oraculo" key={resposta.chave} role="status">
            {resposta.pergunta && <span className="small text-secondary d-block">{resposta.pergunta}</span>}
            <strong>{resposta.resposta}</strong>
            <span className="small">{resposta.dica}</span>
            <span className="small text-secondary">d6: {resposta.dado}{resposta.valor !== resposta.dado && ` → ${resposta.valor}`}</span>
          </div>
        )}
      </section>

      <div className="divisor-estelar my-3" />

      <section>
        <div className="d-flex align-items-center mb-2">
          <p className="rotulo-painel mb-0 flex-grow-1">NPC rápido</p>
          <button type="button" className="btn btn-sm btn-fantasma" onClick={novoNpc}>{npc ? 'Outro' : 'Gerar NPC'}</button>
        </div>
        {npc && (
          <article className="cartao-npc-rapido" key={npc.chave}>
            <h3 className="h6 mb-1">{npc.nome}</h3>
            <p className="small mb-1"><span className="text-secondary">Ofício:</span> {npc.oficio}</p>
            <p className="small mb-1"><span className="text-secondary">Jeito:</span> {npc.traco}</p>
            <p className="small mb-1"><span className="text-secondary">Aparência:</span> {npc.aparencia}</p>
            <p className="small mb-1"><span className="text-secondary">Quer:</span> {npc.desejo}</p>
            <p className="small mb-2"><span className="text-secondary">Mania:</span> {npc.mania}</p>
            {salvo
              ? <p className="small mb-0 text-success">Salvo no caderno.</p>
              : <button type="button" className="btn btn-sm btn-primary" onClick={salvarNpc}>Salvar no caderno</button>}
            {erro && <p className="small text-danger mt-2 mb-0">{erro}</p>}
          </article>
        )}
      </section>
    </div>
  );
}
