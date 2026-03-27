# Backlog do Produto — Mouse Scroll Fixer

## 🧩 Feature 1 — Base do Projeto

### User Story

Como desenvolvedor, quero criar a base do projeto para iniciar o desenvolvimento.

### Descrição

Criação da estrutura inicial com .NET.

### Requisitos Funcionais

* Criar projeto .NET
* Estrutura de pastas

### Requisitos Não Funcionais

* Organização clara
* Código modular

---

## 🧩 Feature 2 — Hook Global de Mouse

### User Story

Como sistema, quero capturar eventos de scroll globalmente.

### Descrição

Implementação do hook `WH_MOUSE_LL`.

### Requisitos Funcionais

* Interceptar `WM_MOUSEWHEEL`

### Requisitos Não Funcionais

* Baixa latência
* Estável

---

## 🧩 Feature 3 — Identificação da Janela

### User Story

Como sistema, quero identificar a janela sob o cursor.

### Descrição

Uso de `WindowFromPoint`.

### Requisitos Funcionais

* Obter HWND correto

---

## 🧩 Feature 4 — Resolução de Processo

### User Story

Como sistema, quero identificar o processo da janela.

### Descrição

Uso de `GetWindowThreadProcessId`.

---

## 🧩 Feature 5 — Filtro por Aplicação

### User Story

Como usuário, quero definir quais apps usarão o scroll.

### Descrição

Whitelist baseada em executável.

---

## 🧩 Feature 6 — Scroll Dispatcher

### User Story

Como sistema, quero redirecionar o scroll.

### Descrição

Enviar `WM_MOUSEWHEEL`.

---

## 🧩 Feature 7 — Configuração Persistente

### User Story

Como usuário, quero salvar minhas preferências.

### Descrição

Arquivo JSON.

---

## 🧩 Feature 8 — Tray Icon

### User Story

Como usuário, quero controlar o app pelo tray.

### Descrição

Menu com opções básicas.

---

## 🧩 Feature 9 — Ativar/Desativar Sistema

### User Story

Como usuário, quero ligar/desligar o sistema.

---

## 🧩 Feature 10 — Tela de Configuração

### User Story

Como usuário, quero gerenciar apps permitidos.

---

## 🧩 Feature 11 — Auto Start

### User Story

Como usuário, quero iniciar com Windows.

---

## 🧩 Feature 12 — Execução como Admin

### User Story

Como usuário, quero rodar como admin.

---

## 🧩 Feature 13 — Logs

### User Story

Como desenvolvedor, quero logs para debug.

---

## 🧩 Feature 14 — Otimização de Performance

### User Story

Como sistema, quero ser leve.

---

## 🧩 Feature 15 — Build e Distribuição

### User Story

Como usuário, quero instalar facilmente.

---
