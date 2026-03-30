# Versionamento (SemVer)

A versão da aplicação segue [Semantic Versioning 2.0.0](https://semver.org/lang/pt-BR/): **MAJOR.MINOR.PATCH**, com sufixo opcional para pré-lançamentos (ex.: `1.2.0-beta.1`).

## Onde definir a versão

O número **não** é incrementado automaticamente. Quando quiser publicar uma nova versão, edite manualmente o ficheiro na raiz do repositório:

| Ficheiro | Função |
|----------|--------|
| `Version.props` | Contém `<Version>…</Version>` consumido pelo MSBuild para todos os projetos da solução. |

Após alterar, compile de novo; o executável e os metadados do assembly refletem o valor (incluindo `AssemblyInformationalVersion`).

O repositório define `IncludeSourceRevisionInInformationalVersion` como `false` em `Version.props`, para que a cadeia mostrada na interface corresponda ao SemVer escrito (sem sufixo automático de hash de commit).

## Onde a versão aparece

- Título da janela de configurações e texto na parte inferior do separador **Configurações**
- Tooltip do ícone na área de notificação (bandeja)

## Relação com `schemaVersion` no JSON

O campo `schemaVersion` em `app-config.json` descreve o **formato** do ficheiro de configuração, não a versão da aplicação. São conceitos independentes.
