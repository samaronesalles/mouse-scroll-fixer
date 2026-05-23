# Plano de implementação: opções de arranque com Windows e execução como administrador

**Branch**: `002-startup-admin-options` | **Data**: 23/05/2026 | **Especificação**: [spec.md](./spec.md)

**Entrada**: Especificação da feature em `specs/002-startup-admin-options/spec.md` (clarificações 2026-05-23 incluídas).

**Nota**: Preenchido pelo comando `/speckit.plan`. Artefatos de Fase 0–1: `research.md`, `data-model.md`, `contracts/`, `quickstart.md`. Tarefas em `tasks.md` serão geradas por `/speckit.tasks`.

## Resumo

Estender o MVP **MouseScrollFixer** (.NET 8 / WinForms) com duas preferências na janela de configurações, persistidas em `app-config.json` (secção **`startup`**, `schemaVersion: 1`):

1. **`autoStartWithWindows`** — registo/desregisto em **`HKCU\...\Run`** (`MouseScrollFixer`).
2. **`runAsAdmin`** — gate **UAC** no início de `Program.Main` (manifest `asInvoker`); UAC negado → continua **sem elevação** com aviso pt-BR.

Inclui **“Reiniciar agora”** ao alterar admin com app em execução, **aviso de desalinhamento** se Run for removido externamente, e testes xUnit para merge/validação JSON.

## Contexto técnico

**Linguagem / versão**: C# 12 / .NET 8 (`net8.0-windows`)  
**Dependências principais**: WinForms, P/Invoke (user32/kernel32/advapi32 conforme necessário), System.Text.Json, `Microsoft.Win32.Registry`  
**Armazenamento**: `app-config.json` em `%LocalAppData%\MouseScrollFixer\` + registo **`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`**  
**Testes**: xUnit — merge/validação `startup`; testes manuais Windows 11 para UAC, Run key, reinício de sessão (`quickstart.md`)  
**Plataforma alvo**: Windows 11 x64 (mesmo gate `OsVersionHelper` do MVP)  
**Tipo de projeto**: desktop-app (extensão do utilitário de bandeja existente)  
**Metas de desempenho**: arranque automático visível na bandeja em ≤2 min (CS-002); gate UAC sem bloquear UI além do prompt do SO  
**Restrições**: privilégios mínimos por defeito; elevação opt-in; pt-BR na UI; sincronização JSON ↔ UI ↔ registo; instância única MVP preservada  
**Escala / âmbito**: 2 booleans + 1 serviço de registo + bootstrap em `Program.cs`; ~8–12 ficheiros tocados

## Verificação da constituição

*GATE: deve passar antes da Fase 0. Reavaliado após Fase 1.*

| Critério | Estado |
|----------|--------|
| **SDD** | `spec.md` → clarificações → este `plan.md` → `tasks.md` (próximo passo). |
| **Escopo** | Apenas arranque automático e elevação configurável; sem alterar núcleo do fix de scroll. |
| **Windows / impacto** | Run key (HKCU) e UAC documentados em `research.md`; riscos de elevação no `quickstart.md`. |
| **Verificação** | Cenários Given/When/Then na spec + roteiros em `quickstart.md` + testes de config. |
| **pt-BR** | Strings novas em `Strings.pt-BR.resx`; artefatos Spec Kit em pt-BR. |

**Stack**: continua **C# / .NET** (ADR-001); sem novo desvio em relação ao MVP 001.

### Reavaliação pós-desenho (Fase 1)

- `data-model.md` e `contracts/app-config.schema.json` cobrem RF-002, RF-005, RF-009.
- `research.md` cobre RF-003, RF-006, RF-007, RF-013, RF-014 e ordem elevação/mutex.
- RF-011/RF-012 herdados do MVP — sem alteração de contrato JSON.
- Nenhum requisito além da especificação foi adicionado.

## Estrutura do projeto

### Documentação (esta feature)

```text
specs/002-startup-admin-options/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── README.md
│   └── app-config.schema.json
├── checklists/
│   └── requirements.md
└── tasks.md              # /speckit.tasks (não criado por este comando)
```

### Código-fonte (alterações previstas)

```text
src/MouseScrollFixer/
├── Program.cs                          # gate UAC antes do mutex
├── Core/
│   ├── Configuration/
│   │   ├── AppConfig.cs                # + Startup
│   │   ├── StartupPreferences.cs       # novo
│   │   ├── AppConfigStore.cs           # MergeDefaults startup
│   │   └── AppConfigValidator.cs       # se necessário
│   └── Startup/
│       └── WindowsStartupRegistration.cs  # HKCU Run
├── Native/Win32/
│   └── ProcessElevationHelper.cs       # IsProcessElevated + TryElevate
└── UI/
    ├── MainSettingsForm.cs             # checkboxes, aviso desalinhamento, reiniciar
    └── Resources/Strings.pt-BR.resx    # novas chaves

tests/MouseScrollFixer.Tests/
└── Configuration/
    ├── AppConfigValidatorTests.cs      # + startup merge/validate
    └── StartupPreferencesTests.cs      # opcional: defaults
```

**Decisão de estrutura**: extensão mínima do projeto único existente; serviço `WindowsStartupRegistration` isolado para testabilidade e RF-014.

## Fases de implementação (referência para tasks)

### Fase A — Modelo e persistência

- `StartupPreferences` + propriedade `AppConfig.Startup`
- `MergeDefaults`: `startup` nunca nulo; defaults `false`
- Atualizar `AppConfigValidatorTests` e schema em `specs/002/.../contracts/`
- Garantir configs MVP sem `startup` continuam válidas (CS-005)

### Fase B — Registo de arranque (RF-003, RF-014)

- `WindowsStartupRegistration`: Register / Unregister / IsRegistered / GetRegisteredPath
- Invocar Register/Unregister quando checkbox auto-start muda (após Save)
- Em `MainSettingsForm.LoadFromConfig`: detetar desalinhamento + painel aviso + botão reativar

### Fase C — Elevação UAC (RF-006, RF-007)

- `ProcessElevationHelper`
- Refatorar `Program.Main`: cultura → **elevação** → mutex → `RunApplication`
- UAC negado: MessageBox pt-BR, continuar não elevado

### Fase D — UI (RF-001, RF-004, RF-008, RF-013)

- GroupBox **Arranque** com dois `CheckBox`
- Handler `runAsAdmin` changed → persist → dialog reiniciar agora
- Strings pt-BR (rótulos, avisos, botões)

### Fase E — Verificação

- `dotnet test` + roteiro manual `quickstart.md`
- Regressão instância única e balão bandeja

## Rastreio de complexidade

Sem violações novas da constituição. Elevação opt-in justificada pela spec (apps elevadas); alternativa “sempre asInvoker” rejeitada porque não cumpre RF-006.

| Decisão | Porque | Alternativa rejeitada |
|---------|--------|------------------------|
| Run key HKCU | Arranque por utilizador sem admin para registar | Task Scheduler — complexidade |
| Bootstrap UAC em `Program` | Toggle via UI sem segundo exe | `requireAdministrator` manifest — não configurável |
| `schemaVersion: 1` | Clarificação spec | v2 — migração desnecessária |

## Artefatos gerados

| Artefato | Caminho |
|----------|---------|
| Pesquisa | [research.md](./research.md) |
| Modelo de dados | [data-model.md](./data-model.md) |
| Contrato JSON | [contracts/app-config.schema.json](./contracts/app-config.schema.json) |
| Quickstart / testes | [quickstart.md](./quickstart.md) |
| Plano | Este ficheiro |

**Próximo comando sugerido**: `/speckit.tasks`
