# Especificação da feature: opções de arranque com Windows e execução como administrador

**Branch da feature**: `002-startup-admin-options`  
**Criado**: 23/05/2026  
**Status**: Rascunho  
**Entrada**: Descrição do usuário: "Adicionar nas configurações duas opções: Iniciar automaticamente com o Windows (persistir preferência; refletir na UI e no app-config.json). Iniciar sempre como administrador (elevação UAC quando ativado; persistir preferência; refletir na UI e no app-config.json). Comportamento observável, pt-BR, Windows 11, alinhado ao MVP existente."

## Clarifications

### Session 2026-05-23

- Q: Quando o utilizador nega ou cancela o UAC com “executar como administrador” ativo, o utilitário deve iniciar sem elevação ou não iniciar? → A: **Inicia sem elevação**, com aviso em pt-BR de que a elevação falhou; a preferência permanece ativa para arranques futuros.
- Q: Ao alterar “executar como administrador” com o programa em execução, deve existir ação para reiniciar de imediato? → A: **Sim** — aviso informativo **e** ação opcional **“Reiniciar agora”** ao guardar a alteração; se o utilizador não reiniciar, a mudança aplica-se no próximo arranque.
- Q: Se o arranque automático for removido fora da app, como reconciliar UI e app-config.json? → A: **Manter preferência ativa** (`true`), **avisar em pt-BR** na configuração que o registo no sistema não existe, e permitir **reativar explicitamente** o registo.
- Q: Como estruturar as novas preferências no app-config.json? → A: **Secção dedicada `startup`** com dois campos booleanos (`autoStartWithWindows` e `runAsAdmin`, ou nomes equivalentes).
- Q: Deve incrementar `schemaVersion` ao introduzir a secção `startup`? → A: **Manter `schemaVersion: 1`** — secção `startup` **opcional**; ausência implica **`false`** para ambos os campos.

## Cenários de usuário e testes *(obrigatório)*

### História de usuário 1 — Iniciar automaticamente com o Windows (Prioridade: P1)

Como utilizador do **Windows 11** que depende do MouseScrollFixer no dia a dia, quero **ativar ou desativar** nas configurações o arranque automático do utilitário quando a minha sessão do Windows inicia, para não precisar abrir o programa manualmente após cada reinício ou login.

**Por que esta prioridade**: Garante que o fix permaneça disponível sem intervenção manual — requisito central para quem usa o produto de forma contínua.

**Teste independente**: Pode ser validado apenas alternando a opção na interface, reiniciando a sessão do Windows (ou simulando arranque automático conforme roteiro de testes) e verificando se o utilitário inicia sozinho ou não, sem depender da opção de administrador.

**Cenários de aceite**:

1. **Dado** que o utilitário está instalado ou disponível para execução e a opção de arranque automático está **desativada**, **quando** o utilizador **ativa** a opção nas configurações e confirma/guarda conforme o fluxo existente do MVP, **então** a preferência fica **persistida**, o controlo na interface reflete o estado **ativado** e, após **novo arranque da sessão do Windows**, o utilitário **inicia automaticamente** (incluindo presença na bandeja conforme comportamento atual do MVP).
2. **Dado** que a opção de arranque automático está **ativada**, **quando** o utilizador **desativa** a opção nas configurações, **então** a preferência fica **persistida**, o controlo reflete o estado **desativado** e, após **novo arranque da sessão do Windows**, o utilitário **não** inicia automaticamente.
3. **Dado** uma preferência de arranque automático já gravada, **quando** o utilizador reabre a janela de configurações, **então** o controlo correspondente **reflete o último estado persistido**, alinhado ao ficheiro de configuração local do utilizador.
4. **Dado** que o arranque automático está ativo, **quando** ocorre o arranque automático após login ou reinício, **então** aplicam-se as **mesmas regras de instância única e aviso de arranque na bandeja** já definidas no MVP (sem segunda instância paralela; aviso observável em pt-BR).

---

### História de usuário 2 — Iniciar sempre como administrador (Prioridade: P1)

Como utilizador que precisa que o fix funcione também em **aplicações com privilégios elevados**, quero **ativar ou desativar** nas configurações a execução **sempre como administrador**, para que, quando ativo, o Windows solicite **elevação (UAC)** no arranque e o utilitário corra com privilégios elevados.

**Por que esta prioridade**: Resolve um gap explícito do backlog (apps elevadas) e é independente em valor quando o utilizador arranca o programa manualmente com a opção ativa.

**Teste independente**: Pode ser validado ativando apenas esta opção, fechando e reabrindo o utilitário, observando o pedido UAC e confirmando que o processo corre elevado quando o utilizador aprova — sem depender do arranque automático com Windows.

**Cenários de aceite**:

1. **Dado** que a opção de executar como administrador está **desativada**, **quando** o utilizador **ativa** a opção nas configurações, **então** a preferência fica **persistida**, o controlo na interface reflete o estado **ativado** e, no **próximo arranque** do utilitário, o Windows apresenta um **pedido UAC observável** antes de concluir o arranque com privilégios elevados (se o utilizador **aprovar**).
2. **Dado** que a opção está **ativada** e o utilizador **aprova** o UAC, **quando** o arranque completa, **então** o utilitário está em execução com **privilégios de administrador** de forma verificável (por exemplo, capacidade de interagir com janelas elevadas nos cenários de teste acordados).
3. **Dado** que a opção está **ativada**, **quando** o utilizador **cancela ou nega** o pedido UAC, **então** o utilitário **inicia com privilégios normais** (sem elevação), recebe um **aviso compreensível em pt-BR** de que a elevação não ocorreu, e a **preferência permanece ativada** para tentativas futuras.
4. **Dado** que a opção está **ativada**, **quando** o utilizador **desativa** a opção nas configurações, **então** a preferência fica **persistida**, o controlo reflete **desativado** e, no **próximo arranque**, **não** é solicitado UAC por este motivo (salvo outro software ou política externa).
5. **Dado** que o utilitário **já está em execução**, **quando** o utilizador **altera** a opção de executar como administrador e guarda, **então** recebe **aviso em pt-BR** de que a mudança exige reinício e pode usar a ação **“Reiniciar agora”**; se **não** reiniciar, a alteração só produz efeito no **próximo arranque**.
6. **Dado** uma preferência de execução como administrador já gravada, **quando** o utilizador reabre a janela de configurações, **então** o controlo correspondente **reflete o último estado persistido**, alinhado ao ficheiro de configuração local do utilizador.

---

### História de usuário 3 — Combinação das duas opções (Prioridade: P2)

Como utilizador que quer o utilitário **sempre disponível e elevado** após login, quero poder **ativar ambas** as opções ao mesmo tempo, para que o arranque automático com o Windows também respeite a preferência de executar como administrador.

**Por que esta prioridade**: Entrega o fluxo completo “arrancar sozinho + elevado”, mas cada opção já entrega valor isoladamente (P1).

**Teste independente**: Pode ser validado com ambas as opções ativas, reinício de sessão e verificação de arranque automático seguido de UAC e execução elevada após aprovação.

**Cenários de aceite**:

1. **Dado** que **arranque automático** e **executar como administrador** estão **ativados**, **quando** a sessão do Windows inicia e dispara o arranque automático, **então** o utilizador vê o **pedido UAC** (se ainda não estiver elevado) e, após aprovação, o utilitário corre **elevado** e segue o comportamento normal do MVP (bandeja, instância única, aviso de arranque).
2. **Dado** o mesmo estado, **quando** o utilizador **nega** o UAC no arranque automático, **então** o utilitário **inicia sem elevação** com **aviso em pt-BR**, de forma **previsível e comunicada** (sem execução “silenciosa” como administrador; arranque automático não deixa o sistema num estado ambíguo quanto à elevação).

---

### Casos extremos

- **Primeira execução sem preferências gravadas**: ambas as opções iniciam **desativadas** por defeito; o utilizador opta explicitamente por ativar.
- **Configuração local ilegível ou corrompida**: o produto adota **estado seguro** (ambas desativadas), comunica de forma clara em pt-BR e não bloqueia o restante do MVP (fix, lista de inclusão, etc.), alinhado ao tratamento de configuração inválida já existente.
- **Utilizador sem permissão para elevar** (conta limitada ou política que impede UAC): a preferência pode ser gravada, mas a elevação **falha de forma clara**; mensagem em pt-BR explica que a execução como administrador não está disponível neste contexto.
- **UAC desativado ou política corporativa**: o produto **não assume** elevação; informa quando a elevação solicitada não puder ocorrer.
- **Caminho do executável alterado** (moveu pasta, novo atalho): ao ativar arranque automático, o produto deve registar o **executável atual**; se o arranque automático falhar por caminho inválido, o utilizador deve poder **corrigir reativando** a opção ou receber orientação clara (detalhe de detecção no plano).
- **Segunda instância durante arranque**: regras de **instância única** do MVP mantêm-se; pedido de elevação não deve criar duas instâncias não elevada e elevada em paralelo de forma confusa para o utilizador.
- **Alteração da opção de administrador com o programa já em execução**: ao guardar, a interface apresenta **aviso em pt-BR** e a ação opcional **“Reiniciar agora”**; se o utilizador não reiniciar, a mudança aplica-se no **próximo arranque** manual ou automático.
- **Desinstalação ou remoção manual do arranque automático fora da app**: ao abrir configurações, se a preferência persistida estiver **ativa** mas o registo no sistema **não existir**, a interface **mantém** o controlo como **ativado** (alinhado ao JSON), exibe **aviso em pt-BR** do desalinhamento e oferece ação para **reativar explicitamente** o registo de arranque automático.
- **Idioma do Windows diferente de pt-BR**: textos desta feature permanecem em **português (Brasil)**, consistente com o MVP.
- **Windows 10 ou anterior**: **fora de escopo**; comportamento alinhado ao suporte **apenas Windows 11** do MVP.

## Requisitos *(obrigatório)*

### Requisitos funcionais

- **RF-001**: O produto DEVE expor na **janela de configurações** (ou secção equivalente já usada no MVP) um controlo **claro e rotulado em pt-BR** para **iniciar automaticamente com o Windows**, permitindo **ativar e desativar** a preferência.
- **RF-002**: O produto DEVE **persistir** a preferência de arranque automático no **ficheiro de configuração local do utilizador** (`app-config.json`), na secção dedicada **`startup`**, de forma que reinstalações da mesma versão ou reabertura da app **restaurem** o último estado escolhido.
- **RF-003**: Quando a preferência de arranque automático estiver **ativada**, o produto DEVE garantir que o utilitário **inicia automaticamente** no **arranque da sessão do utilizador** no Windows 11, usando o **executável atual** do produto. Quando estiver **desativada**, DEVE **remover ou desativar** o mecanismo de arranque automático associado, de modo que **não** haja arranque automático nas sessões seguintes.
- **RF-004**: O produto DEVE expor na **janela de configurações** um controlo **claro e rotulado em pt-BR** para **iniciar sempre como administrador**, permitindo **ativar e desativar** a preferência.
- **RF-005**: O produto DEVE **persistir** a preferência de execução como administrador na secção **`startup`** do mesmo ficheiro de configuração local (`app-config.json`), sincronizada com o estado mostrado na interface.
- **RF-006**: Quando a preferência de executar como administrador estiver **ativada**, o produto DEVE, em **cada arranque** do executável (manual ou automático), **solicitar elevação via UAC** antes de concluir o arranque com privilégios elevados, **salvo** já estar em execução elevada de forma legítima conforme regras do plano.
- **RF-007**: Quando o utilizador **negar ou cancelar** o UAC com a preferência de administrador ativa, o produto DEVE **continuar o arranque com privilégios normais** (sem elevação), **informar em pt-BR** que a elevação não ocorreu, **não** deve induzir o utilizador a acreditar que corre elevado, e DEVE **manter** a preferência gravada para arranques futuros.
- **RF-008**: Ao **carregar** a interface de configurações, os controlos das duas novas opções DEVEM **refletir fielmente** os valores persistidos; ao **guardar ou alterar** conforme o padrão do MVP, o ficheiro `app-config.json` DEVE ser **atualizado** pelos mesmos fluxos já usados para as restantes opções.
- **RF-009**: As novas preferências DEVEM **coexistir** com todas as opções existentes do MVP (ativação do fix, lista de inclusão, perfil de comportamento) **sem** invalidar configurações anteriores; **`schemaVersion` permanece `1`**; a secção **`startup` é opcional** e valores em falta DEVEM assumir **`false`** para `autoStartWithWindows` e `runAsAdmin`.
- **RF-010**: O produto DEVE tratar falhas ao aplicar arranque automático ou elevação de forma **segura e comunicada** (mensagens ou estados observáveis em pt-BR), sem corromper o restante da configuração.
- **RF-011**: Textos de interface, mensagens de erro ou aviso relacionados a estas opções DEVEM estar em **português (Brasil)**, alinhados ao RF-009 do MVP.
- **RF-012**: Comportamento de **instância única** e **aviso de arranque na bandeja** do MVP DEVEM manter-se aplicáveis quando o arranque ocorrer por arranque automático ou após elevação.
- **RF-013**: Quando o utilizador **alterar** a preferência de executar como administrador com o utilitário **já em execução**, o produto DEVE **persistir** imediatamente a nova preferência, **informar em pt-BR** que a mudança exige reinício para produzir efeito na elevação, e oferecer a ação opcional **“Reiniciar agora”**; se o utilizador **não** reiniciar, a elevação (ou sua remoção) aplica-se apenas no **próximo arranque**.
- **RF-014**: Se a preferência de arranque automático estiver **ativa** no ficheiro de configuração mas o **registo no sistema estiver ausente** (remoção externa), ao abrir configurações o produto DEVE **manter** a preferência e o controlo como **ativados**, **avisar em pt-BR** do desalinhamento e permitir **reativar explicitamente** o registo; **não** DEVE alterar silenciosamente a preferência para desativada nem re-registar automaticamente sem ação do utilizador.

### Entidades principais

- **Preferências de arranque (`startup`)**: objeto no ficheiro de configuração local com duas propriedades booleanas — **`autoStartWithWindows`** (arranque automático com a sessão do Windows) e **`runAsAdmin`** (executar sempre como administrador) — espelhadas na interface de configurações; valores em falta em ficheiros antigos assumem **`false`** para ambos.
- **Registo de arranque automático (conceitual)**: associação entre a preferência ativa e o executável do produto no contexto da sessão do utilizador no Windows; deve estar **consistente** com a preferência persistida (detalhe operacional no plano, não nesta especificação).

## Critérios de sucesso *(obrigatório)*

### Resultados mensuráveis

- **CS-001**: Em teste guiado no Windows 11, **100%** dos cenários de aceite das histórias P1 (arranque automático isolado e administrador isolado) passam em **até 3 tentativas** por cenário, com estado persistido verificável na interface e no ficheiro de configuração local.
- **CS-002**: Após ativar arranque automático e reiniciar a sessão, o utilitário está **disponível na bandeja em até 2 minutos** após login (tempo típico de arranque de sessão), **sem** exigir que o utilizador abra manualmente o executável.
- **CS-003**: Com “executar como administrador” ativo, **100%** dos arranques aprovados pelo utilizador no UAC resultam em execução **verificavelmente elevada** nos cenários de teste acordados; **100%** das negações de UAC produzem **arranque sem elevação**, **aviso em pt-BR** e **ausência** de execução elevada.
- **CS-004**: Utilizadores em teste de usabilidade (**≥ 3**) conseguem **localizar e alterar** ambas as opções na configuração em **menos de 1 minuto** cada, sem documentação externa.
- **CS-005**: Ficheiros `app-config.json` produzidos ou migrados pela feature **continuam válidos** para o restante do MVP (ativação, lista, comportamento), com **`schemaVersion: 1`** e secção **`startup` opcional**, com **zero regressões** nos testes de validação de configuração existentes.

## Premissas

- O **MVP existente** (Windows 11, pt-BR, bandeja, instância única, persistência em `%LocalAppData%\MouseScrollFixer\app-config.json`) permanece a base; esta feature **estende** configurações e arranque, sem alterar o núcleo do fix de scroll.
- **Valor por defeito** das duas novas opções: **desativado** (`false`), até o utilizador optar explicitamente por ativar; ficheiros antigos **sem** secção `startup` são tratados como **`false`** em ambos os campos, **sem** alterar `schemaVersion`.
- **Arranque automático** refere-se ao **início da sessão do utilizador** no Windows 11 (login ou reinício com auto-login), não a arranques a frio de serviços sem sessão interativa.
- **Executar como administrador** destina-se a utilizadores que **aceitam** prompts UAC; o produto **não contorna** UAC nem políticas de segurança do sistema.
- Alterações à preferência de administrador **entram em vigor no próximo arranque** do utilitário, salvo o utilizador escolher **“Reiniciar agora”** ao alterar a opção com o programa em execução.
- Não há **telemetria** nem envio remoto de preferências; dados permanecem **locais**, alinhado ao MVP.
- Mecanismos concretos de registo no arranque do Windows, manifesto de elevação e deteção de processo elevado ficam no **plano técnico**, desde que o comportamento observável desta especificação seja satisfeito.
