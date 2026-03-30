# ADR — Architectural Decision Records (estado atual)

Este documento reflete as decisões **implementadas hoje** no código.

## ADR-001 — Linguagem e plataforma

**Decisão:** C# 12 com .NET 8 (`net8.0-windows`), aplicação WinForms.

**Motivo:**
- Integração direta com APIs Win32 (P/Invoke).
- Entrega simples de app residente em bandeja no Windows.

## ADR-002 — Captura global de scroll

**Decisão:** `SetWindowsHookEx` com `WH_MOUSE_LL` para interceptar eventos de mouse.

**Motivo:**
- Permite analisar `WM_MOUSEWHEEL` globalmente antes de o sistema entregar ao alvo.
- Possibilita consumir o evento original quando o fix é aplicado (evita duplicação).

## ADR-003 — Escopo funcional de entrada

**Decisão:** MVP aplica correção **somente ao scroll vertical** (`WM_MOUSEWHEEL`).

**Motivo:**
- Escopo mais previsível para aplicações legadas.
- `WM_MOUSEHWHEEL` (horizontal) permanece fora do MVP.

## ADR-004 — Resolução de alvo sob o cursor

**Decisão:** Resolver alvo por ponto (`WindowFromPoint`) e processo dono da janela (`GetAncestor` + PID + caminho do executável).

**Motivo:**
- Permite aplicar regra por aplicativo real sob o cursor, não apenas por janela em foco global.
- Dá base para lista de inclusão por caminho completo de executável.

## ADR-005 — Entrega do scroll ao aplicativo legado

**Decisão:** Reenvio por `PostMessageW`:
- `WM_MOUSEWHEEL` para o `HWND` efetivo (com preferência por `hwndFocus` do mesmo processo);
- opcionalmente `WM_VSCROLL` (`SB_LINEUP`/`SB_LINEDOWN`) em modo legado.

**Motivo:**
- Compatibilidade com apps antigos que não tratam `WM_MOUSEWHEEL`.
- Evita bloqueio de thread de UI por chamadas síncronas.

## ADR-006 — Filtro por aplicação (whitelist)

**Decisão:** Aplicar fix somente para executáveis presentes em `inclusionList` (`matchKind = ExecutablePath`).

**Motivo:**
- Controle granular por app.
- Reduz interferência em aplicações fora do escopo do usuário.

## ADR-007 — Persistência local e robustez

**Decisão:** Persistência em `%LocalAppData%\MouseScrollFixer\app-config.json` com backup (`app-config.bak.json`) e gravação defensiva.

**Motivo:**
- Configuração local, sem telemetria.
- Recuperação de estado em caso de JSON inválido/corrompido.

## ADR-008 — Instância única e ativação da UI existente

**Decisão:** Mutex nomeado + pipe nomeado para garantir uma instância por sessão de usuário; segunda execução sinaliza a primeira para abrir/restaurar configurações.

**Motivo:**
- Evita múltiplos processos concorrentes.
- Melhora UX quando o app já está apenas na bandeja.

## ADR-009 — Distribuição do executável

**Decisão:** Publicação `win-x64`, single-file, framework-dependent (`SelfContained=false`).

**Motivo:**
- Distribuição simples com um `.exe` principal.
- Pré-requisito explícito: .NET 8 Desktop Runtime no destino.

## ADR-010 — Compatibilidade de sistema operacional

**Decisão:** Suporte de runtime somente para Windows 11 (checagem no arranque).

**Motivo:**
- Alinhamento ao escopo do MVP e aos critérios atuais de validação.
