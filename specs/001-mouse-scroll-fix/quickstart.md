# Quickstart — Mouse Scroll Fixer (MVP)

**Público**: utilizador ou testador no **Windows 11 (x64)**. **Idioma**: pt-BR.

## Pré-requisitos

- Windows 11 (build mínimo indicado no instalador ou notas de release).
- Permissões normais de utilizador; **execução como administrador** apenas se o plano de testes o exigir (ex.: interação com janelas elevadas).

## Instalação (quando existir instalador)

1. Executar o instalador.
2. Marcar ou desmarcar **“Ativar o fix ao concluir”** (texto final pode variar ligeiramente).
3. Concluir; o estado inicial de ativação DEVE corresponder à opção (RF-002).

## Instalador opcional e primeira preferência (RF-002)

**Neste incremento do repositório** não é obrigatório existir um pacote de instalador publicado; o fluxo suportado é **portátil** (executável + ficheiros) e a preferência **ligado/desligado** é definida na **interface** (bandeja ou janela de configurações) e gravada em JSON local.

**Fluxo equivalente** quando existir instalador (Inno Setup, WiX, MSIX, etc.):

- O instalador pode oferecer a opção **«Ativar o fix ao concluir»** que define a **primeira** preferência persistida (`activation.enabled`), em conformidade com a especificação e a matriz de testes.
- Se a configuração já existir e for válida, o produto deve seguir a política de não sobrescrever sem intenção do utilizador (detalhar na release).

Documentação auxiliar: `installer/README.md` na raiz do repositório.

## Primeira execução sem instalador

1. Extrair / copiar o pacote portátil para uma pasta local.
2. Executar o binário principal.
3. Se não existir preferência gravada, seguir o defeito documentado no produto (ex.: fix **desligado** até ativar manualmente).

## Uso mínimo (menos de 1 minuto) — CS-002

1. Abrir o utilitário a partir da bandeja ou atalho.
2. **Ativar** o fix (se desligado).
3. Adicionar **um** aplicativo à lista de inclusão (por exemplo, escolher o `.exe` através do diálogo).
4. Deixar o cursor sobre uma área rolável desse aplicativo e usar a **roda** ou o **scroll vertical do touchpad**.
5. **Desativar** o fix e repetir o passo 4 — deve observar-se diferença de comportamento nos cenários de teste acordados.

## Roteiro de verificação rápida (ligado aos critérios de aceite)

| # | Ação | Resultado esperado |
|---|------|-------------------|
| 1 | Lista vazia + fix ligado | Sem alteração de scroll para apps não listados (comportamento fora da lista = sistema). |
| 2 | Adicionar app válido | Entrada aparece na lista; com cursor sobre esse app, scroll vertical segue regra normalizada. |
| 3 | Remover entrada | Deixa de aplicar sem reinício do SO (salvo limitação documentada). |
| 4 | Alternar ligado/desligado | Comportamento P1 / sistema conforme P2. |
| 5 | Reinício do Windows com último estado “ligado” | CS-005 — fix ativo sem reativar manualmente. |
| 6 | Simular ficheiro de configuração corrompido | Estado seguro + mensagem; sem travar a sessão. |
| 7 | Com instância já em execução (com ou sem janela visível), iniciar o `.exe` novamente | **Não** surge segunda instância; janela existente **ativa**, **primeiro plano**, **visível**, **aba de configurações** (RF-011, CS-006). |
| 8 | Arranque com programa só na bandeja (sem janela principal visível) | **Aviso observável** (balão/notificação ou equivalente) em **pt-BR**: programa iniciado e disponível na bandeja (RF-012, CS-006). |

## Ambiente de referência para CS-001

Documentar por release, por exemplo:

- Edição do Windows 11 (Home/Pro).
- Build exato.
- Aplicações de exemplo da matriz mínima (nomes e versões).
- Dispositivo de entrada: rato + touchpad se disponível.

## Onde está a configuração

- Implementação atual: `%LocalAppData%\MouseScrollFixer\` (por exemplo `app-config.json` e cópia de segurança).
- **Não** contém telemetria; dados locais apenas (RF-010).

## Checklist de regressão (release / alterações grandes)

Ver `specs/001-mouse-scroll-fix/checklists/regression.md` (RF-010, configuração corrompida, RF-007 e cenários alinhados a esta página).
