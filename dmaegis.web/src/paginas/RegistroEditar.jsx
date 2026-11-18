import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import { atualizarRegistro, criarRegistro, listarTags, obterRegistro } from '../api/caderno.js';
import { mensagemDeErro } from '../api/erros.js';
import { Carregando } from '../auth/RotaProtegida.jsx';
import FormularioRegistro from '../caderno/FormularioRegistro.jsx';
import { useCaderno } from '../caderno/useCaderno.js';
import Cabecalho from '../componentes/Cabecalho.jsx';

// Cria (sem idRegistro na URL) ou edita um registro.
export default function RegistroEditar() {
  const { id, idRegistro } = useParams();
  const editando = Boolean(idRegistro);
  const navegar = useNavigate();
  const { campanha, tipos, erro: erroCaderno } = useCaderno(id);
  const [tags, setTags] = useState([]);
  const [registro, setRegistro] = useState(editando ? null : undefined);
  const [erro, setErro] = useState('');

  useEffect(() => {
    listarTags(id).then(setTags).catch(() => {});
  }, [id]);

  useEffect(() => {
    if (!editando) return;
    obterRegistro(idRegistro).then(setRegistro).catch((e) => setErro(mensagemDeErro(e)));
  }, [editando, idRegistro]);

  async function salvar(dados) {
    const salvo = editando ? await atualizarRegistro(idRegistro, dados) : await criarRegistro(id, dados);
    navegar(`/campanhas/${id}/caderno/${salvo.id}`, { replace: true });
  }

  const voltar = editando ? `/campanhas/${id}/caderno/${idRegistro}` : `/campanhas/${id}/caderno`;
  const pronto = campanha && tipos.length > 0 && registro !== null;

  return (
    <>
      <Cabecalho idCampanha={id} />
      <main className="container pb-5" style={{ maxWidth: 860 }}>
        <Link to={voltar} className="d-inline-block mb-3 text-decoration-none">← Voltar</Link>
        <h1 className="h3 mb-3">{editando ? 'Editar registro' : 'Novo registro'}</h1>
        {(erro || erroCaderno) && <div className="alert alert-danger" role="alert">{erro || erroCaderno}</div>}
        {!pronto && !erro && !erroCaderno && <Carregando />}
        {pronto && (
          <FormularioRegistro
            campanha={campanha}
            tipos={tipos}
            tagsExistentes={tags}
            inicial={registro}
            textoBotao={editando ? 'Salvar' : 'Criar registro'}
            aoSalvar={salvar}
            aoCancelar={() => navegar(voltar)}
          />
        )}
      </main>
    </>
  );
}
