---
description: "Lista de tarefas para implementação da feature 001-mouse-scroll-fix"
---

# Tarefas: correção do comportamento do scroll do mouse

**Entrada**: Artefatos de desenho em `specs/001-mouse-scroll-fix/`  
**Pré-requisitos**: `plan.md`, `spec.md`; opcionais: `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Testes**: A especificação não exige TDD nem suíte automatizada obrigatória; incluem-se apenas tarefas de testes unitários pontuais para **lógica pura** (validação de configuração), alinhadas ao `plan.md`. Validação principal permanece **manual** conforme `quickstart.md`.

**Organização**: Tarefas agrupadas por história de usuário para permitir implementação e verificação independentes.

## Formato: `[ID] [P?] [História] Descrição`

- **[P]**: Pode executar em paralelo (ficheiros diferentes, sem dependência de tarefas incompletas na mesma fase)
- **[US1]**, **[US2]**, **[US3]**: Rastreio às histórias em `spec.md`
- Incluir **caminhos de ficheiro** concretos nas descrições

## Convenções de caminhos

- Raiz do repositório: solução em `src/MouseScrollFixer/`, testes em `tests/MouseScrollFixer.Tests/`, conforme `plan.md`

---

## Fase 1: Configuração (infraestrutura partilhada)

**Objetivo**: Inicialização do projeto e estrutura base

- [x] T001 Criar solução .NET na raiz do repositório e projeto `src/MouseScrollFixer/MouseScrollFixer.csproj` (`net8.0-windows`, WinForms, C# 12) conforme `plan.md`
- [x] T002 [P] Criar árvore de pastas `App/`, `Core/Configuration/`, `Core/ScrollNormalization/`, `Hooks/`, `Native/Win32/`, `UI/Resources/` em `src/MouseScrollFixer/` conforme `plan.md`
- [x] T003 [P] Criar projeto de testes `tests/MouseScrollFixer.Tests/MouseScrollFixer.Tests.csproj` (xUnit ou MSTest) com referência ao projeto principal

---

## Fase 2: Fundações (pré-requisitos bloqueantes)

**Objetivo**: Infraestrutura que **deve** estar pronta antes das histórias de usuário (modelos, persistência, SO, stubs da bandeja)

**⚠️ CRÍTICO**: Nenhuma história de usuário deve avançar antes desta fase estar concluída

- [x] T004 Implementar declarações P/Invoke necessárias (ex.: `SetWindowsHookEx`, `CallNextHookEx`, `UnhookWindowsHookEx`, `WindowFromPoint`, `GetAncestor`, APIs de processo/janela) em `src/MouseScrollFixer/Native/Win32/` conforme `research.md`
- [x] T005 Implementar resolução de alvo (HWND → caminho do executável) em `src/MouseScrollFixer/Core/ScrollNormalization/WindowTargetResolver.cs` alinhado a ADR-004/006 e `data-model.md`
- [x] T006 [P] Definir modelos `AppConfig`, `ActivationPreference`, `InclusionEntry`, `BehaviorProfile` em `src/MouseScrollFixer/Core/Configuration/` conforme `data-model.md` e `contracts/app-config.schema.json`
- [x] T007 Implementar `AppConfigStore` (caminho sob `%LocalAppData%`, leitura/escrita JSON, estado seguro em ficheiro corrompido, política mínima de recuperação) em `src/MouseScrollFixer/Core/Configuration/AppConfigStore.cs`
- [x] T008 Implementar validação de regras (teto 64 entradas, duplicados de `executablePath`, campos obrigatórios) em `src/MouseScrollFixer/Core/Configuration/AppConfigValidator.cs`, consistente com `contracts/app-config.schema.json`
- [x] T009 Verificar **apenas Windows 11** no arranque com mensagem em **pt-BR** e encerramento controlado em `src/MouseScrollFixer/Program.cs` (RF-008)
- [x] T010 Inicializar `TrayApplication` com `NotifyIcon` e menu de contexto mínimo (abrir configurações / sair) em `src/MouseScrollFixer/App/TrayApplication.cs`

**Checkpoint**: Fundações prontas — pode iniciar implementação das histórias

---

## Fase 3: História de usuário 1 — Scroll previsível e lista de inclusão (Prioridade: P1) 🎯 MVP

**Objetivo**: Lista de inclusão editável, scroll vertical normalizado (roda e touchpad quando o mecanismo permitir) **apenas** para entradas na lista, com fix **ligável** na interface para viabilizar os cenários de aceite e o `quickstart.md`

**Teste independente**: Seguir tabela do `quickstart.md` (linhas de verificação rápida) com app de exemplo definido no plano; não depende de US2/US3 além do já entregue nas fundações

### Implementação — US1

- [x] T011 [US1] Implementar `LowLevelMouseHook` (`WH_MOUSE_LL`) em `src/MouseScrollFixer/Hooks/LowLevelMouseHook.cs` sem bloquear o callback (trabalho pesado fora do hook), conforme `research.md`
- [x] T012 [US1] Implementar normalização de scroll vertical (`WM_MOUSEWHEEL`, constantes documentadas, ignorar horizontal) em `src/MouseScrollFixer/Core/ScrollNormalization/ScrollNormalizer.cs`
- [x] T013 [US1] Implementar coordenação (aplicar normalização só se `activation.enabled` e executável da janela em foco na lista; caso contrário repassar evento) em `src/MouseScrollFixer/App/ScrollFixerSession.cs` (ou nome equivalente no `App/`)
- [x] T014 [US1] Implementar `MainSettingsForm` com lista de inclusão (adicionar/remover `.exe`, mensagens pt-BR para limites e erros) e controlo de **ativação ligado/desligado** em `src/MouseScrollFixer/UI/MainSettingsForm.cs` (RF-001, RF-005; suporte ao CS-002)
- [x] T015 [P] [US1] Adicionar recurso de strings **pt-BR** em `src/MouseScrollFixer/UI/Resources/Strings.pt-BR.resx` (RF-009)
- [x] T016 [US1] Incluir texto curto de ajuda embutida explicando o efeito do fix quando ligado (RF-003) em `src/MouseScrollFixer/UI/MainSettingsForm.cs`

**Checkpoint**: US1 funcional e testável isoladamente (cenários P1 da `spec.md`)

---

## Fase 4: História de usuário 2 — Controlo de ativação (Prioridade: P2)

**Objetivo**: Alternar fix ligado/desligado sem reinstalar o SO; persistência imediata; alinhar instalador opcional à primeira preferência (RF-002)

**Teste independente**: Alternar estado no mesmo app de teste e comparar antes/depois; verificar persistência após reabrir o utilitário

### Implementação — US2

- [x] T017 [US2] Ligar menu da bandeja e ícone ao estado de ativação (abrir definições, alternar fix, sair) em `src/MouseScrollFixer/App/TrayApplication.cs`, mantendo UI sincronizada com `activation.enabled`
- [x] T018 [US2] Garantir que, com fix **desligado**, não há efeito do normalizador (desinstalar hook ou repasse transparente documentado) e estado claro na UI (RF-005) em `src/MouseScrollFixer/App/ScrollFixerSession.cs` e formulário
- [x] T019 [US2] Adicionar projeto de **instalador opcional** (Inno Setup, WiX ou equivalente) em `installer/` com textos em **pt-BR** e opção “Ativar o fix ao concluir” que grava a **primeira** preferência conforme RF-002, **ou** documentar fluxo equivalente no `specs/001-mouse-scroll-fix/quickstart.md` se o instalador ficar fora do MVP deste incremento

**Checkpoint**: US1 e US2 verificáveis em conjunto (P1 + P2)

---

## Fase 5: História de usuário 3 — Encerramento e recuperação (Prioridade: P3)

**Objetivo**: Sair sem deixar o sistema inconsistente; preferência persistida após reinício de sessão (alinhado a RF-002 e cenários P3)

**Teste independente**: Encerramento pelo fluxo oficial, reinício do Windows, confirmar comportamento com roteiro em `quickstart.md`

### Implementação — US3

- [x] T020 [US3] Implementar encerramento limpo: `UnhookWindowsHookEx`, gravar configuração, libertar `NotifyIcon` em `src/MouseScrollFixer/App/TrayApplication.cs` e `Program.cs` (handlers de saída)
- [x] T021 [US3] Garantir que o arranque aplica `activation.enabled` persistido sem exigir reativação manual quando o último estado foi “ligado” (CS-005) em `src/MouseScrollFixer/App/TrayApplication.cs` / `ScrollFixerSession.cs`

**Checkpoint**: Fluxo de saída e reinício conforme P3

---

## Fase 6: Polimento e transversal

**Objetivo**: Requisitos transversais e endurecimento

- [x] T022 [P] Implementar heurística de **deteção de conflito** e notificação ao utilizador em **pt-BR** (balloon ou diálogo), sem precedência automática (RF-007) em `src/MouseScrollFixer/Core/ConflictDetection/` (ou pasta equivalente)
- [x] T023 [P] Adicionar testes unitários para `AppConfigValidator` (e regras puras associadas) em `tests/MouseScrollFixer.Tests/Configuration/`
- [x] T024 [P] Revisão final: mensagens para configuração corrompida (estado seguro), ausência de telemetria (RF-010), e validação manual guiada por `specs/001-mouse-scroll-fix/quickstart.md` (checklist de regressão)

---

## Dependências e ordem de execução

### Dependências entre fases

- **Fase 1**: Sem dependências externas
- **Fase 2**: Depende da Fase 1 — **bloqueia** todas as histórias
- **Fases 3–5 (US1 → US3)**: Dependem da Fase 2; ordem sugerida **P1 → P2 → P3** (também é a ordem de prioridade na `spec.md`)
- **Fase 6**: Depende das histórias pretendidas para o incremento

### Dependências entre histórias

- **US1 (P1)**: Após Fase 2; não depende de US2/US3
- **US2 (P2)**: Após Fase 2 e idealmente com US1 já integrada (bandeja + sessão); testável de forma independente ao alternar estado
- **US3 (P3)**: Após ter hook e persistência (tipicamente após US1–US2)

### Oportunidades de paralelização

- T002, T003 em paralelo após T001
- T006 em paralelo com T005 após T004 disponível para tipos nativos (ajustar se T006 só precisar de POCOs — pode paralelizar com T004/T005 em parte)
- T015 em paralelo com outras tarefas US1 após existir esqueleto do formulário
- T022, T023, T024 em paralelo na fase de polimento

### Exemplo de paralelização — US1

```text
# Após T013 (coordenador) definido, pode avançar UI em T014 enquanto se refinam strings em T015:
Tarefa: "MainSettingsForm em src/MouseScrollFixer/UI/MainSettingsForm.cs"
Tarefa: "Strings.pt-BR em src/MouseScrollFixer/UI/Resources/Strings.pt-BR.resx"
```

---

## Estratégia de implementação

### MVP primeiro (apenas US1)

1. Concluir Fase 1 e Fase 2  
2. Concluir Fase 3 (US1)  
3. **Parar e validar** com `quickstart.md` e cenários P1  
4. Opcional: demonstração / release interna

### Entrega incremental

1. Fundações → US1 → validar  
2. US2 → validar alternância e persistência  
3. US3 → validar encerramento e reinício  
4. Polimento (RF-007, testes de validação, checklist)

### Grafo de conclusão das histórias (ordem sugerida)

```text
Fase 1 ──► Fase 2 ──► US1 (P1) ──► US2 (P2) ──► US3 (P3) ──► Polimento
```

---

## Notas

- Tarefas **[P]** assumem ficheiros ou responsabilidades distintas para evitar conflitos de merge
- Cada história deve permanecer **testável de forma independente** onde a especificação assim o indica
- O contrato JSON em `specs/001-mouse-scroll-fix/contracts/app-config.schema.json` é a referência de validação; manter alinhamento com `AppConfigValidator` e testes em T023
