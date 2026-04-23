# Windows Service — Guia Completo

## 1. O que é `UseWindowsService()`

O `UseWindowsService()` é o método do .NET que faz sua aplicação se comportar como um **serviço Windows nativo** — rodando em background, sem precisar de usuário logado, reiniciando automaticamente com o sistema.

É configurado no `Program.cs` dentro do `IServiceCollection`:

```csharp
services.AddWindowsService(options =>
{
    options.ServiceName = "SoftwareAutomacao";
});
```

Isso **não instala** o serviço — apenas prepara a aplicação para rodar como um quando instalada.

---

## 2. O que é `ServiceController`

`ServiceController` é uma classe do .NET que serve para **controlar** serviços Windows existentes — iniciar, parar, pausar, verificar status. Ele **não cria nem instala** serviços, apenas os gerencia de fora.

```csharp
using System.ServiceProcess;

var sc = new ServiceController("SoftwareAutomacao");

if (sc.Status == ServiceControllerStatus.Running)
{
    sc.Stop();
    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
}

sc.Start();
sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
```

---

## 3. Arquitetura ideal para serviço + atualizador

```
solution/
├── WorkerService/           ← serviço principal (este projeto)
│   ├── Workers/             ← lógica de background
│   └── Program.cs           ← registrado como Windows Service
│
└── Updater/                 ← (futuro) serviço de atualização separado
    ├── UpdaterWorker.cs     ← verifica update a cada X horas
    ├── Updater.cs           ← baixa, valida, aplica o .zip
    └── Program.cs           ← registrado como Windows Service separado
```

O atualizador pode usar `ServiceController` para parar e reiniciar o `WorkerService` após aplicar uma atualização.

---

## 4. Instalar o serviço no Windows

Após publicar o `.exe`, rode **uma única vez** como Administrador:

**Via PowerShell:**
```powershell
New-Service -Name "SoftwareAutomacao" `
            -BinaryPathName "D:\Releases\worker-service-v1.0.3\WorkerService.exe" `
            -DisplayName "Software - Automacao" `
            -StartupType Automatic

Start-Service -Name "SoftwareAutomacao"
```

**Via sc.exe (CMD):**
```cmd
sc create "SoftwareAutomacao" binPath="D:\Releases\worker-service-v1.0.3\WorkerService.exe"
sc start "SoftwareAutomacao"
```

---

## 5. Gerenciar o serviço no dia a dia

```powershell
# Ver status
Get-Service -Name "SoftwareAutomacao"

# Ver todos os detalhes
Get-Service -Name "SoftwareAutomacao" | Select-Object *

# Parar
Stop-Service -Name "SoftwareAutomacao"

# Iniciar
Start-Service -Name "SoftwareAutomacao"

# Reiniciar
Restart-Service -Name "SoftwareAutomacao"

# Desinstalar (quando necessário)
Remove-Service -Name "SoftwareAutomacao"
```

---

## 6. Ver logs do serviço

### Pelo Serilog (arquivo)
Os logs ficam em:
```
<diretório do .exe>\Logs\log-YYYYMMDD.txt
```

> **Importante:** o `Directory.SetCurrentDirectory(AppContext.BaseDirectory)` deve ser a **primeira linha** do `Program.cs`, antes do `Log.Logger`, para garantir que os logs sejam gravados no diretório do executável e não em `C:\Windows\System32\`.

```csharp
// Ordem correta no Program.cs
Directory.SetCurrentDirectory(AppContext.BaseDirectory); // 1º

Log.Logger = new LoggerConfiguration()                   // 2º
    .WriteTo.File("Logs/log-.txt", ...)
    .CreateLogger();
```

### Pelo Event Viewer (Windows)
Se o serviço travar antes de gravar qualquer log:

```powershell
Get-EventLog -LogName Application -Source "SoftwareAutomacao" -Newest 10
```

Ou manualmente: **Visualizador de Eventos** → `Logs do Windows` → `Aplicativo` → filtrar por Source `SoftwareAutomacao`.

---

## 7. Atualizar para uma nova versão

1. Publicar o novo `.exe` e arquivos em uma nova pasta (ex: `worker-service-v1.0.4\`)
2. Parar o serviço:
   ```powershell
   Stop-Service -Name "SoftwareAutomacao"
   ```
3. Remover o serviço antigo (`Remove-Service` só existe no PowerShell 6+, use `sc.exe`):
   ```powershell
   sc.exe delete "SoftwareAutomacao"
   ```
4. Criar apontando para a nova pasta:
   ```powershell
   New-Service -Name "SoftwareAutomacao" `
               -BinaryPathName "D:\Releases\worker-service-v1.0.4\WorkerService.exe" `
               -DisplayName "Software - Automacao" `
               -StartupType Automatic
   Start-Service -Name "SoftwareAutomacao"
   ```

---

## 8. O que o `sc.exe delete` apaga?

`sc.exe delete` apenas **remove o registro do serviço** no Windows — equivale a desfazer o `New-Service`.

O que ele **faz**:
- Remove a entrada do serviço no registro do Windows (`HKLM\SYSTEM\CurrentControlSet\Services\`)
- O serviço some da lista de serviços

O que ele **não toca**:
- `WorkerService.exe`
- `database.db`
- Pasta `Logs\`
- `appsettings.json`
- Qualquer outro arquivo em `D:\Releases\...`

Todos os arquivos ficam intactos na pasta. O `sc.exe delete` é só o "desregistro" — como desinstalar um atalho, não o programa em si.
