# Quickstart — Opções de arranque e administrador (002)

**Público**: testador ou desenvolvedor no **Windows 11 (x64)**. **Idioma da app**: pt-BR.

## Pré-requisitos

- Windows 11 (mesmo gate do MVP em `Program.cs`).
- Build Release: `dotnet build MouseScrollFixer.sln -c Release`
- Executável: `publish\out\MouseScrollFixer\Release\net8.0-windows\win-x64\MouseScrollFixer.exe`
- Conta com permissão para **aceitar ou negar UAC** (testes de elevação).

## Onde ficam as opções

1. Abrir o utilitário (bandeja).
2. Abrir **Configurações** / janela principal.
3. Secção **Arranque** (ou equivalente): dois checkboxes:
   - **Iniciar automaticamente com o Windows**
   - **Iniciar sempre como administrador**

Alterações persistem em `%LocalAppData%\MouseScrollFixer\app-config.json` na secção `startup`.

## Roteiro P1 — Arranque automático (isolado)

| # | Ação | Resultado esperado |
|---|------|-------------------|
| 1 | Garantir `startup.autoStartWithWindows: false` (ou checkbox desmarcado) | Após reinício/login, app **não** inicia sozinha |
| 2 | Marcar **Iniciar automaticamente com o Windows** | JSON atualizado; registo em `HKCU\...\Run` com caminho do `.exe` |
| 3 | Reiniciar sessão Windows (ou logout/login) | App na bandeja em ≤2 min; aviso de arranque MVP; instância única |
| 4 | Desmarcar opção | Entrada Run removida; após reinício, app **não** auto-inicia |

**Verificação registo (opcional)**:

```powershell
Get-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name 'MouseScrollFixer' -ErrorAction SilentlyContinue
```

## Roteiro P1 — Executar como administrador (isolado)

| # | Ação | Resultado esperado |
|---|------|-------------------|
| 1 | Marcar **Iniciar sempre como administrador** | JSON `startup.runAsAdmin: true` |
| 2 | Fechar app e abrir de novo | Pedido **UAC** |
| 3 | **Aprovar** UAC | Processo elevado (Task Manager: “Elevated”; fix testável em app elevada conforme matriz) |
| 4 | Fechar, reabrir, **negar** UAC | App inicia **sem** elevação; **MessageBox** pt-BR; JSON mantém `runAsAdmin: true` |
| 5 | Desmarcar opção + reiniciar app | Sem UAC por esta opção |

## Roteiro P2 — Combinação + RF-013 / RF-014

| # | Ação | Resultado esperado |
|---|------|-------------------|
| 1 | Ambas opções **ativas** + reinício de sessão | Auto-arranque + UAC |
| 2 | Negar UAC no login | App sem elevação + aviso pt-BR |
| 3 | Com app a correr, alternar `runAsAdmin` | Aviso + **Reiniciar agora** / **Mais tarde** |
| 4 | Remover `MouseScrollFixer` manualmente de Run (Task Manager → Apps de arranque) mantendo JSON `autoStartWithWindows: true` | Ao abrir config: aviso desalinhamento + ação reativar; JSON **permanece** `true` |

## Roteiro de regressão MVP

Após implementação, executar:

```powershell
dotnet test MouseScrollFixer.sln -c Release
```

Confirmar **zero regressões** em `AppConfigValidatorTests` e cenários 7–8 do quickstart do MVP (instância única, balão bandeja).

## Matriz mínima CS-001 / CS-003

Documentar por teste:

- Edição Windows 11 e build.
- Caminho do executável (portátil vs publicado).
- Resultado UAC: aprovado / negado.
- Estado JSON `startup` após cada passo.
- Presença/ausência valor Run.

## Notas de segurança

- **Elevação** é opt-in; o hook global com privilégios elevados tem maior alcance — usar apenas quando necessário para apps elevadas.
- Dados permanecem **locais**; sem telemetria (MVP RF-010).
