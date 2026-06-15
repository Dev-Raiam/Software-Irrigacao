# Capítulo 2 — Visão Geral do Produto

## 2.1 O que o sistema faz

Em alto nível, o IrrigacaoInteligente entrega quatro capacidades:

- **Controle remoto de atuadores:** ligar/desligar bombas e moto-bombas, acionar/desligar inversores de frequência, abrir/fechar válvulas solenoide e ajustar válvulas proporcionais.
- **Leitura de sensores:** corrente, tensão, pressão, nível, temperatura, umidade, distância, pH e posição.
- **Coleta e publicação de telemetria:** leitura periódica via Modbus e disponibilização dos dados.
- **Gestão local de credenciais e estado:** mantém criptografadas as credenciais necessárias para autenticar na nuvem e sincronizar a configuração dos controladores.

## 2.2 Cenário de uso no campo

1. O equipamento de borda é instalado no campo e ligado.
2. Na primeira execução, o sistema aguarda **credenciais** (configuradas via endpoint HTTP) — é o "gate de prontidão".
3. Com credenciais válidas, o serviço se conecta aos **brokers MQTT** (local, junto ao hardware; e remoto, na nuvem).
4. O operador, pela aplicação na nuvem, envia um comando (ex.: "ligar bomba X").
5. O comando trafega pelo broker remoto, é interpretado pelo serviço de borda e reencaminhado ao hardware via broker local.
6. Sensores são lidos periodicamente e a telemetria fica disponível para a operação.

## 2.3 Valor entregue

- **Resiliência:** opera mesmo com conectividade instável; o estado fica local.
- **Segurança:** credenciais protegidas com Data Protection e criptografia, autenticação JWT automática com a nuvem.
- **Baixa latência de atuação:** o serviço fica fisicamente próximo ao hardware.
- **Extensibilidade:** novos comandos e sensores são adicionados como "fatias" verticais isoladas (ver Capítulo 16).
- **Observabilidade:** logging estruturado com Serilog e telemetria persistida.

## 2.4 Fronteiras do sistema

O IrrigacaoInteligente **não é** a aplicação de nuvem nem a interface do usuário final. Ele é o **componente de borda**. Ele depende de:

- Uma **API de nuvem** para autenticação e sincronização de controladores (`Toolbox` APIs).
- Um **broker MQTT remoto** para receber comandos.
- Um **broker MQTT local** e/ou barramento **Modbus** para falar com o hardware.

| Responsabilidade | Quem faz |
| ---------------- | -------- |
| Interface do usuário e regras de negócio de alto nível | Aplicação de nuvem |
| Roteamento de comandos para o hardware | **IrrigacaoInteligente** |
| Leitura física de sensores / acionamento | **IrrigacaoInteligente** + hardware |
| Persistência de longo prazo / analytics | Aplicação de nuvem |

---

Anterior: [Capítulo 1 — Introdução](01-Introducao.md) · Próximo: [Capítulo 3 — Conceitos do Domínio](03-Conceitos-do-Dominio.md)
