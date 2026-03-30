---
description: "Lista de tarefas para implementação da feature 001-mouse-scroll-fix"
---

# Tarefas: correção do comportamento do scroll do mouse (MVP)

**Entrada**: Documentos de desenho em `specs/001-mouse-scroll-fix/`  
**Pré-requisitos**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Testes automatizados**: Opcionais nesta lista — a especificação prioriza roteiros manuais e lógica testável em projeto de testes; incluir projetos de teste apenas onde o plano citar validação de configuração.

**Organização**: Tarefas agrupadas por história de utilizador para permitir implementação e verificação independentes.

## Formato: `[ID] [P?] [História] Descrição`

- **[P]**: Pode executar em paralelo (ficheiros distintos, sem dependências em tarefas incompletas)
- **[USn]**: História de utilizador em `spec.md` (US1…US4)
- Incluir caminhos de ficheiros exatos nas descrições

## Convenções de caminhos

- Código: `src/MouseScrollFixer/`
- Testes: `tests/MouseScrollFixer.Tests/`
- Contratos: `specs/001-mouse-scroll-fix/contracts/`

---

## Fase 1: Configuração (infraestrutura partilhada)

**Objetivo**: Solução .NET, projeto `net8.0-windows` e estrutura de pastas alinhada a `plan.md`.

- [x] T001 Criar ou alinhar `MouseScrollFixer.sln` na raiz do repositório com projetos `src/MouseScrollFixer` e `tests/MouseScrollFixer.Tests`
- [x] T002 Configurar `src/MouseScrollFixer/MouseScrollFixer.csproj` (WinExe, `net8.0-windows`, C# 12, WinForms, `ApplicationManifest`, `InternalsVisibleTo` para o projeto de testes)
- [x] T003 [P] Configurar `tests/MouseScrollFixer.Tests/MouseScrollFixer.Tests.csproj` com referência ao projeto principal e SDK de testes (xUnit ou MSTest conforme `plan.md`)

---

## Fase 2: Fundamentos (pré-requisitos bloqueantes)

**Objetivo**: Configuração persistida, validação, verificação de SO, base Win32 e arranque da aplicação — **obrigatório** antes das histórias de utilizador.

**⚠️ Crítico**: Nenhum trabalho de história de utilizador deve começar antes desta fase estar completa.

- [x] T004 [P] Implementar modelos `AppConfig`, `ActivationPreference`, `InclusionEntry`, `BehaviorProfile` e enums alinhados a `data-model.md` em `src/MouseScrollFixer/Core/Configuration/`
- [x] T005 Implementar serialização JSON (`schemaVersion`, opções `System.Text.Json`) e ficheiro sob perfil do utilizador (`%LocalAppData%\MouseScrollFixer\`) em `src/MouseScrollFixer/Core/Configuration/AppConfigStore.cs` e `AppConfigJson.cs`
- [x] T006 Implementar `AppConfigValidator` com teto de 64 entradas, duplicados de caminho e regras de `MatchKind` em `src/MouseScrollFixer/Core/Configuration/AppConfigValidator.cs`
- [x] T007 Tratar ficheiro corrompido ou inválido com estado seguro (fix desligado, lista vazia ou política documentada) e mensagem em pt-BR em `src/MouseScrollFixer/Core/Configuration/AppConfigLoadResult.cs` e fluxo em `Program.cs`
- [x] T008 [P] Declarar P/Invoke e constantes Win32 necessários (user32, kernel32, estruturas de hook) em `src/MouseScrollFixer/Native/Win32/`
- [x] T009 Implementar verificação **Windows 11** (build mínimo conforme `research.md` / RF-008) em `src/MouseScrollFixer/Native/Win32/OsVersionHelper.cs` e recusa amigável em `src/MouseScrollFixer/Program.cs`
- [x] T010 Implementar `ScrollFixerSession` (instalação/remoção do hook conforme `activation.enabled`) em `src/MouseScrollFixer/App/ScrollFixerSession.cs`
- [x] T011 Implementar `TrayApplication` / `TrayApplicationContext` com `NotifyIcon`, menu de contexto em pt-BR e fluxo para abrir definições em `src/MouseScrollFixer/App/TrayApplication.cs`
- [x] T012 Criar `MainSettingsForm` com separador de **configurações** identificável (TabControl ou equivalente) e textos em `src/MouseScrollFixer/UI/MainSettingsForm.cs` e `UI/Resources/Strings.pt-BR.resx`
- [x] T013 Centralizar cadeias de UI em pt-BR em `src/MouseScrollFixer/UI/Resources/UiStrings.cs` e ficheiros `.resx`

**Checkpoint**: Configuração lida/gravada, SO validado, bandeja e formulário base funcionais — **seguir pela Fase 3 (US4)** para cumprir RF-011 e RF-012 antes do trabalho de scroll (US1), evitando builds intermédios sem instância única.

---

## Fase 3: História de utilizador 4 — Instância única e aviso de arranque (Prioridade: P2)

**Objetivo**: Mutex/IPC para uma instância por sessão; segundo arranque ativa a janela existente, primeiro plano, visível, com separador de configurações; aviso observável em todo arranque quando só bandeja (RF-011, RF-012).

**Ordem**: Colocada **antes** de US1/US2 para que qualquer binário após o fundamento respeite instância única e feedback de arranque exigidos pelo MVP.

**Teste independente**: Roteiros 7 e 8 em `quickstart.md` e CS-006.

- [x] T025 [US4] Implementar exclusão mútua (mutex nomeado `Local\` + identificador do produto) em `src/MouseScrollFixer/SingleInstance/SingleInstanceCoordinator.cs` (criar pasta se necessário)
- [x] T026 [US4] Implementar sinalização do segundo processo à instância existente (mensagem registada, pipe ou memória mapeada) para restaurar `MainSettingsForm`, `BringToFront` e selecionar o separador de configurações
- [x] T027 [US4] Integrar fluxo em `Program.cs`: se segunda instância, não iniciar segunda UI principal — apenas sinalizar e sair
- [x] T028 [US4] Garantir aviso observável (balão `NotifyIcon` e/ou notificação Windows 11) em **cada** arranque quando a janela principal não estiver visível, texto fixo em pt-BR; quando a janela já visível, satisfazer RF-012 sem balão redundante conforme `research.md` — em `TrayApplication.cs` e recursos

**Checkpoint**: RF-011 e RF-012 demonstráveis.

---

## Fase 4: História de utilizador 1 — Scroll previsível (Prioridade: P1) — MVP

**Objetivo**: Lista de inclusão editável, hook de baixo nível, resolução de janela/processo e normalização do scroll vertical apenas para entradas listadas (RF-001).

**Teste independente**: Com o fix ativo e um app de teste na lista, verificar roda e (se disponível) touchpad conforme roteiro em `quickstart.md`.

- [x] T014 [P] [US1] Implementar `LowLevelMouseHook` com `WH_MOUSE_LL` e caminho sem bloquear o callback em `src/MouseScrollFixer/Hooks/LowLevelMouseHook.cs`
- [x] T015 [P] [US1] Implementar resolução de HWND / executável (`WindowFromPoint`, `GetAncestor`, caminho do processo) em `src/MouseScrollFixer/Core/ScrollNormalization/WindowTargetResolver.cs`
- [x] T016 [US1] Implementar `ScrollNormalizer` (direção, delta/`WHEEL_DELTA`, ignorar horizontal no MVP) em `src/MouseScrollFixer/Core/ScrollNormalization/ScrollNormalizer.cs`
- [x] T017 [US1] Integrar lista de inclusão e `activation.enabled` no decisor “aplicar fix a este alvo” no fluxo do hook e sessão
- [x] T018 [US1] Implementar UI da lista (adicionar/remover, diálogo de ficheiro `.exe`, mensagens de limite/invalidação) em `src/MouseScrollFixer/UI/MainSettingsForm.cs`
- [x] T019 [US1] Persistir alterações da lista e refletir no comportamento sem exigir reinício do Windows (salvo limitação documentada)
- [x] T020 [P] [US1] Opcional: testes de validação de `AppConfig` / regras de inclusão em `tests/MouseScrollFixer.Tests/Configuration/` conforme `contracts/app-config.schema.json`

**Checkpoint**: Critérios de aceite P1 da especificação verificáveis no ambiente de referência.

---

## Fase 5: História de utilizador 2 — Controlo de ativação (Prioridade: P2)

**Objetivo**: Ligar/desligar o fix sem reinstalar o SO; persistir `activation.enabled` entre sessões; alinhar primeira preferência ao instalador quando existir (RF-002, RF-003).

**Teste independente**: Alternar estado e observar o mesmo aplicativo de teste; reiniciar o Windows e confirmar CS-005 quando aplicável.

- [x] T021 [US2] Expor controlo claro de ativação (mesmo formulário ou bandeja) com persistência imediata em `src/MouseScrollFixer/UI/MainSettingsForm.cs` e `TrayApplication.cs`
- [x] T022 [US2] Garantir que `ScrollFixerSession` aplica/remove o hook quando `activation.enabled` muda e grava com validação
- [x] T023 [US2] Documentar na UI ou ajuda embutida **o que muda** com o fix ligado (RF-003) em recursos pt-BR
- [x] T024 [P] [US2] Preparar ou alinhar passo de instalador (Inno Setup / WiX / MSIX) com checkbox «Ativar o fix ao concluir» e escrita da primeira preferência em `installer/` ou documentação em `installer/README.md`, conforme `quickstart.md`

**Checkpoint**: RF-002 e cenários P2 de ativação cobertos.

---

## Fase 6: História de utilizador 3 — Encerramento e recuperação (Prioridade: P3)

**Objetivo**: Sair sem deixar o sistema inconsistente; após reinício, preferência persistida e comportamento coerente (P3, CS-003, CS-005).

**Teste independente**: Encerramento pelo menu da bandeja, reinício de sessão, verificação de hook desinstalado e JSON válido.

- [x] T029 [US3] Garantir desinstalação do hook e libertação de recursos no encerramento normal em `ScrollFixerSession` e `TrayApplication`
- [x] T030 [US3] Persistir último estado em `ApplicationExitPersistence` / `Application.ApplicationExit` em `src/MouseScrollFixer/App/ApplicationExitPersistence.cs` e `Program.cs`
- [x] T031 [US3] Validar roteiro pós-reinício: último `activation.enabled` aplicado e ausência de efeitos indesejados documentados (lista de regressão)

**Checkpoint**: P3 e critérios de encerramento satisfeitos.

---

## Fase 7: Polimento e requisitos transversais

**Objetivo**: Conflitos (RF-007), impacto mínimo, strings, verificação final com `quickstart.md`.

- [x] T032 [P] Completar ou rever heurísticas em `src/MouseScrollFixer/Core/ConflictDetection/ConflictDetector.cs` e notificação ao utilizador (sem precedência automática)
- [x] T033 Rever textos de erro e estados «fix ativo/inativo» para RF-005 em recursos pt-BR
- [x] T034 [P] Confirmar ausência de telemetria e dados remotos não autorizados (RF-010) e documentar verificação opcional de atualizações apenas se existir
- [x] T035 Executar validação manual com `specs/001-mouse-scroll-fix/quickstart.md` e checklist em `specs/001-mouse-scroll-fix/checklists/regression.md`

---

## Dependências e ordem de execução

### Dependências entre fases

- **Fase 1**: Sem dependências externas.
- **Fase 2**: Depende da Fase 1 — **bloqueia** todas as histórias.
- **Fase 3 (US4)**: Depende da Fase 2; **antecede** US1/US2 para RF-011 e RF-012 no binário.
- **Fases 4–6 (US1, US2, US3)**: Dependem da Fase 2 e **de preferência** da Fase 3 concluída. Entre si: **US1** (valor central do scroll); **US2** reforça controlo explícito e documentação; **US3** fecha ciclo de vida.
- **Fase 7**: Depende das fases anteriores conforme o âmbito a polir.

### Dependências entre histórias

- **US4 (P2)**: Após fundamento (Fase 2); requer separador de configurações identificável (`MainSettingsForm` da Fase 2); integra `Program.cs` e bandeja — **antes** de US1 nesta lista para evitar violação de RF-011/012 em entregas intermédias.
- **US1 (P1)**: Após Fase 2 e, de preferência, Fase 3; valor central do produto.
- **US2 (P2)**: Melhor após US1 ter hook e lista testáveis; pode sobrepor-se parcialmente a trabalho já feito na bandeja.
- **US3 (P3)**: Depende de hook e persistência definidos (US1/US2).

### Dentro de cada história

- P/Invoke e resolução de janela antes ou em paralelo com normalização; UI da lista em paralelo com testes de contrato se existirem.
- Hook sempre após validação de configuração e sessão.

### Oportunidades de paralelização

- Tarefas **[P]** na mesma fase (ficheiros diferentes).
- Modelos de configuração em paralelo com declarações Win32 na Fase 2.
- `WindowTargetResolver` em paralelo com `ScrollNormalizer` após contrato de mensagens definido.

---

## Exemplo paralelo: US1

```text
# Em paralelo após T028 (US4 concluída):
T014 LowLevelMouseHook em src/MouseScrollFixer/Hooks/LowLevelMouseHook.cs
T015 WindowTargetResolver em src/MouseScrollFixer/Core/ScrollNormalization/WindowTargetResolver.cs
```

---

## Estratégia de implementação

### MVP primeiro (RF-011/012 antes do núcleo de scroll)

1. Concluir Fases 1 e 2.
2. Concluir Fase 3 (US4 — instância única e aviso de arranque).
3. Concluir Fase 4 (US1).
4. **Parar e validar** com roteiro P1 e `quickstart.md` (secção uso mínimo), incluindo CS-006 onde aplicável.

### Entrega incremental

1. Fundamento → US4 → instância única e feedback de arranque conforme especificação.
2. US1 → demonstração de valor (scroll).
3. US2 → confiança e persistência explícita.
4. US3 → robustez de encerramento.
5. Fase 7 → qualidade de release.

### Equipa paralela (referência)

- Após Fase 2: um desenvolvedor em US4 (mutex/IPC + avisos), outro em paralelo em contratos/testes de configuração e strings pt-BR; após T028, paralelizar T014/T015 conforme exemplo acima.

---

## Notas

- **[P]** = ficheiros distintos, sem dependência de ordem entre si.
- **[USn]** liga a rastreio na `spec.md`.
- Marcar tarefas concluídas com `[x]` durante `/speckit.implement`.
- Evitar tarefas vagas ou conflitos no mesmo ficheiro sem coordenação.
