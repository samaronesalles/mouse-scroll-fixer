# Build e distribuição

## Pré-requisitos

- [.NET SDK 8](https://dotnet.microsoft.com/download) (para compilar no repositório)
- [Windows Desktop Runtime 8 (x64)](https://dotnet.microsoft.com/download/dotnet/8.0) na máquina de destino (para executar o `.exe` publicado)

## Versão (SemVer)

A versão do executável vem de `Version.props` na raiz (`<Version>…</Version>`). Fluxo completo: [versioning.md](versioning.md).

## Build local

Na raiz do repositório:

```bash
dotnet build MouseScrollFixer.sln -c Release
dotnet test MouseScrollFixer.sln -c Release
```

Saídas em `publish\` (sem `bin\`/`obj\` dentro de `src\` ou `tests\`); detalhes em `.cursor/rules/build-output-centralizado.mdc`.

## Publicação padrão (single-file, framework-dependent)

O projeto `MouseScrollFixer` define **publicação em ficheiro único** (`PublishSingleFile`), **dependente de runtime instalado** (`SelfContained=false`) e **RID** `win-x64`.

Na raiz:

```bash
dotnet publish src\MouseScrollFixer\MouseScrollFixer.csproj -c Release -o publish\app
```

O artefato principal é **`publish\app\MouseScrollFixer.exe`** (tipicamente **< 1 MB**). Pode distribuir apenas esse `.exe`, desde que a máquina de destino tenha o **.NET 8 Desktop Runtime (x64)** instalado.

Se o runtime não estiver instalado, o Windows mostrará a mensagem padrão de ausência de runtime .NET ao abrir o executável.

### Ficheiros extra na pasta de publicação

- **`MouseScrollFixer.pdb`**: símbolos de depuração. Para distribuição ao público, normalmente pode ficar fora do pacote final.
- **Pastas de idioma satélite**: a app limita satélites para `pt-BR` no projeto; dependendo do ambiente de build, podem existir ficheiros auxiliares além do `.exe`.

Para distribuição portátil mínima, o ficheiro essencial continua a ser **`MouseScrollFixer.exe`** (com .NET Desktop Runtime instalado no destino).

### Propriedades relevantes no `.csproj`

| Propriedade | Função |
|-------------|--------|
| `PublishSingleFile` | Gera um único executável principal. |
| `SelfContained` | Define `false`: exige runtime .NET já instalado no sistema. |
| `RuntimeIdentifier` (`win-x64`) | Alvo x64 no Windows. |
| `SatelliteResourceLanguages` (`pt-BR`) | Limita satélites de idioma do runtime ao português do Brasil, alinhado à UI da app. |

## Saída do `dotnet build` (testes manuais)

Após compilar em Release, o executável da app fica em:

```text
publish\out\MouseScrollFixer\Release\net8.0-windows\win-x64\MouseScrollFixer.exe
```

Os testes unitários compilam para `publish\out\MouseScrollFixer.Tests\Release\net8.0-windows\` (sem RID no projeto de testes).

## Distribuição

- **Portátil**: enviar o `MouseScrollFixer.exe` (ou um ZIP só com esse ficheiro) + instrução de pré-requisito do `.NET 8 Desktop Runtime`.
- **Instalador**: opcional; ver [installer/README.md](../installer/README.md).

## Observações

- A app está orientada a **Windows 11** (ver mensagem em tempo de execução se o SO não for suportado).
- Hooks de baixo nível podem ser sinalizados por antivírus; assinatura de código é recomendável para distribuição alargada.
