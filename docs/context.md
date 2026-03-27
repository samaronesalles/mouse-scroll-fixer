# Contexto do Projeto — Mouse Scroll Fixer

## 📌 Visão Geral

O Mouse Scroll Fixer é uma aplicação desktop para Windows que roda em background (System Tray) e tem como objetivo habilitar o uso do scroll do mouse em aplicações legadas que não suportam esse comportamento nativamente.

O sistema melhora o comportamento padrão do Windows, permitindo que o scroll funcione na janela sob o cursor do mouse, mesmo sem foco — porém de forma controlada e configurável.

---

## 🎯 Problema

No Windows, o comportamento padrão exige que uma janela esteja em foco para receber eventos de scroll (`WM_MOUSEWHEEL`).

Aplicações antigas (legadas) frequentemente:

* Não recebem scroll corretamente
* Não respondem a eventos modernos de input

Ferramentas existentes como o WizMouse resolvem parcialmente o problema, porém:

* Aplicam comportamento global (sem filtro)
* Interferem em aplicações modernas
* Podem causar duplicação de eventos ou inconsistências

---

## 💡 Solução Proposta

Criar uma aplicação que:

* Intercepta eventos globais de scroll do mouse
* Identifica a janela sob o cursor
* Redireciona o evento de scroll para essa janela
* Aplica esse comportamento apenas em aplicações configuradas pelo usuário

---

## ⚙️ Funcionamento Técnico

### Fluxo principal:

1. Usuário gira o scroll do mouse
2. Hook global intercepta o evento
3. Sistema identifica posição do cursor
4. Resolve qual janela está sob o cursor
5. Obtém o processo dono da janela
6. Verifica se o processo está na whitelist
7. Se sim → envia `WM_MOUSEWHEEL` para a janela
8. Se não → ignora

---

## 🧩 Componentes do Sistema

### 1. Mouse Hook Engine

* Captura eventos globais de mouse
* Usa `WH_MOUSE_LL`

### 2. Window Resolver

* Resolve HWND baseado na posição do cursor
* Trata janelas filhas e raiz

### 3. Process Filter

* Lista de aplicações permitidas (whitelist)
* Baseado em caminho completo do executável

### 4. Scroll Dispatcher

* Responsável por enviar eventos para a janela alvo

### 5. Tray Application

* Interface mínima via system tray
* Permite ativar/desativar sistema
* Acesso às configurações

### 6. Config Manager

* Persistência em arquivo JSON
* Gerencia preferências do usuário

### 7. Auto-start Manager

* Controla inicialização com Windows

### 8. Privilege Manager

* Gerencia execução como administrador

---

## 🧠 Regras de Negócio

* O sistema só atua em aplicações explicitamente permitidas
* O sistema pode ser ativado/desativado globalmente
* Deve funcionar em background com baixo consumo
* Não deve interferir em aplicações modernas

---

## 📦 Persistência

Formato JSON:

```json
{
  "enabled": true,
  "autoStart": true,
  "runAsAdmin": false,
  "allowedApps": []
}
```

---

## ⚠️ Restrições Técnicas

* Uso de hooks globais pode ser sensível a antivírus
* Algumas aplicações não respondem a `WM_MOUSEWHEEL`
* Aplicações elevadas exigem execução como admin

---

## 🎯 Objetivos do Projeto

* Criar alternativa superior ao WizMouse
* Garantir controle granular por aplicação
* Minimizar impacto no sistema
* Arquitetura simples, modular e extensível

---
