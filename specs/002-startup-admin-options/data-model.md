# Modelo de dados — 002-startup-admin-options

Extensão do modelo em `specs/001-mouse-scroll-fix/data-model.md`. O documento raiz **`AppConfig`** ganha a secção opcional **`startup`**.

---

## Entidade: `StartupPreferences` (`startup`)

Preferências de arranque persistidas (RF-002, RF-005, RF-009).

| Campo | Tipo | Obrigatório | Default se ausente | Regras / notas |
|-------|------|-------------|-------------------|----------------|
| `autoStartWithWindows` | booleano | não | `false` | `true` → registo em `HKCU\...\Run` (ver `research.md`). |
| `runAsAdmin` | booleano | não | `false` | `true` → gate UAC no `Program.Main` em cada arranque. |

**Objeto `startup` inteiro omitido**: tratar como `{ autoStartWithWindows: false, runAsAdmin: false }`.

**Config corrompida / inválida**: política MVP existente — estado seguro com ambos `false` após merge.

---

## Atualização: `AppConfig` (documento raiz)

| Campo | Tipo | Obrigatório | Regras / notas |
|-------|------|-------------|----------------|
| `schemaVersion` | inteiro | sim | **Permanece `1`** nesta feature. |
| `activation` | objeto | sim | Inalterado (MVP). |
| `inclusionList` | lista | sim | Inalterado (MVP). |
| `behavior` | objeto | não | Inalterado (MVP). |
| `startup` | objeto `StartupPreferences` | não | Novo; ver acima. |

---

## Estado em tempo de execução (fora do JSON)

| Conceito | Descrição |
|----------|-----------|
| **Registo Run** | Valor `MouseScrollFixer` em `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`; **não** espelhado literalmente no JSON — derivado de `autoStartWithWindows` + caminho atual do exe. |
| **Elevação efetiva** | Token do processo (`IsProcessElevated`); pode divergir temporariamente de `runAsAdmin` até reinício (RF-013). |
| **Desalinhamento Run** | `autoStartWithWindows == true` no JSON mas registo Run ausente ou com caminho diferente → flag de UI `StartupMisaligned` (não persistida). |

---

## Transições de estado

### `autoStartWithWindows`

1. Utilizador **ativa** na UI → `startup.autoStartWithWindows = true` → persistir → **`Register(currentExePath)`**.
2. Utilizador **desativa** → `false` → persistir → **`Unregister()`**.
3. **Abrir configurações** com preferência `true` e Run ausente → aviso + ação **Reativar registo** → `Register` (JSON inalterado).
4. **Mover executável** → utilizador reativa opção ou usa “Reativar” para atualizar caminho no Run.

### `runAsAdmin`

1. Utilizador **ativa** → persistir → aviso + opcional **Reiniciar agora** (RF-013).
2. **Próximo arranque** (ou reinício imediato) → gate UAC se `true` e não elevado.
3. **UAC negado** → processo continua **não elevado**; JSON mantém `runAsAdmin: true`.
4. Utilizador **desativa** → persistir → reinício necessário para deixar de pedir UAC.

### Combinação

- Ambos `true`: Run key aponta para o **mesmo** `.exe`; elevação ocorre no entrypoint, não no registo.

---

## Relações

- `AppConfig` **contém zero ou um** `StartupPreferences`.
- `StartupPreferences` **não** altera `ActivationPreference`, `InclusionEntry` nem `BehaviorProfile`.
- Serviço **`WindowsStartupRegistration`** **implementa** efeito lateral de `autoStartWithWindows` no SO.

---

## Validação (`AppConfigValidator`)

| Regra | Severidade |
|-------|------------|
| `startup` nulo após merge | OK (merge preenche defaults) |
| Campos booleanos não booleanos | Inválido (JSON parse falha antes ou validação rejeita) |
| Combinações `true/true` | **Válido** — sem invariante extra |

Nenhum teto de entradas adicional; booleans livres.

---

## Mapeamento código (referência)

| Artefato spec | Tipo C# proposto |
|---------------|------------------|
| `startup` | `StartupPreferences` em `Core/Configuration/` |
| Registo Run | `WindowsStartupRegistration` em `Core/Startup/` |
| Elevação | `ProcessElevationHelper` em `Native/Win32/` ou `Core/Startup/` |
