# MouseScrollFixer

Utilitário desktop para Windows 11 que corrige o scroll em aplicações legadas, com controle por whitelist e operação em bandeja.

## Objetivo do projeto

O `MouseScrollFixer` existe para resolver um problema comum em softwares antigos: rolagem inconsistente ou inexistente com mouse/touchpad.

Objetivos funcionais centrais:
- Permitir usar scroll do mouse em apps legadas com comportamento previsível.
- Atuar em **scroll vertical apenas** no MVP (`WM_MOUSEWHEEL`).
- Permitir que o fix funcione **somente em aplicações selecionadas** nas configurações (`inclusionList` por caminho do executável).
- Permitir ligar/desligar rapidamente o fix pela UI ou bandeja.

## Como funciona (resumo técnico)

1. A app instala um hook global `WH_MOUSE_LL`.
2. Em evento `WM_MOUSEWHEEL`, resolve o `HWND` sob o cursor e o executável dono da janela.
3. Se o executável estiver na lista de inclusão e o fix estiver ativo:
   - reenvia scroll ao alvo via `PostMessageW` (`WM_MOUSEWHEEL`), ou
   - usa fallback legado por `WM_VSCROLL` quando habilitado.
4. Se não estiver na whitelist, não interfere no comportamento.

> Escopo atual: `WM_MOUSEHWHEEL` (horizontal) está fora do MVP.

## Stack de desenvolvimento

- **Linguagem:** C# 12
- **Plataforma:** .NET 8 (`net8.0-windows`)
- **UI:** WinForms
- **Integração nativa:** P/Invoke (`user32`, `kernel32`, `ntdll`)
- **Persistência:** JSON local com `System.Text.Json`
- **Testes:** xUnit (foco atual em validação de configuração)

## Estado do produto

- Projeto em MVP funcional, com foco em estabilidade para apps legadas.
- Suporte oficial atual: **Windows 11**.
- Distribuição principal: executável single-file framework-dependent (`win-x64`).

## Configuração local

A configuração é salva em:

`%LocalAppData%\MouseScrollFixer\app-config.json`

Com backup em:

`%LocalAppData%\MouseScrollFixer\app-config.bak.json`

Campos principais:
- `activation.enabled`
- `inclusionList[]`
- `behavior.invertVertical`
- `behavior.linesPerNotchApprox`
- `behavior.touchpadSameAsWheel`
- `behavior.useVScrollFallback`

## Build, teste e publish

Na raiz do repositório:

```powershell
dotnet build MouseScrollFixer.sln -c Release
dotnet test MouseScrollFixer.sln -c Release
```

Publicação padrão:

```powershell
dotnet publish src\MouseScrollFixer\MouseScrollFixer.csproj -c Release -o publish\app
```

Artefatos importantes:
- Build da app: `publish\out\MouseScrollFixer\Release\net8.0-windows\win-x64\MouseScrollFixer.exe`
- Publish da app: `publish\app\MouseScrollFixer.exe`

> Requisito no destino: .NET 8 Desktop Runtime (x64).

## Estrutura do repositório

```text
src/    # aplicação
tests/  # testes automatizados
docs/   # documentação de arquitetura, build e contexto
specs/  # artefatos SDD (spec/plan/tasks/contratos/checklists)
```

## Projeto “vibe coded” (vide coded)

Este projeto foi construído em fluxo iterativo com IA (vibe coded), mas com disciplina de engenharia:
- requisitos explícitos;
- contratos de dados;
- validação por testes e checklists;
- documentação versionada junto do código.

## Regra do time: tudo via SDD

Neste repositório, **toda mudança deve seguir SDD (Spec-Driven Development)**.

Fluxo obrigatório:
1. `/speckit.constitution` (quando aplicável)
2. `/speckit.specify`
3. `/speckit.clarify` (recomendado)
4. `/speckit.plan`
5. `/speckit.tasks`
6. `/speckit.analyze` (recomendado)
7. `/speckit.implement`

Sem spec/plano/tarefas, a implementação é considerada fora do processo oficial.

## Uso do toolkit Spec Kit

O repositório usa a toolkit **Spec Kit** para organizar SDD.

Referências internas:
- Manual: `docs/spec-kit-manual.md`
- Artefatos da feature atual: `specs/001-mouse-scroll-fix/`

Boas práticas:
- manter artefatos Spec Kit em pt-BR;
- atualizar docs quando o comportamento implementado mudar;
- garantir rastreabilidade entre requisito, tarefa e código.

## Documentação complementar

- Arquitetura e decisões: `docs/adr.md`
- Contexto funcional/técnico: `docs/context.md`
- Build e distribuição: `docs/build.md`
- Versionamento SemVer: `docs/versioning.md`
- Instalador (opcional): `installer/README.md`

## Licença

Definir/atualizar conforme a política do projeto.
