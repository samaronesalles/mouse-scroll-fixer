# Contratos da interface — 001-mouse-scroll-fix

Este diretório define **contratos estáveis** entre componentes e para **testes**:

| Artefato | Descrição |
|----------|-----------|
| [app-config.schema.json](./app-config.schema.json) | Schema JSON do ficheiro persistido (lista de inclusão, ativação e perfil de comportamento incluindo fallback legado por `WM_VSCROLL`). |

**Superfícies adicionais do MVP**:

- **UI (WinForms)**: não há contrato HTTP; a “API” pública para verificação é o **ficheiro de configuração** e o comportamento observável do scroll.
- **Hooks Win32**: implementação interna; requisitos capturados na especificação e no roteiro de testes, não como OpenAPI.
- **Instância única / segundo arranque (RF-011)**: contrato **comportamental** — o utilizador observa uma única instância; segundo processo **ativa** a existente com janela visível e **aba de configurações**. O mecanismo IPC (mensagem registada, pipe, etc.) é **interno** e pode evoluir sem alterar o contrato de utilizador.
- **Aviso de arranque (RF-012)**: contrato **comportamental** — em todo arranque há feedback observável em pt-BR conforme especificação; texto exacto pode residir em recursos da UI, não no JSON de configuração.

Versão do schema: alinhada ao campo `schemaVersion` no documento persistido.
