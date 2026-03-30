# Build e Distribuição

## 📦 Pré-requisitos

* .NET SDK instalado

---

## 🔢 Versão da aplicação (SemVer)

A versão é definida **manualmente** em `Version.props` na raiz do repositório (`<Version>1.0.0</Version>`). Consulte [versioning.md](versioning.md) para o fluxo completo.

---

## ⚙️ Build local

Na raiz do repositório:

```bash
dotnet build MouseScrollFixer.sln -c Release
```

Saídas centralizadas em `publish\` (ver regra do repositório sobre build).

---

## 🚀 Publicação

Exemplo com pasta de saída fixa:

```bash
dotnet publish src\MouseScrollFixer\MouseScrollFixer.csproj -c Release -o publish\app
```

Para pacote autocontido (runtime incluído), por exemplo:

```bash
dotnet publish src\MouseScrollFixer\MouseScrollFixer.csproj -c Release -r win-x64 --self-contained true -o publish\app
```

---

## 📁 Saída

Após o build, o executável principal fica em:

```text
publish\out\MouseScrollFixer\Release\net8.0-windows\MouseScrollFixer.exe
```

(O caminho exato segue `Directory.Build.props` na raiz.)

---

## 📦 Arquivo final

* Executável `.exe`
* Com `--self-contained`, pacote independente de runtime instalado no sistema

---

## ⚙️ Opções adicionais

### Single file:

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

---

## 🔐 Permissões

* Para suporte completo:

  * Executar como administrador

---

## 📥 Distribuição

* ZIP com executável
* Ou instalador (futuro)

---

## ⚠️ Observações

* Hooks podem ser sinalizados por antivírus
* Recomendado assinar código futuramente

---
