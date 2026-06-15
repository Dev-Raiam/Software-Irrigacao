# Capítulo 12 — Segurança

A segurança do IrrigacaoInteligente cobre quatro frentes: **autenticação JWT** com a nuvem, **proteção de dados** (Data Protection), **criptografia de credenciais** e **rate limiting** dos endpoints.

## 12.1 Autenticação JWT automática

A comunicação com a API de nuvem é autenticada por **JWT Bearer**. A injeção do token é transparente para o código de negócio, via um `DelegatingHandler`:

```
ManipuladorTokenAcesso (DelegatingHandler)
  → usa o token de ArmazenamentoToken se ainda válido
  → se expirado: GerenciadorToken → AutenticacaoApi.AutenticarAsync(...)
  → injeta "Authorization: Bearer <token>" nas requisições do AutomacaoApi
```

Componentes envolvidos:

- **`Infrastructure/Auth/GerenciadorToken.cs`** — obtém/renova o token.
- **`Infrastructure/Auth/IAutenticacaoApi.cs`** + **`Infrastructure/Http/AutenticacaoApi.cs`** — chamam o endpoint de autenticação.
- **`Infrastructure/Http/ManipuladorTokenAcesso.cs`** — handler que injeta o Bearer.
- **`State/ArmazenamentoToken.cs`** — guarda o JWT ativo em memória.

O `ManipuladorTokenAcesso` é registrado como transient e plugado no `HttpClient` da automação:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Configurations\InjecaoDependenciaConfiguracao.cs:63-74
        services.AddTransient<ManipuladorTokenAcesso>();
        ...
        services.AddHttpClient<IAutenticacaoApi, AutenticacaoApi>();
        services
            .AddHttpClient<IAutomacaoApi, AutomacaoApi>()
            .AddHttpMessageHandler<ManipuladorTokenAcesso>();
```

A validação do JWT nos endpoints HTTP locais é configurada em `Configurations/AuthenticacaoConfiguracao.cs` (`RegistrarAuthenticacao`) e ativada no pipeline em `Program.cs`:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Program.cs:82-85
app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();
```

## 12.2 Data Protection

Chaves de proteção de dados são persistidas no sistema de arquivos, com nome de aplicação fixo e caminho configurável:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Configurations\InjecaoDependenciaConfiguracao.cs:33-40
        var keysPath =
            configuration["DataProtection:KeysPath"]
            ?? Path.Combine(AppContext.BaseDirectory, "Keys");

        services
            .AddDataProtection()
            .SetApplicationName("IrrigacaoInteligente")
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath));
```

> **Produção (Raspberry/Linux):** use um diretório persistente **fora** do diretório de deploy, por exemplo `/var/lib/irrigacao/keys`. Se as chaves forem perdidas, os dados criptografados (credenciais) não poderão ser descriptografados.

## 12.3 Criptografia de credenciais

As credenciais sensíveis **não** ficam em texto puro. Elas são criptografadas via `Infrastructure/Criptografia/Criptografia.cs` (que implementa `ICriptografia`) antes de serem gravadas no SQLite. O contrato é injetado como singleton:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Configurations\InjecaoDependenciaConfiguracao.cs:45
        services.AddSingleton<ICriptografia, Criptografia>();
```

As credenciais são informadas pelo operador via endpoint `POST /configuracao/credenciais` (ver Capítulo 13) e ficam armazenadas criptografadas localmente. O estado `CredenciaisAplicacao` expõe a propriedade `Invalida` usada pelos gates de prontidão.

## 12.4 Rate Limiting

Os endpoints de credenciais são protegidos por um **Concurrency Limiter** chamado `limite-tentativas`, mitigando abuso/força-bruta:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Program.cs:65-76
builder.Services.AddRateLimiter(options =>
{
    options.AddConcurrencyLimiter(
        "limite-tentativas",
        options =>
        {
            options.PermitLimit = 5;
            options.QueueLimit = 5;
            options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        }
    );
});
```

- **`PermitLimit = 5`** — no máximo 5 requisições simultâneas.
- **`QueueLimit = 5`** — até 5 aguardando na fila.
- **Ordem:** as mais antigas são processadas primeiro.

## 12.5 Recomendações de segurança

- **Mover credenciais do broker remoto** (hoje hardcoded no `MqttWorker`) para configuração segura.
- **Usar User Secrets** em desenvolvimento; variáveis de ambiente/secret store em produção.
- **Proteger o caminho das chaves** de Data Protection com permissões de arquivo adequadas.
- **Rotacionar tokens/credenciais** periodicamente.

---

Anterior: [Capítulo 11 — Persistência e Sincronização](11-Persistencia-e-Sincronizacao.md) · Próximo: [Capítulo 13 — Configuração e Execução](13-Configuracao-e-Execucao.md)
