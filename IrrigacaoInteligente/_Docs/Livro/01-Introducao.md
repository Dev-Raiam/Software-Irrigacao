# Capítulo 1 — Introdução

## 1.1 O que é o IrrigacaoInteligente

O **IrrigacaoInteligente** é um *serviço de borda* (edge service) construído como um **.NET Worker Service**. Ele roda diretamente no equipamento de campo — tipicamente um **CLP** ou **Raspberry Pi** rodando Linux ARM64 — instalado próximo às bombas, válvulas e sensores de uma operação de irrigação.

Sua função central é ser a **ponte entre a nuvem e o hardware**:

- Recebe **comandos** vindos da nuvem (ligar bomba, abrir válvula, ajustar frequência) via **MQTT**.
- Traduz esses comandos para o **hardware de campo**, comunicando-se por **Modbus** e por um broker MQTT local.
- Coleta **telemetria** dos sensores (pressão, nível, temperatura, corrente, etc.).
- Mantém **credenciais e estado** localmente, de forma segura, mesmo com conectividade instável.

## 1.2 Que problema ele resolve

Operações de irrigação modernas precisam ser controladas e monitoradas remotamente, mas o ambiente de campo apresenta desafios:

- **Conectividade intermitente:** a internet no campo pode cair; o sistema precisa continuar operando localmente.
- **Hardware heterogêneo:** bombas, inversores de frequência, válvulas solenoide e proporcionais, e diversos sensores industriais que falam Modbus.
- **Segurança:** credenciais de acesso à nuvem precisam ficar protegidas no dispositivo.
- **Latência:** comandos críticos de atuação devem chegar ao hardware com o mínimo de atraso.

O IrrigacaoInteligente resolve isso atuando como um **agente local inteligente**: a nuvem conversa com ele, e ele conversa com o hardware — desacoplando a complexidade do campo da camada de aplicação na nuvem.

## 1.3 Público-alvo deste livro

- **Equipe de produto / gestão:** para entender o propósito, o valor e os fluxos de negócio (Parte I).
- **Desenvolvedores:** para entender a arquitetura, o código e como estendê-lo (Partes II e III).
- **Operação / DevOps:** para configurar, publicar e monitorar o serviço (Parte IV).

## 1.4 Visão de 30 segundos

```
[ Nuvem / API ]  --MQTT (broker remoto)-->  [ IrrigacaoInteligente (borda) ]  --Modbus / MQTT local-->  [ Hardware de campo ]
        ^                                                |
        |                                                v
        +----------------- Telemetria ------------------- (sensores)
```

A nuvem envia comandos; o serviço de borda os executa no hardware e devolve telemetria. Tudo de forma resiliente, segura e local.

---

Próximo: [Capítulo 2 — Visão Geral do Produto](02-Visao-Geral-do-Produto.md)
