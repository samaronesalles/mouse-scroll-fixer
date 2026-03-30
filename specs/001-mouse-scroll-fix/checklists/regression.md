# Checklist de regressão — 001-mouse-scroll-fix

**Idioma**: pt-BR. **Uso**: antes de release ou após alterações relevantes; complementa o `quickstart.md`.

## RF-010 (sem telemetria)

- Confirmar que o produto **não** envia dados de utilização para fora da máquina (sem endpoints de telemetria no código; configuração apenas local).
- Verificar que eventual “verificação de atualizações” futura, se existir, está documentada como **sem** recolha de uso (conforme especificação).

## Configuração corrompida / inválida

- Com `app-config.json` inválido: ao arrancar, estado seguro (fix desligado / recuperação conforme implementação) e **mensagem** ao utilizador, sem bloquear a sessão.
- Após correção manual ou restauração, a aplicação volta a gravar e a ler normalmente.

## Funcional (alinhado ao `quickstart.md`)

| # | Verificação |
|---|-------------|
| 1 | Lista vazia + fix ligado: apps fora da lista sem alteração de scroll. |
| 2 | Adicionar `.exe` válido: entrada na lista; scroll normalizado com foco nessa app. |
| 3 | Remover entrada: deixa de aplicar sem reinício do SO. |
| 4 | Alternar fix ligado/desligado (UI e bandeja): comportamento coerente com P1/P2. |
| 5 | Reinício do Windows com último estado “ligado”: fix ativo sem reativar manualmente (CS-005). |
| 6 | **RF-007**: com software conhecido de remap/scroll em execução, aparece **aviso**; nenhuma desativação automática de terceiros. |

## Notas

- Ajustar builds e caminhos exatos (`%LocalAppData%\MouseScrollFixer\`) conforme notas de release.
