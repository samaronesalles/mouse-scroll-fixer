# Plano de implementação: correção do comportamento do scroll do mouse

**Branch**: `001-mouse-scroll-fix` | **Data**: 30/03/2026 | **Especificação**: [spec.md](./spec.md)

**Entrada**: Especificação da feature em `specs/001-mouse-scroll-fix/spec.md`

**Nota**: Preenchido pelo comando `/speckit.plan`. O fluxo de execução está descrito em `.specify/templates/plan-template.md`.

## Resumo

O MVP entrega um utilitário **Windows 11** que **normaliza o scroll vertical** (roda e touchpad, mesma regra quando o mecanismo permitir) **apenas** para entradas na **lista de inclusão** configurável pelo utilizador, com **ativação persistida**, **interface e textos em pt-BR**, **sem telemetria** e **aviso** em caso de conflito com outro software de entrada/scroll — **sem precedência automática**.

O produto garante **instância única** por utilizador na sessão (RF-011): segunda execução **ativa** a instância existente, **restaura** a janela principal, **primeiro plano**, **visível**, e abre a **aba de configurações**. Em **todo** arranque há **aviso observável** de que o programa iniciou e está na bandeja (RF-012), em pt-BR — detalhes de mecanismo em `research.md`.

Abordagem técnica (consolidada em `research.md`): aplicação **desktop** em **C# / .NET 8** (`net8.0-windows`), **WinForms** para bandeja e janelas, **hook de baixo nível** (`WH_MOUSE_LL`) e APIs Win32 (P/Invoke) para resolução de janela e reenvio de mensagens de scroll vertical, conforme decisões em `docs/adr.md`. Persistência local em **ficheiro JSON** sob perfil do utilizador; **instalador** opcional com checkbox que define a **primeira** preferência de ativação.

## Contexto técnico

**Linguagem / versão**: C# 12 / .NET 8 (net8.0-windows)  
**Dependências principais**: WinForms, P/Invoke (user32/kernel32), System.Text.Json  
**Armazenamento**: ficheiro JSON local no perfil do utilizador  
**Testes**: xUnit ou MSTest para lógica pura (validação de configuração, regras de inclusão); testes manuais com roteiro em ambiente Windows 11 de referência; testes de integração Win32 apenas onde a stack permitir de forma estável (sem obrigatoriedade de automação completa no MVP).  
**Plataforma alvo**: Windows 11 **x64** apenas no MVP; build mínimo do SO fixado no release (ex.: NT 10.0 build ≥ 22000); **Windows 10** fora de escopo (RF-008).  
**Tipo de projeto**: desktop-app (bandeja + processo em segundo plano quando ativo).  
**Metas de desempenho**: callback do hook **não** deve bloquear (trabalho pesado fora do hook); latência percebida do scroll sem acumulação visível nos cenários de teste; consumo de memória modesto (utilitário residente).  
**Restrições**: impacto mínimo quando inativo ou após saída limpa; sem telemetria nem análise de uso remota (RF-010); scroll **horizontal** fora do MVP; lista de inclusão com **teto** e validação de entradas; elevação apenas quando documentado (ex.: janelas elevadas).  
**Escala / âmbito**: um utilizador por máquina; dezenas de entradas na lista (teto definido em `data-model.md`); sem multi-tenant nem serviços em rede no MVP.

## Verificação da constituição

*GATE: deve passar antes da Fase 0 (pesquisa). Reavaliar após a Fase 1 (desenho).*

Alinhamento com `.specify/memory/constitution.md`:

| Critério | Estado |
|----------|--------|
| **SDD** | Especificação em `spec.md`; este `plan.md`; `tasks.md` será produzido por `/speckit.tasks` — sem saltar a cadeia obrigatória. |
| **Escopo** | Foco no scroll vertical no Windows com lista de inclusão e impacto mínimo; instância única e feedback de arranque conforme especificação; sem expansão além do definido. |
| **Windows / impacto** | Hooks globais, mutex/IPC de segunda instância, privilégios e compatibilidade documentados em `research.md` e riscos em `quickstart.md` / matriz de testes. |
| **Verificação** | Critérios de aceite na especificação + artefatos de contrato e roteiro em `quickstart.md`. |
| **pt-BR** | Artefatos Spec Kit em português brasileiro; código pode seguir convenções em inglês. |

**Desvio de stack (constituição vs ADR)**: a constituição orienta **Delphi**; o repositório possui **ADR-001** a favor de **C# / .NET**. Este plano **consolida** a implementação em **C#** como desvio **explícito** e justificado (ver secção *Rastreio de complexidade* e `research.md`).

### Reavaliação pós-desenho (Fase 1)

- Modelo de dados e contratos JSON cobrem RF-001, RF-002 e casos extremos de preferência corrompida.
- RF-011 e RF-012 são cobertos por decisões em `research.md` e roteiro em `quickstart.md` (comportamento observável, não campos adicionais obrigatórios no JSON).
- Contratos permitem testes de validação sem UI para configuração persistida.
- Nenhum requisito novo além da especificação foi introduzido sem registo.

## Estrutura do projeto

### Documentação (esta feature)

```text
specs/001-mouse-scroll-fix/
├── plan.md              # Este ficheiro (/speckit.plan)
├── research.md          # Fase 0
├── data-model.md        # Fase 1
├── quickstart.md        # Fase 1
├── contracts/           # Fase 1 (schemas e contratos)
└── tasks.md             # Gerado por /speckit.tasks (lista de implementação)
```

### Código-fonte (raiz do repositório — proposta)

```text
src/
└── MouseScrollFixer/
    ├── Program.cs
    ├── App/
    │   ├── TrayApplication.cs          # arranque, NotifyIcon, RF-012
    │   ├── ScrollFixerSession.cs       # ciclo de vida do hook
    │   └── ApplicationExitPersistence.cs  # persistência na saída (alinhado a tasks.md)
    ├── Core/
    │   ├── Configuration/
    │   ├── ConflictDetection/
    │   │   └── ConflictDetector.cs     # RF-007
    │   └── ScrollNormalization/
    ├── Hooks/
    │   └── LowLevelMouseHook.cs
    ├── Native/
    │   └── Win32/
    ├── SingleInstance/                 # mutex / IPC segundo arranque (RF-011)
    └── UI/
        ├── MainSettingsForm.cs         # aba de configurações ao segundo arranque
        └── Resources/                # strings pt-BR

tests/
└── MouseScrollFixer.Tests/
    ├── Configuration/
    └── Core/
```

**Decisão de estrutura**: projeto **único** de aplicação com pastas por responsabilidade (nativo, hooks, núcleo, UI, instância única), alinhado aos ADRs existentes e ao MVP sem camadas desnecessárias. A árvore concretiza-se na primeira tarefa de implementação.

## Rastreio de complexidade

> Preenchido porque há desvio em relação à orientação padrão da constituição (stack).

| Desvio | Porque é necessário | Alternativa mais simples rejeitada porque |
|--------|---------------------|----------------------------------------|
| Stack **C# / .NET** em vez de **Delphi** (orientação da constituição) | **ADR-001** já fixa C# no repositório; documentação e decisões de API (hooks Win32) estão nesse eixo; evita duas pilhas concorrentes sem decisão de produto. | **Só Delphi** exigiria sobrepor o ADR e reescrever premissas documentadas; **só documentação** não entrega executável coerente com o ADR vigente. |
