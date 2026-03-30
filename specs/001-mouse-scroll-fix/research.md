# Pesquisa e decisões técnicas — 001-mouse-scroll-fix

Este documento resolve pontos deixados como “NEEDS CLARIFICATION” no contexto técnico e consolida decisões para o MVP.

---

## 1. Stack: C# / .NET 8 (Windows) vs Delphi (orientação da constituição)

**Decisão**: Implementar o MVP em **C# 12** com **.NET 8** (`net8.0-windows`), **WinForms**, conforme **ADR-001** e ADRs subsequentes em `docs/adr.md`.

**Racional**: O repositório já regista decisão arquitetural para C#, WinForms e P/Invoke para hooks; mantém uma única linha de implementação e documentação alinhada.

**Alternativas consideradas**: **Delphi** (alinhamento literal com a constituição) — exigiria novo ADR ou revogação do ADR-001 e retrabalho de documentação; **C++/WinRT** — mais complexidade para o mesmo valor no MVP.

---

## 2. Captura de scroll vertical: hook de baixo nível e touchpad

**Decisão**: Usar **`SetWindowsHookEx` com `WH_MOUSE_LL`** como caminho principal para eventos de roda e, tipicamente, gestos de scroll que se traduzem em mensagens de roda sintéticas no sistema. Manter **Raw Input** ou APIs adicionais como **extensão documentada** se, em validação, algum hardware não gerar o fluxo esperado pelo hook — com limites explicitados na matriz de testes.

**Racional**: ADR-003; uma única rota de interceptação simplifica a “mesma regra” entre roda e touchpad quando o SO agrega o gesto a `WM_MOUSEWHEEL` / `WM_MOUSEHWHEEL` (horizontal fora do MVP — ignorar).

**Alternativas consideradas**: **Apenas Raw Input** — maior complexidade e diferente por dispositivo; **driver filtro** — fora de escopo e impacto inaceitável.

---

## 3. Identificação de janela e “cenário” para a lista de inclusão

**Decisão**: No MVP, identificar o alvo por **HWND** sob o cursor com **`WindowFromPoint`** + **`GetAncestor(..., GA_ROOT)`** (ADR-004), e mapear inclusão principalmente por **caminho completo do executável** do processo dono da janela (ADR-006), com possibilidade de critérios adicionais documentados em `data-model.md` (por exemplo título de janela apenas se necessário e com limitações).

**Racional**: Caminho do executável é estável para whitelist; alinha com RF-001 e com o modelo mental “aplicativo na lista”.

**Alternativas consideradas**: **Só nome do processo** — colisões entre aplicações; **só HWND** — instável entre execuções.

---

## 4. Normalização do scroll (passo e direção)

**Decisão**: Definir no código constantes **documentadas** (linhas ou delta por “clique” / unidade de `WHEEL_DELTA`, inversão opcional se produto o exigir) e aplicá-las **apenas** quando a janela atual estiver coberta pela lista e o fix estiver **ligado**. Valores exatos e matriz de testes ficam no `quickstart.md` e na checklist de release — não na especificação de utilizador.

**Racional**: Cumpre “previsível” e verificável (CS-001) sem prescrever números na `spec.md`.

**Alternativas consideradas**: **Só repassar evento original** — não normaliza; **acesso direto a definições do Windows por app** — APIs e suporte heterogéneos no MVP.

---

## 5. Persistência e estado corrompido

**Decisão**: Ficheiro **JSON** em diretório do utilizador; em leitura inválida: **fix desligado**, lista vazia ou última boa cópia (se existir política de backup mínima), **mensagem clara** em pt-BR (casos extremos da especificação).

**Racional**: ADR-007; RF-002 e RF-010 (dados locais).

**Alternativas consideradas**: **Registo do Windows** — mais fricção e antivírus; **SQL local** — desproporcional.

---

## 6. Deteção de conflito com outro software

**Decisão**: Combinar **heurísticas leves**: (1) lista de processos / módulos conhecidos de ferramentas de remap/scroll (configurável ou documentada); (2) opcionalmente detetar **cadeia de hooks** ou comportamento anómalo (ex.: duplicação de eventos) nos testes — **notificação** ao utilizador com texto explicativo; **nunca** desativar automaticamente o outro software (RF-007).

**Racional**: Cumpre RF-007 sem engenharia reversa agressiva.

**Alternativas consideradas**: **Silêncio** — viola RF-007; **bloquear instalação** — inaceitável para o MVP.

---

## 7. Instalador e primeira preferência

**Decisão**: Instalador (Inno Setup, WiX ou equivalente) com **checkbox** “Ativar o fix ao concluir” (ou similar em pt-BR); valor escrito como **preferência inicial** no JSON na primeira execução (RF-002).

**Racional**: Clarificação da especificação; fluxo portátil sem instalador documentado em `quickstart.md`.

**Alternativas consideradas**: **Sempre desligado por defeito** — conflita com opção de instalador quando este existir.

---

## 8. Verificação de atualizações (opcional, sem telemetria)

**Decisão**: Se existir no futuro, **GET** opcional a endpoint estático (versão) **sem** identificadores de utilizador e **sem** correlacionar com uso — detalhado no release; **MVP pode omitir** completamente para reduzir superfície (RF-010).

**Racional**: Evita ambiguidade com telemetria.

**Alternativas consideradas**: **Atualizações automáticas silenciosas** — fora do âmbito do MVP se não for explicitamente pedido.

---

## 9. Windows 11 apenas

**Decisão**: **Checagem em arranque** (versão do SO) e recusa amigável em **Windows 10** ou anterior, com mensagem em pt-BR (RF-008).

**Racional**: Clarificação da especificação.

**Alternativas consideradas**: **Suportar Win10** — explicitamente fora do MVP.
