# Contratos — 002-startup-admin-options

## `app-config.schema.json`

Schema JSON **compatível com `schemaVersion: 1`**, estendendo o contrato do MVP (`specs/001-mouse-scroll-fix/contracts/app-config.schema.json`) com a secção opcional **`startup`**.

### Campos novos

| Caminho | Tipo | Default |
|---------|------|---------|
| `startup.autoStartWithWindows` | boolean | `false` |
| `startup.runAsAdmin` | boolean | `false` |

### Exemplo mínimo (config legada — válida)

```json
{
  "schemaVersion": 1,
  "activation": { "enabled": false },
  "inclusionList": [],
  "behavior": { "touchpadSameAsWheel": true }
}
```

### Exemplo com startup

```json
{
  "schemaVersion": 1,
  "activation": { "enabled": true },
  "inclusionList": [],
  "behavior": { "touchpadSameAsWheel": true },
  "startup": {
    "autoStartWithWindows": true,
    "runAsAdmin": false
  }
}
```

### Efeitos no sistema (não serializados)

- `autoStartWithWindows: true` → valor `MouseScrollFixer` em `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`.
- `runAsAdmin: true` → gate UAC no arranque do processo (ver `research.md`).

### Sincronização UI ↔ JSON

Conforme regra do repositório `config-app-json-ui-sync.mdc`: ambos os campos DEVEM ter controlos na janela de configurações e persistência via `AppConfigStore.Save`.
