# Instalador opcional (MouseScrollFixer)

Este repositório pode incluir **no futuro** um instalador (por exemplo **Inno Setup**, WiX ou MSIX) com textos em **pt-BR** e uma tarefa **«Ativar o fix ao concluir»** que define a **primeira** preferência de ativação gravada no JSON local (RF-002), alinhado ao `specs/001-mouse-scroll-fix/` e ao `quickstart.md`.

## Estado neste incremento

- Não há script de build de instalador obrigatório na raiz; o fluxo **portátil** e a definição da preferência na **interface** (ou ficheiro JSON sob `%LocalAppData%\MouseScrollFixer\`) cobrem o MVP.
- Para um **instalador** com checkbox de ativação, o fluxo típico seria:
  1. Copiar o executável e dependências para `{app}`.
  2. Opcional (tarefa marcada): na primeira execução pós-instalação, **gravar** `activation.enabled` conforme a escolha (sem sobrescrever preferências já existentes válidas, salvo política de produto explícita).
  3. Opcional: atalho na bandeja e entrada em «Aplicações» / menu Iniciar.

## Referência

- `specs/001-mouse-scroll-fix/quickstart.md` — secção «Instalador opcional e primeira preferência».
- Contrato JSON: `specs/001-mouse-scroll-fix/contracts/app-config.schema.json`.
