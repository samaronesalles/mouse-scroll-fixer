# Contexto do Projeto — MouseScrollFixer

## Visão geral

`MouseScrollFixer` é um utilitário desktop para Windows 11 que fica na bandeja e corrige o comportamento de scroll em aplicativos legados selecionados pelo usuário.

O foco atual é **scroll vertical**. O produto intercepta `WM_MOUSEWHEEL`, identifica o app sob o cursor e aplica o fix **somente** quando o executável está na lista de inclusão.

## Problema que o projeto resolve

Aplicações antigas podem ignorar ou tratar mal o scroll moderno. O usuário precisa de um comportamento previsível sem impactar todo o sistema.

Ferramentas globais sem filtro tendem a interferir em apps que já funcionam bem. Por isso o MouseScrollFixer usa whitelist por executável.

## Comportamento atual implementado

1. O hook global (`WH_MOUSE_LL`) recebe o evento de mouse.
2. Apenas eventos `WM_MOUSEWHEEL` são considerados (horizontal fica fora do MVP).
3. O sistema resolve `HWND` sob o cursor e obtém o caminho do executável do processo dono.
4. Se o executável não estiver em `inclusionList`, o evento segue normal.
5. Se estiver na lista:
   - aplica normalização conforme `behavior`;
   - envia `WM_MOUSEWHEEL` por `PostMessageW` para o alvo efetivo (preferindo foco de teclado no mesmo processo);
   - ou, em modo legado, converte para múltiplos `WM_VSCROLL`.
6. O evento original é consumido para evitar duplicação.

## Componentes principais

- `Program` + `TrayApplication`: ciclo de vida da app e bandeja.
- `ScrollFixerSession`: coordena hook, filtro por inclusão e despacho de mensagens.
- `LowLevelMouseHook`: instalação/suspensão do `WH_MOUSE_LL`.
- `WindowTargetResolver`: mapeia ponto do cursor para app/processo.
- `ScrollNormalizer`: extrai/normaliza delta vertical.
- `AppConfigStore` + `AppConfigValidator`: persistência e validação de `app-config.json`.
- `SingleInstanceCoordinator`: instância única (mutex + pipe).
- `MainSettingsForm`: UI para ativar fix, editar lista e ajustar comportamento.

## Regras de negócio atuais

- O fix só atua quando `activation.enabled = true`.
- O fix só atua para executáveis explicitamente listados em `inclusionList`.
- A lista é limitada a 64 entradas válidas.
- Configuração inválida/corrompida cai para estado seguro e mostra aviso.
- Não há telemetria nem envio remoto de uso.

## Persistência atual

Local: `%LocalAppData%\MouseScrollFixer\`

- `app-config.json` (principal)
- `app-config.bak.json` (backup)

Estrutura principal:

```json
{
  "schemaVersion": 1,
  "activation": {
    "enabled": false,
    "lastModifiedUtc": "2026-03-30T12:34:56.0000000Z"
  },
  "inclusionList": [],
  "behavior": {
    "invertVertical": false,
    "linesPerNotchApprox": 3.0,
    "touchpadSameAsWheel": true,
    "useVScrollFallback": false
  }
}
```

## Limitações e escopo

- Suporte oficial atual: Windows 11.
- MVP: apenas scroll vertical.
- Não há hoje implementação de auto start com Windows.
- Não há modo “executar como administrador” configurável na UI.
