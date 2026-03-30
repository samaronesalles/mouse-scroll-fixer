# Modelo de dados — 001-mouse-scroll-fix

## Visão geral

O MVP persiste **preferências locais** e a **lista de inclusão** num único documento JSON (ver contrato em `contracts/app-config.schema.json`). Não há base de dados servidor.

---

## Entidade: `AppConfig` (documento raiz)

| Campo | Tipo | Obrigatório | Regras / notas |
|-------|------|-------------|----------------|
| `schemaVersion` | inteiro | sim | Incrementar quando o formato mudar de forma incompatível. MVP: `1`. |
| `activation` | objeto `ActivationPreference` | sim | RF-002. |
| `inclusionList` | lista de `InclusionEntry` | sim | Pode ser lista vazia; teto máximo de entradas: **64** (ajustável apenas com alteração de especificação/plano). |
| `behavior` | objeto `BehaviorProfile` | não | Se omitido, aplicar perfil por defeito documentado no código. |

---

## Entidade: `ActivationPreference`

Representa se o fix está **ligado ou desligado** e permanece entre reinícios (RF-002).

| Campo | Tipo | Obrigatório | Regras / notas |
|-------|------|-------------|----------------|
| `enabled` | booleano | sim | `true` = fix ativo; `false` = não aplicar normalização. |
| `lastModifiedUtc` | string (ISO 8601) | não | Útil para diagnóstico local; não enviar a terceiros. |

**Primeira execução**: se o instalador gravou escolha, `enabled` DEVE refletir essa escolha na primeira escrita. Sem instalador, o defeito é o definido no `plan.md` / `quickstart.md` (ex.: `false` até o utilizador ativar).

---

## Entidade: `InclusionEntry`

Uma entrada na lista de inclusão (RF-001). Identificação **principal** por caminho do executável.

| Campo | Tipo | Obrigatório | Regras / notas |
|-------|------|-------------|----------------|
| `id` | string (UUID) | sim | Estável para operações de UI (editar/remover). |
| `executablePath` | string | sim | Caminho absoluto normalizado; validar existência no momento da adição quando possível; recusar com mensagem se inválido. |
| `displayName` | string | não | Nome amigável para a lista (ex.: nome do ficheiro ou produto). |
| `matchKind` | string (enum) | sim | MVP: `"ExecutablePath"` apenas; reservado para extensões futuras (`WindowTitle`, etc.). |
| `notes` | string | não | Uso interno / suporte; não telemetria. |

**Validação**: rejeitar duplicados do mesmo `executablePath` com a mesma `matchKind` (mensagem em pt-BR).

---

## Entidade: `BehaviorProfile` (perfil de comportamento)

Agrupa parâmetros de **como** o scroll vertical é normalizado (entidade “se necessário” na especificação).

| Campo | Tipo | Obrigatório | Regras / notas |
|-------|------|-------------|----------------|
| `invertVertical` | booleano | não | Se `true`, inverte a direção vertical em relação ao evento de entrada (só se alinhado aos testes). |
| `linesPerNotchApprox` | número | não | Metáfora de “linhas” por detente; valores concretos no código e roteiro de testes. |
| `touchpadSameAsWheel` | booleano | sim | MVP: `true` — mesma regra quando o mecanismo permitir. |
| `useVScrollFallback` | booleano | não | Se `true`, converte scroll para `WM_VSCROLL` (`SB_LINEUP`/`SB_LINEDOWN`) para apps legadas que ignoram `WM_MOUSEWHEEL`. |

---

## Transições de estado (ligado / desligado)

1. Utilizador altera `activation.enabled` na UI → persistir imediatamente ou com “Aplicar” explícito (definir na implementação; preferir persistência imediata para RF-005).
2. Arranque da aplicação → ler ficheiro → se inválido → estado seguro (fix **desligado**, lista vazia ou recuperação) + notificação.
3. Encerramento normal → último estado gravado disponível no próximo arranque (P3).

---

## Relações

- `AppConfig` **contém** 0..N `InclusionEntry` e **uma** `ActivationPreference`.
- `BehaviorProfile` **opcional** em `AppConfig`; se ausente, usar valores por defeito.

---

## Estado em tempo de execução (fora do JSON)

**Instância única e UI (RF-011, RF-012)**: o mutex/IPC e o separador ativo na janela principal após um segundo arranque **não** fazem parte do documento persistido; são **apenas** estado de processo e devem cumprir a especificação sem exigir novos campos em `AppConfig`.
