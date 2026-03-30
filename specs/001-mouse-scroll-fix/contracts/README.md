# Contratos da interface — 001-mouse-scroll-fix

Este diretório define **contratos estáveis** entre componentes e para **testes**:

| Artefato | Descrição |
|----------|-----------|
| [app-config.schema.json](./app-config.schema.json) | Schema JSON Schema do ficheiro de configuração persistido (lista de inclusão, ativação, perfil de comportamento). |

**Superfícies adicionais do MVP**:

- **UI (WinForms)**: não há contrato HTTP; a “API” pública para verificação é o **ficheiro de configuração** e o comportamento observável do scroll.
- **Hooks Win32**: implementação interna; requisitos capturados na especificação e no roteiro de testes, não como OpenAPI.

Versão do schema: alinhada ao campo `schemaVersion` no documento persistido.
