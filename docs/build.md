# Build e distribuição

## Pré-requisitos

- [.NET SDK 8](https://dotnet.microsoft.com/download) (para compilar no repositório)

## Versão (SemVer)

A versão do executável vem de `Version.props` na raiz (`<Version>…</Version>`). Fluxo completo: [versioning.md](versioning.md).

## Build local

Na raiz do repositório:

```bash
dotnet build MouseScrollFixer.sln -c Release
dotnet test MouseScrollFixer.sln -c Release
```

Saídas em `publish\` (sem `bin\`/`obj\` dentro de `src\` ou `tests\`); detalhes em `.cursor/rules/build-output-centralizado.mdc`.

## Publicação padrão (single-file, autocontido)

O projeto `MouseScrollFixer` define **publicação em ficheiro único** (`PublishSingleFile`), **runtime incluído** (`SelfContained`) e **RID** `win-x64`. Não é necessário passar `-r` nem `--self-contained` na linha de comandos.

Na raiz:

```bash
dotnet publish src\MouseScrollFixer\MouseScrollFixer.csproj -c Release -o publish\app
```

O artefacto principal é **`publish\app\MouseScrollFixer.exe`** (~140–160 MB, conforme versão do SDK). Pode **copiar só este `.exe`** para outra pasta ou máquina (Windows x64) e executar: o runtime e as bibliotecas nativas necessárias são extraídos em tempo de execução (por exemplo para uma pasta temporária), sem depender de uma instalação global do .NET.

### Ficheiros extra na pasta de publicação

- **`MouseScrollFixer.pdb`**: símbolos de depuração. Para distribuição ao público costuma **omitir** do pacote (ZIP/instalador).
- **Pastas de idioma** (por exemplo `pt-BR\`, `cs\`, …): podem aparecer junto ao `.exe`; em muitos casos estão vazias ou só são usadas em cenários específicos. Para um pacote mínimo, **basta o `.exe`**; se quiser um ZIP “limpo”, pode incluir apenas o `.exe`.

### Propriedades relevantes no `.csproj`

| Propriedade | Função |
|-------------|--------|
| `PublishSingleFile` | Gera um único executável principal. |
| `SelfContained` | Inclui o runtime .NET no pacote (não exige runtime instalado no sistema). |
| `RuntimeIdentifier` (`win-x64`) | Alvo x64 no Windows. |
| `IncludeNativeLibrariesForSelfExtract` | Embute DLLs nativas no `.exe` e extrai-as ao correr. |
| `IncludeAllContentForSelfExtract` | Inclui conteúdo adicional (por exemplo satélites) no bundle. |
| `SatelliteResourceLanguages` (`pt-BR`) | Limita satélites de idioma do runtime ao português do Brasil, alinhado à UI da app. |

## Saída do `dotnet build` (testes manuais)

Após compilar em Release, o executável da app fica em:

```text
publish\out\MouseScrollFixer\Release\net8.0-windows\win-x64\MouseScrollFixer.exe
```

Os testes unitários compilam para `publish\out\MouseScrollFixer.Tests\Release\net8.0-windows\` (sem RID no projeto de testes).

## Distribuição

- **Portátil**: enviar o `MouseScrollFixer.exe` (ou um ZIP só com esse ficheiro).
- **Instalador**: opcional; ver [installer/README.md](../installer/README.md).

## Observações

- A app está orientada a **Windows 11** (ver mensagem em tempo de execução se o SO não for suportado).
- Hooks de baixo nível podem ser sinalizados por antivírus; assinatura de código é recomendável para distribuição alargada.
