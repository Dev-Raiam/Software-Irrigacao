# Padrão de ID de Correlação (Correlation ID)

## O que é?

O Padrão de ID de Correlação (ou Correlation ID) é uma estratégia fundamental em arquiteturas distribuídas e microsserviços para rastrear uma única solicitação à medida que ela atravessa múltiplos sistemas ou componentes independentes.

## Como funciona

### Geração
Quando uma requisição entra no sistema pela primeira vez, um identificador único (geralmente um UUID/GUID) é gerado.

### Propagação
Esse ID é passado adiante em cada chamada subsequente entre serviços, normalmente via:
- Cabeçalhos HTTP (como `X-Correlation-ID`)
- Metadados de mensagens em sistemas de mensageria
- Propriedades em filas (RabbitMQ, Azure Service Bus, etc.)

### Registro (Logging)
Cada serviço envolvido no fluxo inclui esse mesmo ID em seus próprios logs, permitindo a correlação posterior.

## Por que é essencial?

Sem esse padrão, depurar um erro em um ambiente complexo é quase impossível, pois os logs de cada serviço ficam isolados. Com o ID de Correlação, os desenvolvedores podem:

### Rastrear de ponta a ponta
Ver todo o caminho percorrido por uma solicitação específica através de múltiplos serviços.

### Identificar gargalos
Localizar exatamente em qual componente houve demora ou falha no processamento.

### Análise de causa raiz
Reunir registros dispersos de diferentes máquinas ou serviços para entender por que um erro ocorreu.

## Exemplos de uso prático

### Microserviços
Essencial para conectar logs de serviços de pedidos, pagamento e estoque em uma única transação de compra.

### Mensagens de erro (SharePoint/Microsoft)
Frequentemente, ao encontrar um erro em softwares como o SharePoint, um ID de correlação é exibido para que os administradores possam encontrar os detalhes técnicos específicos nos logs do servidor.

### Observabilidade
Ferramentas como o Azure Application Insights podem gerar e gerenciar esses IDs automaticamente.

---

# Padrões de Mensagens Assíncronas

Os padrões de mensagens assíncronas são fundamentais para criar sistemas escaláveis e resilientes, onde os componentes não precisam esperar uma resposta imediata para continuar operando.

## Principais Padrões

### 1. Fire-and-Forget (Disparar e Esquecer)
O remetente envia uma mensagem e não espera por nenhuma confirmação de processamento.

**Uso ideal:** Tarefas que podem ser processadas em segundo plano, como envio de e-mail de boas-vindas após cadastro.

### 2. Request-Reply (Requisição-Resposta Assíncrona)
Mesmo sendo assíncrono, o remetente precisa de uma resposta.

**Como funciona:**
- O remetente envia a mensagem com um ID de Correlação
- Indica um "canal de resposta" (callback queue)
- O destinatário processa a tarefa e envia o resultado para esse canal específico

### 3. Publish-Subscribe (Pub/Sub)
Uma mensagem é enviada para um "tópico" e distribuída para todos os interessados (assinantes) simultaneamente.

**Exemplo:** Quando um pedido é pago, o sistema de vendas publica um evento. Os sistemas de Logística, Nota Fiscal e Estoque recebem essa mesma informação e agem de forma independente.

### 4. Competing Consumers (Consumidores Concorrentes)
Várias instâncias de um serviço leem da mesma fila para processar mensagens em paralelo.

**Benefício:** Permite aumentar a velocidade de processamento apenas adicionando mais instâncias do consumidor.

### 5. Saga Pattern (Saga de Transações)
Utilizado para gerenciar transações que envolvem múltiplos microsserviços.

**Funcionamento:** Se uma etapa falha, o padrão coordena transações de compensação para desfazer as etapas anteriores e manter a consistência dos dados.

### 6. Message Bridge (Ponte de Mensagens)
Conecta sistemas diferentes que usam protocolos ou formatos de mensagem distintos, traduzindo a comunicação entre eles sem que precisem ser alterados.

## Vantagens dos Padrões Assíncronos

### Desacoplamento
Os serviços não precisam saber da existência ou do estado um do outro.

### Resiliência
Se um serviço cair, as mensagens ficam seguras na fila até que ele volte.

### Escalabilidade
Você pode processar picos de carga acumulando mensagens e processando-as conforme a capacidade disponível.

---

# Request-Reply Messaging

O padrão Request-Reply no mundo assíncrono é a solução para quando você precisa de uma resposta, mas não quer (ou não pode) manter uma conexão aberta esperando por ela.

## O Mecanismo

Para que isso funcione sem confusão, o padrão utiliza três elementos técnicos principais:

### Correlation ID (ID de Correlação)
O remetente anexa um identificador único na mensagem de envio. Quando a resposta volta, ela traz esse mesmo ID para que o remetente saiba a qual pedido aquela resposta se refere.

### Reply-To Address (Endereço de Retorno)
A mensagem original inclui um metadado indicando em qual fila ou tópico a resposta deve ser postada.

### Callback Queue (Fila de Resposta)
Uma fila temporária ou exclusiva onde o remetente fica "ouvindo" as respostas que chegam.

## Fluxo de Trabalho

1. **Serviço A (Requisitante)** gera um ID único, coloca-o no cabeçalho da mensagem e envia para a Fila de Requisição.
2. **Serviço B (Processador)** retira a mensagem da fila, executa o trabalho e cria uma nova mensagem com o resultado.
3. **Serviço B** copia o ID de Correlação para a nova mensagem e a envia para a Fila de Resposta indicada.
4. **Serviço A** recebe a mensagem, lê o ID e correlaciona com a ação original.

## Quando usar?

### Integração com sistemas legados
Quando você precisa consultar um dado em um sistema lento que não suporta conexões síncronas rápidas.

### Processamento pesado
Onde a resposta pode demorar segundos ou minutos (ex: gerar um relatório PDF complexo).

### Desacoplamento total
Quando você quer garantir que, mesmo se o serviço de resposta cair, o pedido original não se perca.

## Desafios

### Timeout
O requisitante precisa saber quanto tempo esperar antes de desistir da resposta.

### Estado
O serviço que pede a informação muitas vezes precisa manter um "estado" (em memória ou banco) para saber o que fazer quando a resposta finalmente chegar.

---

# Message Broker Patterns

Os padrões de Message Broker definem como as mensagens são roteadas, transformadas e entregues dentro de uma arquitetura.

## Padrões Essenciais

### 1. Point-to-Point (Ponto a Ponto)
É o modelo de fila (Queue) tradicional.

**Como funciona:** Uma mensagem é enviada para uma fila e consumida por apenas um destinatário. Se houver vários consumidores, o broker distribui as mensagens entre eles (geralmente via Round Robin).

**Uso:** Distribuição de carga de trabalho (Worker Queues).

### 2. Fan-out (Difusão)
O broker ignora critérios de filtragem e envia uma cópia da mensagem para todas as filas que estão conectadas a ele.

**Uso:** Quando você precisa que vários sistemas diferentes recebam a mesma informação instantaneamente (ex: uma atualização de preço que deve ir para o site, o app e o PDV físico).

### 3. Message Routing (Roteamento por Chave)
A mensagem é entregue a filas específicas com base em uma "Routing Key" (chave de roteamento) presente no cabeçalho.

**Uso:** Logs de sistema. Você pode enviar todos os logs para o broker, mas configurar uma fila para receber apenas erros de "Nível Crítico" e outra para receber "Avisos".

### 4. Competing Consumers (Consumidores Concorrentes)
Múltiplos consumidores leem da mesma fila simultaneamente para aumentar a vazão (throughput).

**Garantia:** O broker garante que cada mensagem seja processada por apenas um consumidor, evitando duplicidade de esforço.

### 5. Dead Letter Channel (Fila de Mensagens Mortas - DLQ)
Quando uma mensagem não pode ser entregue ou processada (após várias tentativas de erro), o broker a move para uma fila especial chamada DLQ.

**Importância:** Impede que uma mensagem "venenosa" trave o processamento de toda a fila e permite auditoria posterior para entender por que ela falhou.

### 6. Message Sequencer (Sequenciador)
Em sistemas distribuídos, as mensagens podem chegar fora de ordem. Este padrão usa um componente para reordenar as mensagens com base em um número de sequência antes de entregá-las ao destino final.

### 7. Content-Based Router (Roteador Baseado em Conteúdo)
O broker inspeciona o conteúdo da mensagem (o corpo do JSON/XML) para decidir para onde enviá-la.

**Exemplo:** Se o campo `pais` for "Brasil", envie para a fila de processamento da América Latina.

---

# Event-Driven Architecture

A Arquitetura Orientada a Eventos (EDA) é um modelo de design de software onde a comunicação entre os serviços é baseada na detecção, consumo e reação a eventos (mudanças de estado significativas).

Em vez de um serviço chamar o outro diretamente (o modelo tradicional "Comando"), os serviços apenas notificam que algo aconteceu.

## Componentes Core

### Produtores de Eventos
Captam uma mudança de estado (ex: "Item Adicionado ao Carrinho") e publicam um evento. Eles não sabem quem vai ler essa informação.

### Canais de Eventos (Event Bus/Broker)
A infraestrutura que transporta o evento (Kafka, RabbitMQ, AWS EventBridge).

### Consumidores de Eventos
Serviços que ficam "ouvindo" e reagem quando um evento do seu interesse acontece.

## Modelos de EDA

### Pub/Sub (Publish/Subscribe)
O modelo clássico onde o evento é enviado para todos os interessados.

### Event Streaming
Os eventos são gravados em logs contínuos e podem ser lidos em tempo real ou "rebobinados" (ex: Apache Kafka). É ideal para análises complexas e auditoria.

## Benefícios Reais

### Desacoplamento Extremo
O serviço de vendas não precisa saber que o serviço de marketing existe; ele apenas publica "Venda Concluída". O marketing decide por conta própria se quer reagir a isso.

### Escalabilidade Independente
Se o serviço de e-mail estiver lento, ele não trava a finalização da compra. As mensagens ficam acumuladas no broker até que ele consiga processá-las.

### Agilidade
É fácil adicionar novos recursos. Quer criar um sistema de recompensas? Basta criar um novo consumidor que ouça o evento de "Compra Concluída", sem alterar uma linha de código do sistema de vendas.

## Desafios (O que ninguém conta)

### Consistência Eventual
Como os serviços são assíncronos, os dados podem levar alguns milissegundos (ou segundos) para se propagarem por todo o sistema.

### Rastreabilidade
É aqui que o Correlation ID se torna obrigatório, pois seguir o fluxo de um evento entre 10 serviços diferentes sem um ID comum é impossível.

### Complexidade de Depuração
Entender a "causa raiz" exige boas ferramentas de observabilidade.

---

# Implementação Prática

## Exemplo em C# - Correlation ID

```csharp
public class CorrelationContext
{
    private static readonly AsyncLocal<string> _correlationId = new();
    
    public static string CorrelationId
    {
        get => _correlationId.Value ?? GenerateNewId();
        set => _correlationId.Value = value;
    }
    
    private static string GenerateNewId()
    {
        return Guid.NewGuid().ToString();
    }
}

// Middleware para ASP.NET Core
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }
        
        CorrelationContext.CorrelationId = correlationId;
        context.Response.Headers["X-Correlation-ID"] = correlationId;
        
        await _next(context);
    }
}
```

## Exemplo de Logging com Correlation ID

```csharp
public class CustomLogger : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, 
        TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        var message = formatter(state, exception);
        var correlationId = CorrelationContext.CorrelationId;
        
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] " +
                         $"[{logLevel}] [CID: {correlationId}] {message}");
    }
}
```

## Exemplo com MQTT

```csharp
public class MqttPublisher
{
    public async Task PublishAsync(string topic, object payload)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(JsonSerializer.Serialize(payload))
            .WithCorrelationData(Encoding.UTF8.GetBytes(CorrelationContext.CorrelationId))
            .Build();
            
        await _mqttClient.PublishAsync(message);
    }
}
```

---

## Conclusão

O ID de Correlação é o fio invisível que une microsserviços em arquiteturas distribuídas. Sem ele, a observabilidade e a depuração se tornam tarefas quase impossíveis. Quando combinado com padrões de mensagens assíncronas e arquitetura orientada a eventos, ele cria sistemas verdadeiramente resilientes, escaláveis e observáveis.

Lembre-se: em sistemas distribuídos, a capacidade de rastrear uma requisição através de múltiplos serviços não é um luxo - é uma necessidade fundamental para operação e manutenção eficazes.
