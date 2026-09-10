# Atividade — AULA 06

## Síncrono ou Assíncrono?

*Análise de fluxos de comunicação entre serviços — Arquitetura de Aplicações Web*

## 🎯 MISSÃO

Vocês são os arquitetos dos 4 fluxos abaixo. Para CADA cenário:

- Decidam o estilo de comunicação: síncrono (request/response), assíncrono (fila/evento) ou API Gateway/BFF
- Desenhem o fluxo com caixas (serviços) e setas (chamadas/mensagens) no espaço indicado
- Justifiquem com pelo menos 2 fatores (urgência da resposta, tolerância a atraso, picos, falhas...)
- Apontem o principal risco da escolha de vocês

*⏱️ Tempo: 25 minutos  |  👥 Formato: em duplas  |  Não existe resposta única — o que vale é a justificativa.*

> **Nomes:** Gabriel Luiz Sa Silva, Igor Nobrega Moreira   **Turma:** Arquiteturas de aplicações Web 702   **Data:** 10 / 09 / 2026

## CENÁRIO 01 — PagFácil — aprovar ou negar AGORA

No checkout do PagFácil, ao clicar em “Pagar”, o serviço de Pagamentos precisa consultar o saldo/limite do cliente no serviço de Contas — e a resposta define se a venda acontece neste exato momento.

- O cliente está na tela, esperando o resultado da compra
- Sem a resposta de Contas, não há decisão possível: aprovar às cegas é proibido
- Tempo de resposta do serviço de Contas: ~80 ms em condições normais

**Sua análise:**

1. Estilo recomendado:   x Síncrono      ☐ Assíncrono (fila/evento)      ☐ API Gateway/BFF

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

Cliente - HTTP POST /pagar -> Serviço de Pagamentos - Chamada HTTP/gRPC Síncrona -> Serviço de Contas
Cliente <- Sucesso/Erro  <- Serviço de Pagamentos <- Confirmação do Saldo  <- Serviço de Contas

3. Justificativa (mínimo 2 fatores):
Como a aprovação depende 100% da verificação do saldo e o sistema não pode aprovar as cegas, o ciclo de requisição do usuário precisa bloquear até obter esse dado. O processamento assíncrono criaria um estado incerto no checkout que a regra de negócio não permite.

Um tempo de resposta de 80 ms do serviço de Contas está dentro do limite aceitável para chamadas síncronas via HTTP/REST ou gRPC, mantendo uma boa UX no frontend sem estourar o tempo de espera do cliente.

4. Principal risco da escolha:

Se o serviço de Contas cair ou der timeout, o serviço de Pagamentos paralisa e o checkout falha totalmente.

## CENÁRIO 02 — CadastraJá — o e-mail de boas-vindas

Após criar a conta no CadastraJá, o sistema envia um e-mail de boas-vindas. O provedor de e-mail às vezes demora 8 segundos para responder e falha em 2% das tentativas.

- O usuário quer começar a usar o app imediatamente após o cadastro
- O e-mail chegar 1 minuto depois não incomoda ninguém
- Se o provedor falhar, o envio deve ser tentado de novo — sem o usuário perceber

**Sua análise:**

1. Estilo recomendado:   ☐ Síncrono      x Assíncrono (fila/evento)      ☐ API Gateway/BFF

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

Usuário -HTTP POST /cadastro -> Serviço de Cadastro - Publica Evento/Mensagem -> Fila / Message Broker
Usuário <-  Resposta 201 Created  - Serviço de Cadastro   

3. Justificativa (mínimo 2 fatores):

O provedor externo demora até 8 segundos para responder. Se a gente usasse chamada síncrona, a tela do usuário ia ficar carregando por um bom tempo. Com a fila, o cadastro finaliza na hora e o usuário já entra no app sem ter que esperar.

O provedor falha em 2% das vezes. Jogando a mensagem para uma fila, o Worker de notificação pode tentar reenviar o e-mail em background se der algum erro, sem que a conta do usuário trave ou dê mensagem de falha.

4. Principal risco da escolha:

Se o worker cair ou a fila acumular muitas mensagens, o e-mail pode demorar bem mais do que 1 minuto para chegar. Além disso, se o e-mail falhar de vez (ir para a DLQ), o sistema principal nem vai ficar sabendo na hora se não tiver um monitoramento em cima.

## CENÁRIO 03 — MegaMarket — baixa de estoque nos picos

No marketplace MegaMarket, cada venda gera uma baixa no serviço de Estoque. Nas grandes promoções o tráfego sobe 10x e o Estoque não dá conta de responder na velocidade das vendas.

- Atraso de alguns segundos na baixa é aceitável
- PERDER uma baixa de estoque não é aceitável (gera venda sem produto)
- O checkout não pode ficar lento nem cair porque o Estoque está sobrecarregado

**Sua análise:**

1. Estilo recomendado:   ☐ Síncrono      x Assíncrono (fila/evento)      ☐ API Gateway/BFF

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

Cliente - Finaliza Venda -> Serviço de Vendas- Publica "ItemVendido" -> Fila / Message Broker
Cliente <-  Compra Aprovada  - Serviço de Vendas  

3. Justificativa (mínimo 2 fatores):

Com um aumento de 10x no tráfego, o serviço de Estoque não aguenta a pressão síncrona. Usar uma fila faz o buffer de todas as mensagens: a venda finaliza rápido no frontend e o Estoque vai processando as baixas no ritmo dele, sem travar o checkout.

O enunciado diz que perder baixa é inaceitável. A fila armazena os eventos de forma persistente. Mesmo se o serviço de Estoque oscilar ou cair por uns segundos, as mensagens continuam salvas para serem processadas assim que ele voltar.

4. Principal risco da escolha:

Como a baixa demora uns segundos para acontecer, dois clientes podem comprar a última unidade do mesmo produto ao mesmo tempo. Se o estoque esgotar enquanto a mensagem ainda estiver na fila, o sistema aceita a venda e gera o furo.

## CENÁRIO 04 — AppBanco — uma tela, cinco serviços

A tela inicial do AppBanco mostra saldo, fatura do cartão, investimentos, empréstimos e cashback — dados de 5 serviços diferentes. O time mobile reclama: são 5 chamadas, 5 formatos de resposta e 5 pontos de falha em cada abertura do app.

- A tela precisa abrir rápido, inclusive em redes móveis ruins
- Cada serviço tem equipe, formato e autenticação próprios
- Amanhã nasce a versão web, que precisa de MAIS dados que a mobile

**Sua análise:**

1. Estilo recomendado:   ☐ Síncrono      ☐ Assíncrono (fila/evento)      x API Gateway/BFF

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

                  +-> Serviço de Saldo
                  |-> Serviço de Cartão
App Mobile - 1 chamada HTTP -> BFF Mobile / Gateway -> Serviço de Investimentos
                                   Agrega dados em paralelo -> Serviço de Empréstimos
                                                           +-> Serviço de Cashback

3. Justificativa (mínimo 2 fatores):

Em vez do app fazer 5 requisições separadas, ele faz só 1 chamada pro BFF. O BFF busca os 5 serviços por trás na rede interna rápida, junta tudo em um JSON só e manda limpo pro celular.

 O BFF (Backend For Frontend) permite criar uma camada ideal para cada cliente. O time mobile pede só o essencial para abrir rápido, e amanhã o time web pode criar o BFF Web para buscar mais dados sem ter que alterar a API dos microserviços.

4. Principal risco da escolha:

Se o BFF cair ou ficar lento, a tela inicial inteira do app quebra.

## DESAFIO

1. Escolha um cenário em que vocês indicaram ASSÍNCRONO. Os brokers de mensagens costumam garantir entrega “pelo menos uma vez” — ou seja, a MESMA mensagem pode chegar duas vezes. O que aconteceria no seu fluxo? Como o consumidor deveria se proteger?

Se a mesma mensagem de venda for entregue duas vezes, o serviço de Estoque vai processar o evento em duplicidade. Isso causará uma baixa dupla no estoque para uma única compra.
O consumidor precisa implementar Idempotência.
