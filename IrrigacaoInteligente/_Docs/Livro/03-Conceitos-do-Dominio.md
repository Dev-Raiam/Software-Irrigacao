# Capítulo 3 — Conceitos do Domínio

Este capítulo apresenta o vocabulário do sistema sem entrar no código. Esses termos aparecem em todo o livro e no próprio código-fonte.

## 3.1 Controlador

Um **Controlador** representa um equipamento de campo gerenciado (por exemplo, o CLP que comanda um conjunto de bombas e válvulas). Existe a noção de um **controlador master**, usado como referência para a leitura de telemetria via Modbus.

## 3.2 Dispositivo

Um **Dispositivo** é um elemento físico conectado ao controlador. Pode ser um **sensor** (entrada) ou um **atuador/comando** (saída). O sistema cataloga tipos de dispositivo por meio de um enum com códigos numéricos e descrições, por exemplo:

- **Sensores:** tensão, corrente, potência, frequência, nível, pressão, boia, posição.
- **Atuadores/comandos:** válvula solenoide, comando de partida, ativação, abertura, fechamento, velocidade, retrolavagem.

## 3.3 Porta

Uma **Porta** descreve o ponto de conexão física/lógica de um dispositivo no controlador. Comandos de atuação usam o **endereço lógico** da porta para saber onde escrever no hardware.

## 3.4 Comando

Um **Comando** é uma intenção de atuação enviada pela nuvem. Há dois grandes grupos:

- **Comandos digitais:** acionamentos liga/desliga (bomba, solenoide, moto-bomba).
- **Comandos analógicos:** valores contínuos (frequência de inversor, abertura de válvula proporcional).

Cada comando tem um *handler* dedicado que sabe como traduzi-lo para o hardware.

## 3.5 Telemetria

**Telemetria** é o conjunto de leituras coletadas do campo (valores de sensores e estados). É lida periodicamente do hardware e disponibilizada para a operação. Conceitos relacionados no código incluem `Telemetria`, `TelemetriaResposta`, `BlocoTelemetriaProtocolo` e `BlocoTelemetriaSinal`.

## 3.6 Credenciais e Estado da Aplicação

- **Credenciais da Aplicação:** dados sensíveis necessários para autenticar na nuvem; ficam criptografados localmente.
- **Estado da Aplicação:** o sistema só fica "pronto" quando há credenciais válidas. Esse conceito é implementado pelos **gates de prontidão** (ver Capítulo 7).

## 3.7 Unidades de Medida

O domínio define unidades padronizadas para as leituras: `KPa`, `PSI`, `Bar`, `MPa`, `Celsius`, `Kelvin`, `Fahrenheit`, `Metro`, `Centimetro`, `Percentual`, `Volt`, `Ampere`. Isso garante que a telemetria seja interpretada corretamente em toda a cadeia.

## 3.8 Glossário rápido

| Termo | Significado |
| ----- | ----------- |
| **Borda (edge)** | Computação executada perto do hardware, no campo |
| **Broker MQTT** | Servidor de mensagens publish/subscribe |
| **Modbus** | Protocolo industrial para falar com sensores/atuadores |
| **Gate de prontidão** | Mecanismo que bloqueia workers até o sistema estar pronto |
| **Master** | Controlador de referência para telemetria |

---

Anterior: [Capítulo 2 — Visão Geral do Produto](02-Visao-Geral-do-Produto.md) · Próximo: [Capítulo 4 — Visão Arquitetural](04-Visao-Arquitetural.md)
