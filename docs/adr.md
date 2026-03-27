# ADR — Architectural Decision Records

## ADR-001 — Linguagem e Plataforma

**Decisão:** Uso de C# com .NET (net8.0-windows)

**Motivo:**

* Integração nativa com Windows
* Suporte fácil a Win32 via P/Invoke
* Boa produtividade

**Alternativas:**

* Python (rejeitado: performance e distribuição)
* C++ (rejeitado: complexidade desnecessária)

---

## ADR-002 — UI Framework

**Decisão:** Uso de WinForms (sem uso de designer)

**Motivo:**

* Suporte nativo a System Tray (`NotifyIcon`)
* Simplicidade

---

## ADR-003 — Captura de Eventos

**Decisão:** Uso de `SetWindowsHookEx` com `WH_MOUSE_LL`

**Motivo:**

* Captura global de eventos
* Necessário para interceptar scroll

---

## ADR-004 — Identificação de Janela

**Decisão:** Uso de `WindowFromPoint` + `GetAncestor`

**Motivo:**

* Permite identificar janela sob cursor corretamente

---

## ADR-005 — Comunicação com janela alvo

**Decisão:** Uso de `SendMessage` / `PostMessage`

**Motivo:**

* Simula evento de scroll

---

## ADR-006 — Filtro por Aplicação

**Decisão:** Whitelist baseada em caminho do executável

**Motivo:**

* Maior precisão
* Evita conflitos com apps similares

---

## ADR-007 — Persistência

**Decisão:** Arquivo JSON local

**Motivo:**

* Simples
* Fácil manutenção

---

## ADR-008 — Inicialização com Windows

**Decisão:** Registro no Registry (HKCU)

**Motivo:**

* Simples e suficiente

---

## ADR-009 — Execução como Administrador

**Decisão:** Opcional, configurável

**Motivo:**

* Necessário para apps elevados
* Evita exigir permissão sempre

---

## ADR-010 — Distribuição

**Decisão:** Publicação self-contained

**Motivo:**

* Não depender de runtime instalado

---

## ADR-011 — Arquitetura Modular

**Decisão:** Separação em Core, Services, UI

**Motivo:**

* Facilita manutenção e testes

---
