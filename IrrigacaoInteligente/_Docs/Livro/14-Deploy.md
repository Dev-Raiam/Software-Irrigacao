# Capítulo 14 — Deploy

## 14.1 Alvo de produção

O alvo típico é um **Raspberry Pi** ou **CLP** rodando **Linux ARM64**. O perfil de `Release` já está configurado para gerar um binário único, self-contained.

## 14.2 Publicar

```bash
dotnet publish -c Release
```

Isso gera um executável:

- **`linux-arm64`** — runtime de destino.
- **`PublishSingleFile`** — um único arquivo executável.
- **`SelfContained`** — inclui o runtime .NET (não exige .NET instalado no dispositivo).
- **sem símbolos de debug** — binário enxuto.

## 14.3 Checklist de produção

- **Data Protection:** aponte `DataProtection:KeysPath` para um diretório persistente fora do deploy (ex.: `/var/lib/irrigacao/keys`) e garanta permissões adequadas. Perder essas chaves inutiliza as credenciais criptografadas.
- **Logs:** defina `Log:Path` para um diretório com espaço e rotação adequados (ex.: `/var/log/irrigacao`).
- **Banco SQLite:** garanta que o diretório do `IrrigacaoInteligente.db` seja gravável e persistente.
- **Brokers MQTT:** confirme acesso ao broker local (hardware) e ao remoto (nuvem). Considere externalizar as credenciais do broker remoto (hoje hardcoded).
- **Ambiente:** defina `DOTNET_ENVIRONMENT=Production` para usar o pipeline de log de produção.

## 14.4 Rodar como serviço (systemd)

Em produção, registre o binário como um serviço do systemd para reinício automático. Exemplo de unit:

```ini
[Unit]
Description=IrrigacaoInteligente Edge Service
After=network.target

[Service]
Type=notify
WorkingDirectory=/opt/irrigacao
ExecStart=/opt/irrigacao/IrrigacaoInteligente
Restart=always
RestartSec=5
Environment=DOTNET_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl daemon-reload
sudo systemctl enable --now irrigacao
sudo systemctl status irrigacao
```

> O host é um Worker Service e suporta integração com o ciclo de vida do systemd (`Type=notify`).

## 14.5 Pós-deploy

1. Verifique os logs em `Log:Path` (ou `journalctl -u irrigacao -f`).
2. Envie as credenciais via `POST /configuracao/credenciais`.
3. Confirme no log a mensagem "Aplicação pronta." e a conexão dos brokers MQTT.

---

Anterior: [Capítulo 13 — Configuração e Execução](13-Configuracao-e-Execucao.md) · Próximo: [Capítulo 15 — Logs e Observabilidade](15-Logs-e-Observabilidade.md)
