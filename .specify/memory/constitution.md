<!--
Sync Impact Report
- Versão: (inicial) → 1.0.0
- Princípios: preenchimento inicial (substitui placeholders do template); sem renomeações prévias.
- Seções adicionadas: Restrições de plataforma e stack; Fluxo de trabalho e qualidade (além do esqueleto do template).
- Seções removidas: nenhuma (template apenas tinha placeholders).
- Templates: .specify/templates/plan-template.md ✅ atualizado (Constitution Check). .specify/templates/spec-template.md ✅ sem alteração necessária. .specify/templates/tasks-template.md ✅ sem alteração necessária.
- TODOs adiados: nenhum.
-->

# Constituição do projeto mouse-scroll-fixer

## Core Principles

### I. Desenvolvimento orientado por especificação (SDD)

Toda entrega relevante DEVE seguir a cadeia **constituição → especificação → (clarificar) → plano → tarefas → implementação**, com artefatos em `specs/` e `.specify/` como fonte de verdade. O código NÃO DEVE antecipar requisitos que não estejam refletidos na especificação e no plano vigentes. Mudanças de comportamento começam pela atualização dos documentos, depois pela implementação.

**Racional**: Reduz deriva entre intenção e código e torna o trabalho com assistentes de IA rastreável e revisável.

### II. Foco no problema do usuário e escopo mínimo

O produto DEVE priorizar corrigir ou melhorar o comportamento do scroll do mouse no Windows de forma previsível, sem absorver escopo não relacionado. Novas capacidades exigem justificativa na especificação (valor para o usuário e relação com o objetivo central).

**Racional**: Evita complexidade acidental e mantém o utilitário fácil de manter e auditar.

### III. Plataforma Windows e impacto no sistema

A solução DEVE ser projetada para **Windows** como alvo principal. Interceptação, hooks ou integrações com o sistema DEVEM usar a menor superfície necessária, privilegiando estabilidade e privilégios mínimos quando aplicável. Efeitos colaterais (desempenho, segurança, compatibilidade) DEVEM ser considerados no plano técnico e, quando possível, mitigados ou documentados.

**Racional**: Ferramentas de baixo nível com entrada inadequada podem degradar a experiência ou o sistema; a constituição exige consciência explícita disso.

### IV. Comportamento observável e verificação

Requisitos DEVEM ser formulados de modo **testável ou verificável** (cenários *Given/When/Then*, critérios manuais claros ou testes automatizados quando a stack permitir). Regressões conhecidas DEVEM ser capturadas como casos de teste ou checklist antes de fechar a feature.

**Racional**: “Funciona na máquina de alguém” não é critério suficiente para um fix de entrada/saída.

### V. Documentação do fluxo Spec Kit em português (pt-BR)

Texto dos artefatos do fluxo Spec Kit (constituição, especificações, planos, tarefas, checklists e extensões do mesmo fluxo) DEVE estar em **português brasileiro**, mantendo identificadores técnicos (paths, APIs, nomes de flags) como no código. O código-fonte pode seguir convenções do projeto em inglês quando for o padrão da stack.

**Racional**: Alinha o repositório ao público e às regras acordadas do time; reduz ambiguidade em revisões.

## Restrições de plataforma e stack

- **Alvo**: Windows (versões mínimas e dependências de runtime DEVEM ser declaradas no plano quando conhecidas).
- **Stack**: O repositório está orientado a **Delphi** e integração nativa Win32 conforme necessário para manipulação de entrada; desvios (outra linguagem ou camada) exigem decisão explícita no `plan.md` e compatibilidade com os princípios I–IV.
- **Distribuição e build**: Artefatos de build e instruções de execução DEVEM ser descritos no plano ou em documentos referenciados (`quickstart.md`, README quando existir), sem suposições implícitas.

## Fluxo de trabalho e qualidade

- **Branches**: Preferir branches de feature alinhadas ao identificador em `specs/` (por exemplo `###-nome`), conforme scripts do Spec Kit.
- **Revisão**: Alterações DEVEM ser checadas contra esta constituição; o autor resume o alinhamento em PR ou descrição de commit quando aplicável.
- **Planos**: A seção **Constitution Check** do `plan.md` DEVE ser preenchida e considerada antes de pesquisa (Fase 0) e revisada após o desenho (Fase 1).
- **Agentes de IA**: Comandos `/speckit.*` e templates em `.specify/` guiam o trabalho; desvios temporários DEVEM ser marcados como débito explícito na especificação ou nas tarefas.

## Governance

Esta constituição prevalece sobre convenções informais do repositório em caso de conflito. Emendas exigem pull request ou alteração revisada equivalente, com atualização da versão e da data **Last Amended**. A versão segue **SemVer** para o documento: **MAJOR** — remoção ou redefinição incompatível de princípio; **MINOR** — novo princípio ou seção material; **PATCH** — clarificações, redação ou correções sem mudança de significado. Revisões periódicas de conformidade DEVEM ocorrer antes de releases significativos ou quando múltiplas features alterarem entrada do sistema.

**Version**: 1.0.0 | **Ratified**: 2025-03-27 | **Last Amended**: 2025-03-27
