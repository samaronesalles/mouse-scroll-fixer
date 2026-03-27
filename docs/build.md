# Build e Distribuição

## 📦 Pré-requisitos

* .NET SDK instalado

---

## ⚙️ Build local

```bash
dotnet build
```

---

## 🚀 Publicação

```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

---

## 📁 Saída

```
/bin/Release/net8.0-windows/win-x64/publish/
```

---

## 📦 Arquivo final

* Executável `.exe`
* Independente de runtime

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
