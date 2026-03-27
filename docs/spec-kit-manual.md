# Manual de uso do Spec Kit (Spec-Driven Development)

Este documento explica **o que é** o Spec Kit, **como instalar**, **como inicializar um repositório** e **como seguir o ciclo de desenvolvimento orientado por especificação (SDD)** de ponta a ponta. O conteúdo é **genérico**: serve para qualquer projeto e stack, desde que você use um assistente de código compatível (por exemplo Cursor, Claude Code, Copilot, etc.).

---

## 1. O que é Spec-Driven Development (SDD)?

No desenvolvimento tradicional, o código costuma ser o centro: a especificação é um rascunho que se descarta ou ignora depois. No **SDD**, a especificação passa a ser o eixo:

- **Intenção antes de implementação**: define-se *o quê* e *por quê* antes de fixar *como* em detalhe técnico.
- **Refinamento em etapas**: em vez de um único prompt gigante que gera código, o processo usa **fases** (constituição, especificação, plano, tarefas, implementação) com artefatos revisáveis.
- **Alinhamento com agentes de IA**: os comandos `/speckit.*` guiam o assistente a produzir e manter documentos e tarefas **rastreáveis**, reduzindo improviso e “vibe coding”.

O SDD não substitui boas práticas de engenharia (testes, revisão, CI); ele **estrutura** o trabalho com a IA para que decisões e requisitos fiquem explícitos.

---

## 2. O que é o Spec Kit?

O **Spec Kit** é um toolkit open source que oferece:

- **Specify CLI** (`specify`): instalação e inicialização de projetos com templates, scripts e integração com agentes.
- **Comandos de agente** (normalmente como *slash commands*): `/speckit.constitution`, `/speckit.specify`, `/speckit.plan`, `/speckit.tasks`, `/speckit.implement`, e opcionalmente `/speckit.clarify`, `/speckit.analyze`, `/speckit.checklist`.

Repositório oficial de referência: [github/spec-kit](https://github.com/github/spec-kit) (há também forks e espelhos; o fluxo de uso é o mesmo quando baseado nesse ecossistema).

---

## 3. Pré-requisitos

Antes de começar, confira no seu ambiente:

| Requisito | Motivo |
|-----------|--------|
| **Git** | O fluxo padrão usa branches e pastas de feature; o CLI pode inicializar repositório. |
| **Python 3.11+** | O Specify CLI é distribuído como ferramenta Python. |
| **uv** (recomendado) | Gerenciador rápido para instalar e atualizar o `specify-cli` sem dor de cabeça com venv global. Veja [§3.1](#31-o-que-é-o-uv-e-como-instalar). |
| **Assistente de código compatível** | Para usar os comandos `/speckit.*` dentro do IDE ou CLI configurado pelo Spec Kit. |

### 3.1. O que é o uv e como instalar

**O que é:** [uv](https://github.com/astral-sh/uv) é um **instalador e gerenciador de projetos Python** mantido pela Astral (a mesma equipe do Ruff). Em uma única ferramenta rápida (binário em Rust) ele cobre o que, na prática, costuma exigir `pip`, ambientes virtuais e utilitários extras. Por isso o manual do Spec Kit usa `uv` para:

- **`uv tool install`** — instalar o CLI `specify` de forma isolada (ferramentas no estilo “global”, sem misturar dependências num `venv` “genérico” confuso).
- **`uvx`** — executar o `specify` (ou outra ferramenta) **sem** instalação permanente, útil para testar.

**Como instalar o uv:** use **um** dos métodos abaixo (o instalador oficial costuma ser o mais simples). Lista completa de opções (Docker, Cargo, releases no GitHub, etc.): [Instalação do uv](https://docs.astral.sh/uv/getting-started/installation/).

**Windows — instalador oficial (PowerShell)** (pode pedir ajuste de *execution policy*; é o fluxo documentado pela Astral):

```powershell
powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"
```

**Windows — WinGet:**

```powershell
winget install --id=astral-sh.uv -e
```

**Windows — Scoop:**

```powershell
scoop install main/uv
```

**macOS / Linux — instalador oficial:**

```bash
curl -LsSf https://astral.sh/uv/install.sh | sh
```

**macOS — Homebrew:**

```bash
brew install uv
```

**Qualquer sistema — PyPI** (se já tiver Python; preferível isolar com **pipx**):

```bash
pipx install uv
```

Após instalar, **feche e reabra o terminal** (ou carregue de novo o `PATH`) e confira:

```bash
uv --version
```

Se o instalador standalone foi usado, atualizações posteriores podem ser feitas com:

```bash
uv self update
```

*(Com instalação só por gerenciador de pacotes — WinGet, Scoop, Homebrew, pip, etc. — use o comando de *upgrade* desse gerenciador em vez de `uv self update`.)*

**Sistemas operacionais**: Linux, macOS e Windows são suportados. No Windows, prefira **PowerShell** quando a documentação oferecer scripts `--script ps`.

---

## 4. Instalação do Specify CLI

### 4.1. Instalação persistente (recomendada)

Instala o comando `specify` no seu PATH; você reutiliza em todos os projetos.

1. Instale o **uv** (se ainda não tiver); veja [§3.1](#31-o-que-é-o-uv-e-como-instalar).
2. Instale o CLI a partir do repositório do Spec Kit. Para **estabilidade**, fixe uma **tag de release** (substitua `vX.Y.Z` pela versão desejada; consulte *Releases* no repositório):

```bash
uv tool install specify-cli --from git+https://github.com/github/spec-kit.git@vX.Y.Z
```

Para instalar a **última linha principal** (pode incluir mudanças ainda não lançadas como release):

```bash
uv tool install specify-cli --from git+https://github.com/github/spec-kit.git
```

3. Verifique:

```bash
specify --help
```

### 4.2. Uso pontual (sem instalar de forma permanente)

Útil para experimentar ou em ambientes efêmeros:

```bash
uvx --from git+https://github.com/github/spec-kit.git@vX.Y.Z specify init --help
```

(Ajuste a tag conforme necessário.)

### 4.3. Atualizar o Specify CLI

Quando usar instalação persistente, para **forçar** reinstalação/atualização:

```bash
uv tool install specify-cli --force --from git+https://github.com/github/spec-kit.git@vX.Y.Z
```

### 4.4. Ambientes restritos (air-gapped / sem PyPI)

O repositório oficial documenta fluxos alternativos (por exemplo, empacotamento de wheels). Consulte o guia de instalação empresarial no README do projeto, se aplicável.

---

## 5. Inicializar o Spec Kit em um projeto

Você pode criar um **novo diretório** ou adicionar o Spec Kit a um **repositório existente**.

### 5.1. Novo projeto (nova pasta)

```bash
specify init NOME_DO_PROJETO --ai NOME_DO_AGENTE
```

Substitua `NOME_DO_AGENTE` pelo identificador suportado (ex.: `cursor-agent`, `claude`, `copilot`, `gemini`, `codex`, etc.). A lista completa evolui; use `specify init --help` e a documentação do Spec Kit para opções atuais.

### 5.2. Repositório atual (diretório já existente)

Na raiz do repositório:

```bash
specify init . --ai NOME_DO_AGENTE
```

ou:

```bash
specify init --here --ai NOME_DO_AGENTE
```

Se o diretório **já contém arquivos** e o CLI pedir confirmação, você pode forçar a mesclagem (atenção: pode sobrescrever/ mesclar artefatos):

```bash
specify init --here --force --ai NOME_DO_AGENTE
```

### 5.3. Windows e PowerShell

Para gerar scripts compatíveis com **PowerShell**:

```bash
specify init --here --ai NOME_DO_AGENTE --script ps
```

### 5.4. Outras opções úteis

| Opção | Efeito |
|-------|--------|
| `--no-git` | Não inicializa repositório Git. |
| `--ignore-agent-tools` | Pula checagens de ferramentas do agente (útil se você só quer os templates). |
| `--ai-skills` | Instala templates como *skills* do agente (combinar com `--ai`; ex.: Codex em modo skills). |
| `--ai-commands-dir` | Obrigatório com `--ai generic` para apontar pasta de comandos customizados. |
| `--branch-numbering` | `sequential` (padrão) ou `timestamp` para nomes de branch de feature. |

### 5.5. Verificar o ambiente

Após a instalação:

```bash
specify check
```

Esse comando costuma validar **git** e presença de CLIs de agentes configurados, conforme o Spec Kit.

---

## 6. Onde aparecem os artefatos?

Após `specify init`, o repositório ganha estrutura típica (nomes exatos podem variar levemente com a versão), incluindo:

- **Memória / constituição**: princípios globais do projeto.
- **Templates**: modelos de spec, plano e tarefas.
- **Scripts**: automação (criação de feature, pré-requisitos, etc.).
- **Pastas de especificação**: frequentemente algo como `specs/<id-feature>/` com `spec.md`, `plan.md`, `tasks.md`, e eventualmente contratos, pesquisa, quickstart.

O importante é: você passa a ter **fontes de verdade** para requisitos e trabalho, não só código.

---

## 7. O ciclo SDD (passo a passo)

Execute as fases **nessa ordem lógica**. Pular etapas aumenta retrabalho e divergência entre código e intenção.

### Fase A — Constituição (`/speckit.constitution`)

**Objetivo**: definir **princípios e regras de governança** do projeto (qualidade, testes, UX, performance, segurança, como tomar decisões técnicas).

**Como usar**: no assistente integrado, invoque o comando e descreva o que a constituição deve cobrir.

**Resultado típico**: arquivo de constituição sob algo como `.specify/memory/constitution.md`.

**Por que importa**: tudo o que vem depois (spec, plano, código) deve ser **compatível** com esses princípios.

---

### Fase B — Especificação (`/speckit.specify`)

**Objetivo**: capturar **o quê** o produto ou feature deve fazer e **por quê**, em linguagem de negócio e cenários de usuário.

**Boas práticas**:

- Foque em **comportamento observável**, fluxos, dados de entrada/saída, restrições, fora de escopo.
- Evite, nesta fase, amarrar demais a **stack** (isso vem no plano).

**Resultado típico**: `spec.md` dentro da pasta da feature (ex.: `specs/001-.../spec.md`), muitas vezes com histórias de usuário e requisitos.

---

### Fase C — Clareza (`/speckit.clarify`) — fortemente recomendada

**Objetivo**: fechar **lacunas, ambiguidades e edge cases** antes do plano técnico.

**Quando pular**: apenas em spikes ou protótipos descartáveis; documente explicitamente que foi um **atalho intencional**.

**Resultado**: esclarecimentos registrados (por exemplo, seção “Clarifications” ou equivalente no template).

---

### Fase D — Plano técnico (`/speckit.plan`)

**Objetivo**: traduzir a spec em **arquitetura, stack, módulos, integrações, dados e estratégia de entrega**.

**Boas práticas**:

- Inclua **decisões e alternativas** quando houver trade-offs.
- Alinhe com a constituição (performance, segurança, testes).
- Se algo mudar depois, **atualize o plano** em vez de só “ir codando”.

**Resultado típico**: `plan.md` e documentos auxiliares (modelo de dados, contratos de API, pesquisa, quickstart).

---

### Fase E — Auditoria opcional do plano (recomendada na prática)

Antes de gerar tarefas, vale pedir ao assistente para **revisar o plano** com olhar de implementação: dependências esquecidas, ordem de trabalho, riscos. Isso não é um comando único obrigatório do núcleo, mas é **boa disciplina**.

---

### Fase F — Tarefas (`/speckit.tasks`)

**Objetivo**: decompor o plano em **tarefas ordenadas**, com dependências e, quando aplicável, paralelização marcada (ex.: `[P]`).

**Resultado típico**: `tasks.md` com caminhos de arquivos, checkpoints e, se aplicável, ordem test-first.

---

### Fase G — Consistência (`/speckit.analyze`) — opcional e estratégica

**Objetivo**: **análise de consistência** entre artefatos (spec, plano, tarefas) antes de implementar.

**Quando usar**: após `/speckit.tasks` e **antes** de `/speckit.implement`, para reduzir retrabalho.

---

### Fase H — Implementação (`/speckit.implement`)

**Objetivo**: executar as tarefas na ordem correta, seguindo TDD se o `tasks.md` assim exigir.

**Responsabilidade sua**: ter as **ferramentas da stack** instaladas (`dotnet`, `npm`, `cargo`, etc.), pois o agente pode executar comandos locais.

**Depois de implementar**: testar manualmente ou automatizado conforme o projeto; erros de runtime (navegador, desktop, serviço) devem ser levados de volta ao assistente com logs.

---

### Fase I — Checklist de qualidade (`/speckit.checklist`) — opcional

**Objetivo**: gerar listas de verificação para **completude e clareza** dos requisitos (metaforicamente “testes para o inglês” da spec).

---

## 8. Comandos principais (referência rápida)

| Comando | Função |
|---------|--------|
| `/speckit.constitution` | Criar/atualizar princípios do projeto |
| `/speckit.specify` | Definir requisitos e histórias (o quê/por quê) |
| `/speckit.clarify` | Esclarecer lacunas antes do plano |
| `/speckit.plan` | Plano técnico e arquitetura |
| `/speckit.tasks` | Quebrar em tarefas implementáveis |
| `/speckit.analyze` | Checar consistência entre artefatos |
| `/speckit.implement` | Executar o plano via tarefas |
| `/speckit.checklist` | Checklists de qualidade da especificação |

*(Nomes exatos podem depender do agente; em alguns fluxos, skills usam prefixo diferente, ex.: `$speckit-*` no Codex em modo skills.)*

---

## 9. Variável de ambiente `SPECIFY_FEATURE`

Quando **não** há Git ou o fluxo de detecção de feature falha, você pode forçar o diretório da feature:

- Defina `SPECIFY_FEATURE` para o nome da pasta da feature (ex.: `001-nome-da-feature`) **antes** de `/speckit.plan` e comandos subsequentes.

Consulte a documentação atual do Spec Kit para detalhes e limitações.

---

## 10. Extensões e presets (visão geral)

O Spec Kit permite estender o comportamento sem fork do núcleo:

- **Extensões**: novos comandos ou integrações (Jira, revisão pós-implementação, etc.).
- **Presets**: personalizam **templates e terminologia** (por exemplo, formatos obrigatórios de spec em uma organização).

Resolução de templates costuma seguir prioridade: overrides locais → presets → extensões → núcleo. Comandos de extensão/preset podem ser aplicados na instalação. Para detalhes, veja o README oficial sobre *extensions* e *presets*.

---

## 11. Como manter o projeto “100% SDD” na prática

Disciplina sugerida:

1. **Toda mudança de requisito** atualiza primeiro `spec.md` (e clarificações), depois o código.
2. **Toda mudança arquitetural** atualiza `plan.md` (e ADRs externos, se o time usar).
3. **Toda entrega** deve ser rastreável a itens em `tasks.md`.
4. **Evite** implementar funcionalidades que não existam na spec/plano/tarefas — isso é a principal fonte de “drift”.
5. Use branches de feature conforme os scripts do Spec Kit, para isolar trabalho e revisão.

---

## 12. Problemas comuns

| Sintoma | O que verificar |
|---------|-------------------|
| Comando `/speckit.*` não aparece | `specify init` foi executado com o agente correto? Pasta do projeto aberta na raiz? |
| `specify check` reclama de ferramentas | Instalar o CLI do agente ou usar `--ignore-agent-tools` na init (se aceitável). |
| Conflito ao init em repo não vazio | Usar `--force` com consciência; revisar diffs antes de commitar. |
| Plano desalinhado da spec | Rodar `/speckit.clarify` e `/speckit.analyze` antes de implementar. |

---

## 13. Leitura adicional

- **Repositório e README**: [github/spec-kit](https://github.com/github/spec-kit)
- **Metodologia** (documentos “spec-driven” ligados ao projeto)
- **Specify CLI**: opções atualizadas em `specify --help` e na documentação do release que você fixar

---

## 14. Resumo em uma frase

**Spec Kit + SDD** = instalar o `specify`, inicializar o repositório com o agente certo, e repetir o ciclo **constituição → especificação → (clarificar) → plano → tarefas → (analisar) → implementar**, mantendo os artefatos como fonte de verdade junto ao código.
