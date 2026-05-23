---
description: "Lista de tarefas para implementação da feature 002-startup-admin-options"
---

# Tarefas: opções de arranque com Windows e execução como administrador

**Entrada**: Documentos de desenho em `specs/002-startup-admin-options/`  
**Pré-requisitos**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Testes automatizados**: Incluídos para validação de configuração (`CS-005`); UAC, Run key e reinício de sessão permanecem **manuais** (`quickstart.md`).

**Organização**: Tarefas agrupadas por história de utilizador para implementação e verificação independentes.

## Formato: `[ID] [P?] [História] Descrição`

- **[P]**: Pode executar em paralelo (ficheiros distintos, sem dependências em tarefas incompletas)
- **[USn]**: História de utilizador em `spec.md` (US1…US3)
- Incluir caminhos de ficheiros exatos nas descrições

## Convenções de caminhos

- Código: `src/MouseScrollFixer/`
- Testes: `tests/MouseScrollFixer.Tests/`
- Contratos: `specs/002-startup-admin-options/contracts/`

---

## Fase 1: Configuração (infraestrutura partilhada)

**Objetivo**: Confirmar branch e alinhar contrato JSON com o modelo antes de alterar código.

- [x] T001 Confirmar branch `002-startup-admin-options` e revisar contrato `startup` em `specs/002-startup-admin-options/contracts/app-config.schema.json` face a `data-model.md`

---

## Fase 2: Fundamentos (pré-requisitos bloqueantes)

**Objetivo**: Modelo `startup`, merge/defaults e testes de regressão de config — **obrigatório** antes das histórias US1/US2.

**⚠️ Crítico**: Nenhum trabalho de história de utilizador deve começar antes desta fase estar completa.

- [x] T002 [P] Criar `StartupPreferences` (`autoStartWithWindows`, `runAsAdmin`, defaults `false`) em `src/MouseScrollFixer/Core/Configuration/StartupPreferences.cs`
- [x] T003 Adicionar propriedade `Startup` a `AppConfig` e `CreateDefault()` em `src/MouseScrollFixer/Core/Configuration/AppConfig.cs`
- [x] T004 Estender `AppConfigStore.MergeDefaults` para garantir `config.Startup` não nulo com ambos `false` se ausente em `src/MouseScrollFixer/Core/Configuration/AppConfigStore.cs`
- [x] T005 [P] Adicionar testes de merge/validação: config legada sem `startup` válida; config com `startup` explícita — em `tests/MouseScrollFixer.Tests/Configuration/AppConfigValidatorTests.cs`
- [x] T006 [P] Adicionar chaves pt-BR base da secção Arranque (título do grupo, rótulos genéricos) em `src/MouseScrollFixer/UI/Resources/Strings.pt-BR.resx`

**Checkpoint**: `dotnet test MouseScrollFixer.sln -c Release` passa; configs MVP antigas continuam válidas (`CS-005`).

---

## Fase 3: História de utilizador 1 — Iniciar automaticamente com o Windows (Prioridade: P1) 🎯 MVP

**Objetivo**: Checkbox na UI, persistência `startup.autoStartWithWindows`, registo/desregisto em `HKCU\...\Run` (RF-001, RF-002, RF-003, RF-014).

**Teste independente**: Roteiro P1 “Arranque automático (isolado)” em `specs/002-startup-admin-options/quickstart.md` — sem ativar “administrador”.

- [x] T007 [US1] Implementar `WindowsStartupRegistration` (`Register`, `Unregister`, `IsRegistered`, normalização de caminho) em `src/MouseScrollFixer/Core/Startup/WindowsStartupRegistration.cs`
- [x] T008 [P] [US1] Adicionar testes unitários de normalização/comparação de caminho do registo em `tests/MouseScrollFixer.Tests/Configuration/WindowsStartupRegistrationTests.cs` (mock ou wrapper testável)
- [x] T009 [US1] Adicionar `GroupBox`/secção **Arranque** e `CheckBox` “Iniciar automaticamente com o Windows” em `src/MouseScrollFixer/UI/MainSettingsForm.cs` (`BuildUi`, `LoadFromConfig`, handler com supressão no load)
- [x] T010 [US1] Ao alterar auto-start: atualizar `config.Startup.AutoStartWithWindows`, `AppConfigStore.Save`, chamar `Register`/`Unregister` com `Environment.ProcessPath` em `src/MouseScrollFixer/UI/MainSettingsForm.cs`
- [x] T011 [US1] Detetar desalinhamento Run (preferência `true`, registo ausente/caminho diferente): painel/aviso pt-BR + botão **Reativar arranque automático** sem alterar JSON para `false` (RF-014) em `src/MouseScrollFixer/UI/MainSettingsForm.cs`
- [x] T012 [P] [US1] Strings pt-BR para auto-start, aviso de desalinhamento e botão reativar em `src/MouseScrollFixer/UI/Resources/Strings.pt-BR.resx`

**Checkpoint**: Ativar/desativar auto-start reflete JSON e Run key; reinício de sessão inicia ou não a app conforme opção.

---

## Fase 4: História de utilizador 2 — Iniciar sempre como administrador (Prioridade: P1)

**Objetivo**: Checkbox `runAsAdmin`, gate UAC no arranque, fallback sem elevação com aviso, “Reiniciar agora” (RF-004…RF-007, RF-013).

**Teste independente**: Roteiro P1 “Executar como administrador (isolado)” em `quickstart.md` — auto-start desligado.

- [x] T013 [P] [US2] Implementar `ProcessElevationHelper` (`IsProcessElevated`, tentativa `Verb=runas` / tratamento `Win32Exception` 1223) em `src/MouseScrollFixer/Native/Win32/ProcessElevationHelper.cs`
- [x] T014 [US2] Refatorar `Program.Main`: cultura pt-BR → leitura mínima de `startup.runAsAdmin` → gate UAC → mutex → `RunApplication` em `src/MouseScrollFixer/Program.cs` (ordem conforme `research.md`)
- [x] T015 [US2] Em UAC negado: `MessageBox` pt-BR e continuar arranque **sem** elevação no processo atual (RF-007) em `src/MouseScrollFixer/Program.cs`
- [x] T016 [US2] Adicionar `CheckBox` “Iniciar sempre como administrador”, load/save em `config.Startup.RunAsAdmin` em `src/MouseScrollFixer/UI/MainSettingsForm.cs`
- [x] T017 [US2] Ao alterar `runAsAdmin` com app em execução: persistir, aviso pt-BR e diálogo **Reiniciar agora** / **Mais tarde**; **Sim** → `Process.Start` do exe + `Application.Exit()` (RF-013) em `src/MouseScrollFixer/UI/MainSettingsForm.cs`
- [x] T018 [P] [US2] Strings pt-BR para administrador, UAC negado, reinício e botões do diálogo em `src/MouseScrollFixer/UI/Resources/Strings.pt-BR.resx`

**Checkpoint**: UAC aprovado → processo elevado; UAC negado → app normal + aviso; reinício imediato opcional funciona.

---

## Fase 5: História de utilizador 3 — Combinação das duas opções (Prioridade: P2)

**Objetivo**: Arranque automático dispara o mesmo gate UAC; instância única e balão MVP mantidos (RF-012, história P2).

**Teste independente**: Roteiro P2 em `quickstart.md` (ambas opções ativas + negar UAC no login).

- [x] T019 [US3] Garantir que entrada Run aponta para o executável atual (não launcher separado) e que arranque via Run passa pelo gate UAC quando `runAsAdmin` true — rever integração em `src/MouseScrollFixer/Program.cs` e `src/MouseScrollFixer/Core/Startup/WindowsStartupRegistration.cs`
- [x] T020 [US3] Validar que elevação não quebra `SingleInstanceCoordinator` (mutex antes de UI, processo não elevado stub termina após elevação bem-sucedida) — ajustes se necessário em `src/MouseScrollFixer/Program.cs` e `src/MouseScrollFixer/SingleInstance/SingleInstanceCoordinator.cs`
- [x] T021 [US3] Executar roteiro manual combinado (auto-start + admin, UAC aprovado/negado) e registar resultados conforme matriz em `specs/002-startup-admin-options/quickstart.md`

**Checkpoint**: Fluxo completo login → UAC → bandeja elevada; negação UAC no auto-arranque cumpre RF-007.

---

## Fase 6: Polimento e transversal

**Objetivo**: Regressão MVP, build e documentação mínima de verificação.

- [x] T022 [P] Executar `dotnet build MouseScrollFixer.sln -c Release` e `dotnet test MouseScrollFixer.sln -c Release` na raiz do repositório
- [x] T023 Regressão RF-011/RF-012: segundo arranque ativa configurações; balão na bandeja — roteiros 7–8 do MVP em `specs/001-mouse-scroll-fix/quickstart.md`
- [x] T024 [P] Atualizar exemplo JSON com `startup` em `docs/context.md` (secção persistência) se ainda não refletir a feature

---

## Dependências e ordem de execução

### Dependências entre fases

- **Fase 1** → **Fase 2** → **Fases 3 e 4** (US1 e US2 podem seguir em paralelo após Fase 2) → **Fase 5 (US3)** → **Fase 6**

### Dependências entre histórias

| História | Depende de | Independente para teste |
|----------|------------|-------------------------|
| **US1** | Fase 2 | Sim — só auto-start |
| **US2** | Fase 2 | Sim — só admin/UAC |
| **US3** | US1 + US2 | Combinação e regressão instância única |

### Dentro de cada história

- Serviços core (`WindowsStartupRegistration`, `ProcessElevationHelper`) antes de UI que os invoca
- Strings `.resx` em paralelo quando marcadas [P]
- US3 apenas após US1 e US2 funcionais isoladamente

### Oportunidades de paralelismo

```text
Após Fase 2:
  Stream A (US1): T007 → T009 → T010 → T011  ||  T008, T012 [P]
  Stream B (US2): T013 → T014 → T015 → T016 → T017  ||  T018 [P]
```

---

## Estratégia de implementação

### MVP primeiro (US1 apenas)

1. Fase 1 + Fase 2  
2. Fase 3 (US1)  
3. **Parar e validar** roteiro auto-start em `quickstart.md`  
4. Entregar incremento utilizável (arranque com Windows)

### Entrega incremental

1. Fundamentos → US1 (auto-start) → demo  
2. US2 (admin + UAC) → demo  
3. US3 (combinação) + polimento → release candidate  

### Escopo sugerido MVP

**US1** (8 tarefas: T001–T012, excluindo US2/US3) entrega valor imediato alinhado ao backlog item “Auto start com Windows”.

---

## Resumo

| Métrica | Valor |
|---------|-------|
| **Total de tarefas** | 24 |
| **Fase 1 (Setup)** | 1 |
| **Fase 2 (Fundamentos)** | 5 |
| **US1 (P1)** | 6 |
| **US2 (P1)** | 6 |
| **US3 (P2)** | 3 |
| **Polimento** | 3 |
| **Tarefas paralelizáveis [P]** | 9 |

### Critérios de teste independente por história

| História | Critério |
|----------|----------|
| **US1** | JSON + Run key; reinício de sessão com/sem auto-início |
| **US2** | UAC aprovado/negado; reiniciar agora; JSON `runAsAdmin` |
| **US3** | Ambas ativas no login; instância única + balão MVP |

**Próximo comando sugerido**: `/speckit.implement` ou execução manual das tarefas T001→T024.
