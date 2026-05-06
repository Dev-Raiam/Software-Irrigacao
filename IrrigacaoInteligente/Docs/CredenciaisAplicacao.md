# Configurações da aplicação

## 🔧 Configurações técnicas (definidas no código/appsettings)

| Categoria | Configuração | Valor |
|---|---|---|
| **Banco** | Connection String | `Data Source=database.db` |
| **API Toolbox** | URL Base | `https://api.toolbox.app.br` |
| **API Toolbox** | Timeout | `30s` |
| **API Toolbox** | Tentativas de retry | `3` |
| **API Toolbox** | Delay entre retries | `2s` |
| **API Toolbox** | Accept header | `application/vnd.data.integration.v1+json` |
| **API Toolbox** | Endpoint de autenticação | `autenticacao/v1/autenticar-cliente` |
| **API Toolbox** | Endpoint de controladores | `/automacao/v1/paineis/{painelId}/controladores?status=todos` |
| **MQTT** | Broker Local | `localhost` |
| **MQTT** | Broker Remoto | `broker.freemqtt.com` |
| **MQTT** | Porta | `1883` |
| **MQTT** | Usuário Remoto | `freemqtt` |
| **MQTT** | Senha Remoto | `public` |
| **MQTT** | Tópico de comando | `comando/03800edb-8dff-4e2b-9ad8-00f0af1cdebf` |
| **HTTP Server** | URL do Kestrel | `http://0.0.0.0:4900` |
| **Rate Limit** | Política | `limite-tentativas` |
| **Rate Limit** | Limite concorrente | `5` |
| **Rate Limit** | Fila | `5` |
| **Logs** | Caminho dos arquivos | `Logs/log-.txt` |
| **Logs** | Retenção | `7 dias` (rolling diário) |
| **Logs** | Nível padrão | `Information` |

## 👤 Credenciais enviadas pelo usuário (via HTTP)

| Endpoint | Campo | Tipo | Obrigatório |
|---|---|---|---|
| **POST Credencial** ([AdicionarCredenciais](cci:2://file:///d:/Desenvolvimento/Backend/SoftwareIrrigacao/IrrigacaoInteligente/Features/Configuracao/ConfiguracaoSistema/AdicionarCredenciais.cs:5:0-27:1)) | `ContaId` | `Guid` | ✅ |
| **POST Credencial** | `PainelId` | `Guid` | ✅ |
| **POST Credencial** | `Integracao.Chave` | `string` | ✅ |
| **POST Credencial** | `Integracao.Segredo` | `string` | ✅ |
| **POST Credencial** | `Integracao.ContextoId` | `Guid` | ✅ |
| **POST AtualizarConta** ([AtualizarCredenciais](cci:2://file:///d:/Desenvolvimento/Backend/SoftwareIrrigacao/IrrigacaoInteligente/Features/Configuracao/ConfiguracaoSistema/AtualizarCredenciais.cs:5:0-12:1)) | `ContaId` | `Guid` | ✅ |
| **POST AtualizarConta** | `PainelId` | `Guid` | ✅ |

## 💡 Observações sobre dependências cruzadas

- O **`PainelId`** que o usuário envia **deveria** alimentar o tópico MQTT de comando (atualmente fixo em `03800edb-...`).
- A **`Chave` + `Segredo` + `ContextoId`** do usuário são usadas para chamar o endpoint hardcoded `autenticacao/v1/autenticar-cliente` e obter o JWT.
- O **`ContaId`** do usuário é injetado nos endpoints hardcoded da API Toolbox (ex: `/paineis?contaId={contaId}`).