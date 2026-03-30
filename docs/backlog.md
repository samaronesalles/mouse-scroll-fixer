# Backlog do Produto — MouseScrollFixer

Este backlog foi atualizado para o estado real do código atual.

## Entregue no MVP atual

- Base da solução .NET 8 (`src` + `tests`) com build centralizado em `publish\`.
- Hook global `WH_MOUSE_LL` para interceptação de scroll.
- Resolução de janela/processo por ponto sob o cursor.
- Filtro por aplicação via `inclusionList` (whitelist por caminho do executável).
- UI WinForms na bandeja com:
  - ativar/desativar fix;
  - adicionar/remover/editar entradas da lista;
  - ajuste de comportamento (`invertVertical`, `linesPerNotchApprox`, `touchpadSameAsWheel`, `useVScrollFallback`).
- Persistência local em `%LocalAppData%\MouseScrollFixer\app-config.json` com backup.
- Instância única com ativação da UI existente na segunda execução.
- Aviso de arranque na bandeja e notificação de possível conflito com software de terceiros.
- Testes automatizados de validação de configuração (`AppConfigValidatorTests`).

## Backlog de evolução (não implementado ainda)

## 1) Scroll horizontal (fora do MVP)
- **Objetivo**: avaliar suporte opcional a `WM_MOUSEHWHEEL`.
- **Risco**: maior chance de interferência com apps modernos.

## 2) Auto start com Windows
- **Objetivo**: opção explícita de iniciar com a sessão do usuário.
- **Estado**: hoje não há controle de auto start na UI nem persistência dedicada.

## 3) Suporte a apps elevadas
- **Objetivo**: definir estratégia clara para cenários com privilégios diferentes.
- **Estado**: app roda `asInvoker`; não há modo configurável “rodar como admin”.

## 4) Melhorias de testes
- **Objetivo**: ampliar cobertura além de validação de config (ex.: normalização e comportamento de sessão).

## 5) Instalador oficial
- **Objetivo**: empacotar distribuição com fluxo guiado e opção de primeira preferência de ativação.
- **Estado**: documentação existe em `installer/README.md`, sem pipeline obrigatório no repo.
