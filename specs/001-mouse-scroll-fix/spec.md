# Especificação da feature: correção do comportamento do scroll do mouse

**Branch da feature**: `001-mouse-scroll-fix`  
**Criado**: 27/03/2026  
**Status**: Implementado (MVP) e em manutenção  
**Entrada**: Descrição do usuário: "Utilitário Windows para corrigir ou normalizar o comportamento do scroll do mouse de forma previsível, com impacto mínimo no sistema e comportamento verificável."

## Clarifications

### Session 2026-03-27

- Q: Qual o escopo do fix no MVP (global, lista de inclusão, exclusão ou dois modos)? → A: **Lista de inclusão no MVP** — apenas uma lista explícita de aplicativos e/ou cenários cobertos; fora dela não há obrigação de correção até versão futura.
- Q: A preferência ligado/desligado deve persistir entre reinícios do Windows? → A: **Sim — persistir** entre sessões; restaurar o último estado escolhido pelo usuário após reinício.
- Q: Como tratar conflito com outro software que altera entrada ou scroll? → A: **Aviso ao usuário**; **sem** precedência automática — o utilizador decide (desativar um dos lados ou seguir orientação na ajuda).
- Q: Scroll horizontal no MVP? → A: **Só vertical no MVP**; scroll horizontal **fora de escopo** até versão futura.
- Q: Estado padrão na primeira execução (sem preferência gravada)? → A: **Definido no instalador** (por exemplo, checkbox); essa escolha torna-se a **primeira preferência persistida**.
- Q: No MVP, a lista de inclusão é editável pelo utilizador? → A: **Sim — lista editável na interface** no MVP (adicionar/remover aplicativos e/ou cenários), **dentro de limites** definidos no plano (por exemplo: teto de entradas, forma de identificar app/cenário).
- Q: Qual o suporte de sistema operacional no MVP (Windows 10 e 11, apenas 11, etc.)? → A: **Apenas Windows 11** no MVP; **Windows 10** e outras versões **fora de escopo** nesta versão (suporte futuro apenas se documentado em release posterior).
- Q: No MVP, o fix aplica-se só à roda do mouse ou também ao touchpad? → A: **Roda do mouse e touchpad** no MVP — **mesma regra** de normalização do **scroll vertical** quando o mecanismo o permitir; **limites e exceções** (por exemplo hardware ou piloto) no **plano**.
- Q: Qual o idioma da interface e textos do produto no MVP? → A: **Apenas português (Brasil) — pt-BR** na interface, mensagens, ajuda embutida e textos do instalador ligados à experiência do produto; **outros idiomas fora de escopo** nesta versão.
- Q: Telemetria ou análise de uso no MVP? → A: **Sem telemetria nem análise de uso** no MVP; **nenhum** envio de dados de utilização do produto para fora da máquina por este fim; eventual **verificação de atualizações** (se existir) **não** substitui nem contorna este requisito com recolha de uso — detalhe no plano.

### Session 2026-03-30

- Q: Comportamento ao tentar abrir uma segunda instância e aviso ao iniciar na bandeja? → A: **Instância única** — se já houver uma instância (mesmo só na bandeja), nova execução **ativa** a existente: **restaura** a janela em primeiro plano, **visível** na área de trabalho, e abre a **aba de configurações**. Em **todo** arranque (incluindo início “silencioso” na bandeja), deve haver **sempre** um **aviso observável** (por exemplo **balão** ou equivalente) de que o programa foi iniciado e está na bandeja, em **pt-BR**.

## Cenários de usuário e testes *(obrigatório)*

### História de usuário 1 — Scroll previsível no uso diário (Prioridade: P1)

Como utilizador do **Windows 11** que sofre com scroll acelerado demais, invertido, com passos irregulares ou ignorado por alguns aplicativos, quero **definir na interface** quais aplicativos e/ou cenários entram na **lista de inclusão** e quero que o **scroll vertical** — por **roda do mouse** ou **gesto de touchpad** equivalente — se comporte de modo **consistente e previsível** nesses itens, para conseguir ler, navegar e trabalhar sem ajustes manuais constantes nesses contextos.

**Por que esta prioridade**: Entrega o valor central do produto; sem isso não há release utilizável.

**Teste independente**: Pode ser validado instalando ou executando apenas o núcleo do fix, exercitando aplicações de exemplo definidas no plano e verificando se o scroll atende aos cenários de aceite sem depender de histórias P2/P3.

**Cenários de aceite**:

1. **Dado** que o fix está ativo e a janela em primeiro plano pertence à **lista de inclusão** (entradas atualmente configuradas), **quando** o usuário usa a **roda do mouse** para **scroll vertical**, **então** o deslocamento do conteúdo segue uma regra explícita (por exemplo: direção, passo aproximado por “clique” da roda, ausência de saltos inesperados nos casos de teste acordados).
2. **Dado** o mesmo estado ativo e o mesmo contexto de janela na lista de inclusão, **quando** o utilizador produz **scroll vertical** via **touchpad** (gesto equivalente), **então** aplica-se a **mesma regra** de normalização que para a roda, **salvo** limitações documentadas no plano.
3. **Dado** o mesmo estado ativo, **quando** o usuário repete a ação várias vezes em sequência (roda ou touchpad conforme roteiro), **então** o comportamento permanece estável (sem degradar visivelmente nem acumular erro nos cenários de teste).
4. **Dado** o fluxo de gestão da lista na interface, **quando** o utilizador **adiciona** um aplicativo ou cenário válido segundo o plano (limites e identificação), **então** esse item passa a ser tratado como incluso para efeitos de P1 quando em foco; **quando** **remove** uma entrada, **então** deixa de ser coberto pelo fix sem exigir reinício do sistema operacional (salvo limitação documentada no plano).

---

### História de usuário 2 — Controle de ativação (Prioridade: P2)

Como usuário, quero **ligar ou desligar** o comportamento do fix sem reinstalar o sistema, para poder comparar “antes/depois” e evitar interferência quando não for desejada. No MVP **não** há modo de operação separado denominado “modo seguro” além de **desativar** o fix (volta ao comportamento do sistema sem o utilitário nos cenários cobertos); eventual rotulagem na interface deve alinhar-se a RF-002.

**Por que esta prioridade**: Aumenta confiança e adesão; é essencial para suporte e diagnóstico, mas o produto ainda entrega valor com P1 sozinha em ambiente controlado.

**Teste independente**: Pode ser testado alternando o estado ativo/inativo e observando o mesmo aplicativo de teste, sem implementar ainda integrações avançadas de P3.

**Cenários de aceite**:

1. **Dado** que o utilitário está instalado ou disponível para execução, **quando** o usuário desativa o fix, **então** o sistema volta ao comportamento anterior ao fix para os mesmos cenários cobertos (salvo limitações documentadas).
2. **Dado** o utilitário desativado, **quando** o usuário reativa o fix, **então** o comportamento descrito em P1 volta a valer nos cenários de aceite.
3. **Dado** uma instalação com **opção no instalador** sobre ativar o fix ao concluir (ou equivalente), **quando** o utilizador termina a instalação e abre o utilitário pela primeira vez, **então** o estado de ativação **corresponde** à opção escolhida no instalador e fica sujeito à persistência descrita em RF-002.

---

### História de usuário 3 — Encerramento e recuperação (Prioridade: P3)

Como usuário, quero poder **encerrar o utilitário** (incluindo reinício da sessão ou do equipamento) sem deixar o sistema em estado inconsistente, para não depender de suporte técnico para “desfazer” o efeito do programa.

**Por que esta prioridade**: Reduz risco percebido e incidentes; complementa P1/P2.

**Teste independente**: Pode ser validado com roteiro de encerramento e nova sessão, verificando estado do sistema e do scroll sem exigir as demais histórias além do mínimo necessário.

**Cenários de aceite**:

1. **Dado** que o fix estava ativo, **quando** o usuário encerra o utilitário pelo fluxo oficial (por exemplo: sair do aplicativo), **então** não permanecem efeitos indesejados documentados como fora de escopo de “limpeza” (comportamento a detalhar no plano sem violar esta especificação).
2. **Dado** um reinício do Windows após uso do utilitário, **quando** o usuário inicia nova sessão, **então** o sistema permanece utilizável, a **preferência de ativação persistida** é aplicada (último estado ligado/desligado) e o comportamento do scroll segue as regras de P2 e P1 conforme essa preferência.

---

### História de usuário 4 — Instância única e aviso de arranque (Prioridade: P2)

Como utilizador, quero que **não existam duas instâncias** do utilitário ao mesmo tempo e que, se eu tentar abrir de novo, a **janela já em execução** (mesmo estando **só na bandeja do sistema**) volte **visível** e mostre **configurações**, e quero ser **avisado em todo arranque** de que o programa iniciou e está na bandeja, para não achar que “nada aconteceu”.

**Por que esta prioridade**: Evita confusão, duplicidade de processos e perda de controlo quando o programa inicia apenas no ícone da área de notificação.

**Teste independente**: Pode ser validado com duplo clique no atalho enquanto o programa já corre (com e sem janela minimizada à bandeja) e observando o aviso em arranques com início na bandeja, sem depender de P1 além do mínimo para ter uma aba de configurações identificável.

**Cenários de aceite**:

1. **Dado** que **já existe** uma instância do utilitário em execução, **quando** o utilizador inicia o aplicativo **novamente** (atalho, executável ou outro mecanismo de arranque suportado), **então** **não** é criada uma segunda instância e a instância existente é **ativada**.
2. **Dado** que a instância existente está apenas na **bandeja do sistema** (sem janela principal visível), **quando** ocorre a segunda tentativa de arranque conforme o cenário anterior, **então** a janela principal é **restaurada**, passa a **primeiro plano** e fica **visível na área de trabalho**, e a interface apresenta a **aba de configurações** (ou equivalente nomeado no plano).
3. **Dado** um arranque do aplicativo em que **não** haja janela principal visível de imediato (por exemplo apenas na bandeja), **quando** o arranque completa, **então** o utilizador recebe um **aviso observável** (por exemplo balão ou notificação) de que o programa foi **iniciado** e está **disponível na bandeja do sistema**, em conformidade com RF-012 e RF-009.

---

### Casos extremos

- **Aplicativos ou cenários fora da lista de inclusão atual** (não configurados pelo utilizador): não há requisito de correção do scroll; o comportamento pode ser o do sistema sem o fix.
- **Lista vazia, limite de entradas atingido ou entrada inválida**: o produto DEVE comportar-se de forma **previsível e comunicada** (mensagens ou estados claros), conforme plano; não deve corromper dados nem deixar o fix num estado ambíguo quando isso for verificável.
- O que ocorre quando o usuário **não tem privilégios administrativos** necessários para o mecanismo escolhido no plano? O utilitário deve falhar de forma clara e segura, sem corromper configuração do sistema.
- **Conflito com outro software** que também altera entrada ou scroll: o produto **notifica** o utilizador quando **detectar** conflito relevante (critérios no plano); **não** impõe precedência automática entre os dois — a resolução é do utilizador (por exemplo: desativar temporariamente um dos programas). Orientações podem constar da ajuda ou documentação embutida.
- O que ocorre em **encerramento abrupto** (fim de processo, desligamento)? O estado do sistema deve permanecer recuperável conforme P3 dentro do que for razoável para a stack.
- **Preferência persistida ilegível ou corrompida**: o produto DEVE adotar um **estado seguro padrão** (definido no plano, tipicamente fix desligado) e comunicar de forma clara, sem travar a sessão.
- **Múltiplos monitores**: o comportamento por monitor e por janela em foco deve ser declarado no plano (cenários de teste). **Dispositivos de entrada**: no MVP, **roda do mouse** e **touchpad** para scroll vertical estão no escopo com **mesma regra** quando possível; outros dispositivos ou caminhos de entrada ficam **fora de escopo** ou como **melhor esforço** apenas se o plano assim documentar.
- **Scroll horizontal** (incluindo roda basculada ou gestos): **fora do escopo do MVP**; não há requisito de correção nesta versão.
- **Distribuição sem instalador** (por exemplo, pacote portátil) ou **sem** opção de ativação na instalação: o estado inicial antes da primeira preferência gravada segue **regra documentada no plano** (e na ajuda, se aplicável).
- **Sistema operacional não suportado** (por exemplo **Windows 10** ou anterior): o produto **não** integra o MVP; o utilizador DEVE ser informado de que o escopo suportado é **Windows 11** (mensagem na instalação, ao executar ou na documentação — detalhe no plano), sem prometer correção do scroll nesses ambientes.
- **Idioma de exibição do Windows** diferente de pt-BR: o MVP **não** exige localização da UI ao idioma do sistema; a experiência do produto permanece em **português (Brasil)** conforme RF-009.
- **Segunda execução durante o arranque da primeira**: o comportamento de **instância única** (RF-011) deve ser satisfeito; o plano pode definir o intervalo de tolerância se houver janela de corrida técnica.

## Requisitos *(obrigatório)*

### Requisitos funcionais

- **RF-001**: O produto DEVE permitir ao utilizador **gerir a lista de inclusão pela interface** no MVP: **adicionar e remover** aplicativos e/ou cenários de janela cobertos, **dentro de limites** (quantidade, identificação, formatos válidos) definidos no **plano**. O produto DEVE **persistir** essa lista entre sessões, salvo falha tratada conforme casos extremos. O produto DEVE expor comportamento de scroll **vertical** **previsível** **somente** para as entradas **atualmente** presentes nessa lista, aplicável à **roda do mouse** e ao **scroll vertical via touchpad** com a **mesma regra** de normalização quando o mecanismo o permitir; **divergências e limites** por piloto ou hardware ficam no **plano**. **Scroll horizontal** não faz parte do MVP. Fora da lista de inclusão, não há requisito de correção nesta versão. A verificação combina cenários de aceite, roteiro de testes e a matriz mínima ou exemplos referenciados no plano.
- **RF-002**: O produto DEVE permitir ao usuário **ativar e desativar** o efeito do fix sem exigir reinstalação do sistema operacional. A **última preferência** de ativação (ligado/desligado) DEVE **persistir entre reinícios do Windows** e ser **restaurada** ao iniciar nova sessão, salvo falha tratada conforme casos extremos. Quando existir **instalador** com opção explícita (por exemplo, ativar o fix ao concluir a instalação), a **primeira** preferência efetiva DEVE **alinhar-se** a essa escolha e ser persistida como estado inicial. Quando não houver instalador ou essa opção, o estado inicial antes da primeira gravação segue o **plano** (caso extremo correspondente).
- **RF-003**: O produto DEVE documentar ou expor de forma compreensível **o que muda** no comportamento do scroll quando o fix está ligado (por exemplo: texto curto na interface ou ajuda embutida), **em português (Brasil)** conforme RF-009, alinhado à constituição do projeto.
- **RF-004**: O produto DEVE ser projetado para **impacto mínimo** no sistema quando inativo ou após encerramento, conforme princípios de constituição; detalhes de mecanismo ficam no plano técnico.
- **RF-005**: O produto DEVE tratar falhas de forma **segura**: mensagens ou estados que não deixem o usuário sem saber se o fix está ativo ou não, quando isso for verificável na interface ou por comportamento observável.
- **RF-006**: O produto DEVE ser **verificável**: os requisitos desta especificação devem poder ser validados por roteiros de teste manuais ou automatizados definidos nas fases posteriores, sem depender de detalhes de implementação neste documento.
- **RF-007**: Quando **detectar** conflito relevante com outro software que também altere entrada ou scroll, o produto DEVE **notificar** o utilizador de forma compreensível. **Não** DEVE impor precedência automática entre o fix deste produto e o software externo; a desativação ou ajuste fica a cargo do utilizador, com apoio da ajuda quando aplicável. O que conta como “detecção” e “relevante” é definido no plano para fins de teste.
- **RF-008**: No MVP, o produto DEVE **suportar apenas Windows 11** como sistema operacional alvo. **Windows 10** e versões anteriores estão **fora de escopo** nesta versão; o utilizador DEVE poder identificar esse requisito (instalador, arranque da aplicação ou documentação embutida — detalhe no plano). Edições do Windows 11 (por exemplo Home vs Pro), arquitetura (por exemplo **64 bits**) e build mínimo são fixados no **plano**.
- **RF-009**: No MVP, a **interface de utilizador**, **mensagens** (incluindo notificações de conflito e erros tratáveis), **ajuda ou documentação embutida** e **textos do instalador** relacionados à experiência do produto DEVEM estar em **português (Brasil)**. **Outros idiomas** não fazem parte do escopo desta versão; localização adicional é **versão futura** salvo decisão explícita em documento de produto.
- **RF-010**: No MVP, o produto **NÃO DEVE** recolher nem transmitir **telemetria**, **análise de uso** ou dados equivalentes sobre o comportamento do utilizador para **destinos externos** (rede, nuvem ou serviços de terceiros) por este fim. **Configurações e preferências** tratadas nesta especificação permanecem **locais** à máquina, salvo ação explícita do utilizador (por exemplo exportar ou copiar). Uma **verificação opcional de atualizações** do produto, se existir, **NÃO** pode servir de veículo para telemetria ou análise de uso; o comportamento exato fica no **plano**, em conformidade com este requisito.
- **RF-011**: O produto DEVE garantir **no máximo uma instância** do aplicativo em execução **por utilizador na sessão** (instância única). Quando o utilizador iniciar o programa e **já existir** uma instância em execução, o arranque **não** DEVE criar um segundo processo de aplicação; a instância existente DEVE ser **ativada**: a janela principal DEVE ser **restaurada** se estiver apenas na **bandeja do sistema** (área de notificação), DEVE passar a **primeiro plano** e DEVE ficar **visível na área de trabalho**, e a interface DEVE apresentar a **aba de configurações** (ou o painel equivalente definido no plano). O mecanismo de exclusão mútua ou sinalização entre processos fica no **plano**, sem alterar o resultado observável para o utilizador.
- **RF-012**: Em **cada** arranque do aplicativo, o utilizador DEVE receber um **aviso ou indicação observável** de que o programa foi **iniciado** e que permanece **acessível na bandeja do sistema** (ícone na área de notificação). Quando o arranque **não** apresentar de imediato a janela principal visível (por exemplo início apenas na bandeja), esse feedback **não** pode ser omitido: formas típicas incluem **balão** junto ao ícone, **notificação** do Windows ou equivalente **compatível com Windows 11**, em **português (Brasil)** conforme RF-009. Quando a janela principal já estiver **visível** desde o arranque, o mesmo requisito de clareza pode ser satisfeito pela **própria janela** (por exemplo título ou mensagem inicial), sem obrigatoriedade de balão redundante — o **plano** define a combinação para cada fluxo de arranque. O objetivo é eliminar a perceção de arranque “silencioso” sem qualquer feedback.

### Entidades principais *(dados / configuração)*

- **Lista de inclusão (configurável)**: Conjunto de aplicativos e/ou cenários de janela em que o fix de **scroll vertical** se aplica; **gerido pelo utilizador na interface** no MVP, com **persistência** entre sessões; **limites** e regras de identificação no plano; matriz ou exemplos de teste de referência vinculados ao plano.
- **Preferência de ativação**: Representa se o fix está ligado ou desligado; **persiste entre reinícios do Windows** conforme RF-002. A **primeira** definição pode originar-se da **opção do instalador**, quando existir.
- **Perfil de comportamento** *(se necessário para o MVP)*: Agrupa parâmetros que definem “quanto” ou “como” o scroll é normalizado, sem nomear tecnologias nesta especificação.

## Critérios de sucesso *(obrigatório)*

### Resultados mensuráveis

- **CS-001**: Em testes com roteiro acordado, **100%** dos cenários de aceite prioritários (P1) passam em duas execuções consecutivas em **ambiente de referência Windows 11** descrito no plano.
- **CS-002**: Usuários de validação (ou o próprio autor, em time reduzido) conseguem alternar fix ativo/inativo e observar diferença **em menos de 1 minuto** após leitura de instruções curtas (quickstart) **em português (Brasil)**.
- **CS-003**: Após encerramento normal do utilitário, **nenhum** sintoma crítico documentado na lista de verificação de regressão (por exemplo: scroll irreversível ou perda de controle do ponteiro) nos cenários cobertos pelo roteiro.
- **CS-004**: Redação desta especificação permanece **agnóstica a framework e linguagem** em critérios de sucesso; métricas acima são verificáveis por observação ou checklist, não por medição de código.
- **CS-005**: Após **reinício do Windows** com preferência persistida em **ligado**, o usuário obtém o comportamento de P1 na lista de inclusão **sem** precisar ativar o fix manualmente de novo (salvo falha documentada).
- **CS-006**: Em roteiro de validação em **Windows 11**, em **100%** das execuções de arranque cobertas pelo teste (incluindo arranque com início apenas na bandeja), o utilizador identifica **sem ambiguidade** o **aviso** de que o programa está em execução na bandeja do sistema, conforme RF-012; em **100%** das tentativas de **segunda execução** com instância já ativa, **não** surge uma segunda instância e a janela existente cumpre RF-011 (incluindo abertura da aba de configurações).

## Premissas

- O público-alvo do MVP é **Windows 11** em configuração típica de desktop; **Windows 10** e outras plataformas ou versões estão fora de escopo nesta versão salvo documento de produto posterior.
- O repositório prioriza implementação alinhada à **constituição** do projeto; a escolha exata de API, runtime ou driver é **premissa de plano**, não desta especificação.
- “Comportamento previsível” será detalhado em **números ou listas de verificação** no plano ou em clarificações (por exemplo: linhas por detente, direção), mantendo esta especificação em linguagem de resultado para o usuário.
- Usuários aceitam que alguns aplicativos ou jogos em **tela cheia exclusiva** possam ter limitações documentadas na fase de plano ou clarificação.
- No MVP, a **lista de inclusão** é **configurável pelo utilizador** na interface; o **plano** define limites, identificação de entradas e a **matriz mínima de validação** (ou conjunto de exemplos) para testes de release. Comportamentos fora desses limites podem ser recusados com mensagem clara, sem violar RF-001.
- **Scroll horizontal** será tratado apenas em **versão futura**, fora do escopo funcional do MVP descrito aqui.
- No MVP, o comportamento previsível do **scroll vertical** aplica-se à **roda do mouse** e ao **touchpad** (mesma regra quando possível); pormenores e exceções no plano.
- Quando o fluxo de entrega incluir **instalador** com opção de ativação, essa escolha estabelece a **preferência inicial** alinhada a RF-002; outros fluxos de entrega seguem o plano.
- A experiência textual do MVP é **monolíngue em português (Brasil)**; não há requisito de adaptar rótulos ao idioma de exibição do Windows nesta versão (ver RF-009).
- **Privacidade e rede**: no MVP não há **telemetria** nem **análise de uso** transmitida para fora da máquina (ver RF-010); o plano detalha o que é permitido sem violar esse requisito (por exemplo atualizações opcionais).
- **Instância única e aviso de arranque** são obrigatórios no MVP (RF-011, RF-012); detalhes de implementação e texto exato do aviso ficam no plano, com textos em **pt-BR**.
