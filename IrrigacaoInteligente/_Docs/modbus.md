Boa — agora você chegou na pergunta **certa de verdade** 👏
Aqui é onde muita gente se confunde em Modbus.

Vou te mostrar **direto e prático**, sem enrolação:

---

# 🧠 Mapeamento REAL das funções do NModbus

Cada função lê um **tipo específico de “porta lógica”** do Modbus:

---

## 🔵 `ReadCoilsAsync` / `ReadCoilsRegistersAsync`

```csharp
bool[] coils = await master.ReadCoilsAsync(slave, start, quantidade);
```

✔ Tipo: **Coils (0xxxx)**
✔ Retorno: `bool[]`

👉 Representa:

* Saídas digitais (DO)
* Flags internas

💡 Exemplo:

* Válvula ligada/desligada
* Motor ON/OFF

---

## 🟢 `ReadInputsAsync`

```csharp
bool[] inputs = await master.ReadInputsAsync(slave, start, quantidade);
```

✔ Tipo: **Discrete Inputs (1xxxx)**
✔ Retorno: `bool[]`

👉 Representa:

* Entradas digitais (DI)

💡 Exemplo:

* Botão pressionado
* Sensor ligado/desligado

---

## 🟡 `ReadHoldingRegistersAsync`

```csharp
ushort[] regs = await master.ReadHoldingRegistersAsync(slave, start, quantidade);
```

✔ Tipo: **Holding Registers (4xxxx)**
✔ Retorno: `ushort[]`

👉 Representa:

* Valores numéricos
* Configurações
* Variáveis internas

💡 Exemplo:

* Temperatura (convertida)
* Setpoint
* Velocidade

---

## 🟠 `ReadInputRegistersAsync`

```csharp
ushort[] regs = await master.ReadInputRegistersAsync(slave, start, quantidade);
```

✔ Tipo: **Input Registers (3xxxx)**
✔ Retorno: `ushort[]`

👉 Representa:

* Sensores (somente leitura)

💡 Exemplo:

* Temperatura crua
* Pressão
* Corrente

---

# 🔥 Tabela que você precisa decorar

| Função               | Tipo Modbus     | Endereço | Tipo C#    | Pode escrever? |
| -------------------- | --------------- | -------- | ---------- | -------------- |
| ReadCoils            | Coils           | 0xxxx    | `bool[]`   | ✔              |
| ReadInputs           | Discrete Inputs | 1xxxx    | `bool[]`   | ❌              |
| ReadHoldingRegisters | Holding         | 4xxxx    | `ushort[]` | ✔              |
| ReadInputRegisters   | Input Reg       | 3xxxx    | `ushort[]` | ❌              |

---

# ⚠️ Tradução para “portas físicas”

| Porta física     | Onde normalmente está    |
| ---------------- | ------------------------ |
| Saída digital    | Coils (0xxxx)            |
| Entrada digital  | Discrete Inputs (1xxxx)  |
| Sensor analógico | Input Register (3xxxx)   |
| Parâmetro/config | Holding Register (4xxxx) |

---

# 💣 Pegadinha MUITO comum

👉 Nem sempre o fabricante segue isso!

Exemplo real:

| Coisa           | Pode estar em             |
| --------------- | ------------------------- |
| Saída digital   | Coil OU Holding           |
| Entrada digital | Input OU Holding          |
| Sensor          | Input Register OU Holding |

👉 Por isso:
📄 **mapa Modbus manda em tudo**

---

# 🧠 Sobre seu código (ligando tudo)

Você usou:

```csharp
ReadHoldingRegistersAsync → valores numéricos
ReadCoilsRegistersAsync → saídas digitais
```

✔ Está correto
✔ Está padrão industrial

---

# ✅ Regra de ouro (leva isso pra vida)

👉 Função define o tipo de dado
👉 Endereço define QUAL variável
👉 Dispositivo define o significado

---

Se quiser, no próximo passo eu posso te montar um **mapa mental estilo PLC** tipo:

* Entrada física → endereço Modbus
* Saída física → endereço
* Como padronizar isso no seu código

isso ajuda MUITO quando o projeto cresce 🔥
