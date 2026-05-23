# Pesquisa e decisões técnicas — 002-startup-admin-options

Este documento resolve decisões de implementação para arranque automático com Windows e execução como administrador, alinhadas à especificação e às clarificações de 2026-05-23.

---

## 1. Registo de arranque automático (RF-003)

**Decisão**: Usar a chave de registo **`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`** com valor **`MouseScrollFixer`** apontando para o **caminho absoluto entre aspas** do executável atual (`Environment.ProcessPath` ou equivalente).

**Racional**: Padrão de arranque **por sessão de utilizador** no Windows 11; escrita em **HKCU** não exige privilégios de administrador para registar/desregistar; alinha com “início da sessão do utilizador” na especificação.

**Alternativas consideradas**:
- **Pasta Startup do utilizador** — equivalente funcional, mas menos fácil de detetar desalinhamento programaticamente; Run key é idempotente e familiar.
- **Task Scheduler (Logon)** — robusto para “Run with highest privileges”, mas complexidade e permissões desnecessárias quando a elevação é tratada no `Program.Main` (decisão 2).
- **Serviço Windows** — fora de escopo e privilégios excessivos.

---

## 2. Elevação UAC configurável (RF-006, RF-007)

**Decisão**: Manter **`app.manifest`** com **`asInvoker`**. No **início de `Program.Main`**, após cultura pt-BR e **antes** do mutex de instância única:

1. Ler configuração mínima (`startup.runAsAdmin`) via `AppConfigStore` (ou leitura parcial do JSON).
2. Se `runAsAdmin == true` e o processo **não** estiver elevado (`ProcessElevationHelper.IsProcessElevated()`), tentar **`Process.Start`** com **`Verb = "runas"`** e o mesmo executável/argumentos.
3. Se o utilizador **aprovar** UAC → o processo elevado continua; o processo não elevado **termina** (`return`).
4. Se o utilizador **negar** UAC (`Win32Exception` 1223 ou equivalente) → **continuar** no processo atual **sem elevação**, mostrar **MessageBox** em pt-BR (RF-007), **sem** alterar a preferência persistida.

**Racional**: Permite alternar “executar como administrador” **sem** recompilar manifest; funciona igual para arranque **manual** e **automático** (Run key dispara o mesmo `.exe`); cumpre clarificação “inicia sem elevação com aviso”.

**Alternativas consideradas**:
- **`requireAdministrator` no manifest** — elevação sempre, impossível desligar pela UI sem segundo binário.
- **Executável launcher separado** — mais ficheiros de distribuição e complexidade de atualização de caminho.
- **Task Scheduler com RunLevel Highest** — duplica mecanismos quando Run + bootstrap UAC bastam.

---

## 3. Ordem de arranque: elevação vs instância única (RF-012)

**Decisão**: Sequência em `Program.Main`:

1. Cultura pt-BR  
2. Gate de elevação (decisão 2) — pode terminar o processo ou continuar  
3. **`SingleInstanceCoordinator.TryAcquireSingleton`**  
4. `RunApplication()` (resto do MVP)

**Racional**: Evita que o processo “stub” não elevado retenha o mutex enquanto o processo elevado arranca; evita duas instâncias visíveis durante elevação.

**Alternativas consideradas**: Mutex antes da elevação — bloqueia o arranque elevado se o stub detiver o mutex.

---

## 4. Deteção de desalinhamento do arranque automático (RF-014)

**Decisão**: Serviço **`WindowsStartupRegistration`** com:

- **`IsRegistered(expectedPath)`** — lê valor `MouseScrollFixer` em Run e compara caminhos normalizados (case-insensitive, aspas removidas).
- **`Register(path)`** / **`Unregister()`** — aplicados quando o utilizador altera a opção ou clica **“Reativar arranque automático”** na UI.
- Ao **abrir** `MainSettingsForm`: se `startup.autoStartWithWindows == true` e `!IsRegistered(currentPath)`, mostrar **painel/aviso** em pt-BR + botão para **re-aplicar** registo (sem alterar JSON para `false`).

**Racional**: Implementa clarificação “manter preferência ativa, avisar, reativar explicitamente”.

**Alternativas consideradas**: Auto-correção silenciosa — viola RF-014; forçar `false` no JSON — viola clarificação.

---

## 5. “Reiniciar agora” ao alterar `runAsAdmin` (RF-013)

**Decisão**: Após persistir a alteração de `runAsAdmin`, se o valor **mudou** em relação ao estado efetivo da sessão (elevado vs não elevado), mostrar **`MessageBox`** com texto explicativo e botões **Sim/Não** (“Reiniciar agora” / “Mais tarde”). **Sim** → `Process.Start` do executável atual (passará pelo gate UAC se aplicável) + **`Application.Exit()`** na instância corrente.

**Racional**: Feedback imediato sem reinício forçado; alinha RF-013.

**Alternativas consideradas**: Reinício silencioso — UX agressiva; só mensagem sem ação — viola clarificação.

---

## 6. Modelo JSON `startup` (RF-009)

**Decisão**: Secção opcional **`startup`** no `app-config.json`:

```json
"startup": {
  "autoStartWithWindows": false,
  "runAsAdmin": false
}
```

**`schemaVersion` permanece `1`**. `MergeDefaults` garante objeto `startup` com ambos `false` se ausente.

**Racional**: Clarificação da especificação; compatível com configs MVP existentes.

**Alternativas consideradas**: `schemaVersion: 2` — rejeitado na clarificação; campos na raiz — pior agrupamento semântico.

---

## 7. Verificação de processo elevado

**Decisão**: Helper **`ProcessElevationHelper.IsProcessElevated()`** usando **`WindowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator)`** com **`WindowsIdentity.GetCurrent()`**, documentando que reflete **token elevado da sessão** (suficiente para testes de RF-006 e interação com janelas elevadas nos roteiros).

**Racional**: API estável em .NET 8 Windows; sem P/Invoke extra para `TokenElevationType` salvo necessidade em testes.

**Alternativas consideradas**: **`GetTokenInformation(TokenElevation)`** — mais preciso em edge cases UAC filtrado; adotar se testes falharem em máquinas de referência.

---

## 8. Hook global e privilégios (constituição III)

**Decisão**: Documentar em `quickstart.md` que **elevação aumenta a superfície** (hook vê mais contextos, incluindo apps elevadas) e que o utilizador deve **aceitar UAC** conscientemente; **não** elevar por defeito.

**Racional**: Princípio “privilégios mínimos” da constituição; opção explícita na UI.

**Alternativas consideradas**: Elevação automática sempre que lista incluir app elevado — fora de escopo e surpreendente.

---

## 9. Testes automatizados vs manuais

**Decisão**:
- **Automatizados (xUnit)**: `MergeDefaults` com/sem `startup`; validação JSON; normalização de caminho em `WindowsStartupRegistration` (com abstração testável ou wrapper de registo mockável).
- **Manuais (Windows 11)**: UAC aprovar/negar, reinício de sessão com Run key, desalinhamento externo, “Reiniciar agora”, combinação auto-start + admin.

**Racional**: Registo real e UAC não são estáveis em CI headless; lógica pura coberta por testes.

**Alternativas consideradas**: Testes de integração contra registo real — frágeis e poluem máquina de dev.
