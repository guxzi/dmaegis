// Ícones de traço simples (desenho próprio), na cor do texto do botão.
function Icone({ children, tamanho = 16 }) {
  return (
    <svg width={tamanho} height={tamanho} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"
      strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" focusable="false">
      {children}
    </svg>
  );
}

// Uma tela com um raio de luz: mostrar na projeção.
export const IconeProjetar = (props) => (
  <Icone {...props}>
    <rect x="3" y="4" width="18" height="12" rx="2" />
    <path d="M8 20h8M12 16v4" />
    <path d="M10 8.5l4 1.5-4 1.5z" fill="currentColor" />
  </Icone>
);

export const IconeCopiar = (props) => (
  <Icone {...props}>
    <rect x="8" y="8" width="12" height="12" rx="2" />
    <path d="M16 8V6a2 2 0 0 0-2-2H6a2 2 0 0 0-2 2v8a2 2 0 0 0 2 2h2" />
  </Icone>
);

export const IconeBaixar = (props) => (
  <Icone {...props}>
    <path d="M12 4v11M7 10l5 5 5-5" />
    <path d="M5 20h14" />
  </Icone>
);
