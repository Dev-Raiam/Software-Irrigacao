**Explicação das configurações do `CreateNamedPipe`:**

**Linha 50 - `PIPE_NAME`**
- Caminho do named pipe: `\\.\pipe\meu_pipe_comunicacao`

**Linha 51 - `win32pipe.PIPE_ACCESS_DUPLEX`**
- **DUPLEX**: Comunicação bidirecional (servidor e cliente podem ler e escrever)
- Alternativas: `PIPE_ACCESS_INBOUND` (só cliente escreve), `PIPE_ACCESS_OUTBOUND` (só cliente lê)

**Linha 52 - `win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_READMODE_MESSAGE | win32pipe.PIPE_WAIT`**
- **PIPE_TYPE_MESSAGE**: Pipe trata dados como mensagens (não como stream de bytes)
- **PIPE_READMODE_MESSAGE**: Leitura em modo mensagem (lê mensagem completa)
- **PIPE_WAIT**: Operações bloqueantes (espera até completar)

**Linha 53 - `1` (max instances)**
- Máximo de 1 instância do pipe (só 1 cliente por vez)
- Para múltiplos clientes, aumente este valor

**Linha 54 - `4096` (output buffer size)**
- Buffer de saída: 4KB para dados enviados pelo servidor

**Linha 55 - `4096` (input buffer size)**
- Buffer de entrada: 4KB para dados recebidos pelo servidor

**Linha 56 - `0` (default timeout)**
- Timeout padrão: 0 = sem timeout (bloqueante)
- Para timeout, usar valor em milissegundos

**Linha 57 - `None` (security attributes)**
- Segurança padrão (qualquer processo pode conectar)
- Para restringir acesso, passar objeto de segurança


Sim, `win32file.ReadFile(pipe, 4096)[1]` é uma chamada **bloqueante** que:

**O que faz:**
- Lê até 4096 bytes do pipe
- **Bloqueia** até houver dados disponíveis ou o pipe ser fechado
- Retorna uma tupla `(resultado, dados)` onde `[1]` pega apenas os dados que é do tipo `bytes`

**Fluxo:**
1. Fica esperando dados chegarem no buffer
2. Quando dados chegam, lê e retorna
3. Se não houver dados, continua bloqueado indefinidamente

**Por que `[1]`:**
- `ReadFile` retorna `(hr, data)` - código de resultado + dados
- `[1]` ignora o código de resultado e pega só os dados

**Problema:** É bloqueante, então o `Ctrl+C` não funciona quando está esperando dados. É o mesmo problema do `ConnectNamedPipe`.


O resultado [0] (o primeiro elemento da tupla) é o código de resultado do Windows API:

O que é:

Um número inteiro indicando sucesso ou erro
0 = sucesso
Outros valores = códigos de erro do Windows
Exemplos de códigos de erro:

109 = ERROR_BROKEN_PIPE (pipe quebrado)
232 = ERROR_NO_DATA (sem dados)
233 = ERROR_PIPE_NOT_CONNECTED (pipe não conectado)