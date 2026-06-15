# IrrigacaoInteligente — O Livro

> Documentação em formato de livro do sistema **IrrigacaoInteligente**: um serviço de borda (.NET Worker) que faz a ponte entre a nuvem e o hardware de campo de irrigação e automação.

Este livro combina **visão de negócio** (o que o sistema faz e por quê) com **documentação técnica** (como é construído e como operá-lo). Cada capítulo é um arquivo Markdown independente nesta pasta.

---

## Sumário

### Parte I — Visão de Negócio
- [Capítulo 1 — Introdução](01-Introducao.md)
- [Capítulo 2 — Visão Geral do Produto](02-Visao-Geral-do-Produto.md)
- [Capítulo 3 — Conceitos do Domínio](03-Conceitos-do-Dominio.md)

### Parte II — Arquitetura
- [Capítulo 4 — Visão Arquitetural](04-Visao-Arquitetural.md)
- [Capítulo 5 — Estrutura do Projeto](05-Estrutura-do-Projeto.md)
- [Capítulo 6 — Modelo de Domínio](06-Modelo-de-Dominio.md)

### Parte III — Componentes Técnicos
- [Capítulo 7 — Inicialização e Gates de Prontidão](07-Inicializacao-e-Gates.md)
- [Capítulo 8 — Comunicação MQTT](08-Comunicacao-MQTT.md)
- [Capítulo 9 — Telemetria e Modbus](09-Telemetria-e-Modbus.md)
- [Capítulo 10 — Hardware: Comandos e Sensores](10-Hardware-Comandos-e-Sensores.md)
- [Capítulo 11 — Persistência e Sincronização](11-Persistencia-e-Sincronizacao.md)
- [Capítulo 12 — Segurança](12-Seguranca.md)

### Parte IV — Operação
- [Capítulo 13 — Configuração e Execução](13-Configuracao-e-Execucao.md)
- [Capítulo 14 — Deploy](14-Deploy.md)
- [Capítulo 15 — Logs e Observabilidade](15-Logs-e-Observabilidade.md)
- [Capítulo 16 — Guia de Extensão](16-Guia-de-Extensao.md)

---

## Como ler este livro

- **Gestores / visão de produto:** Parte I é suficiente para entender propósito e valor.
- **Novos desenvolvedores:** leia as Partes I e II, depois aprofunde na Parte III conforme a área em que for atuar.
- **Operação / DevOps:** foque na Parte IV (configuração, deploy, logs).

> **Nota:** Este livro é derivado e expande o `Docs/README.md`. Quando o código evoluir, mantenha ambos sincronizados.
