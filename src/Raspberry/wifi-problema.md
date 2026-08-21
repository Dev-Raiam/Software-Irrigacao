# Correção: WiFi não reconecta ao perder internet no Raspberry Pi

> **Data:** 21/08/2026  
> **Host:** toolbox-plc-1  
> **SO:** Debian 12 (bookworm) | Kernel 6.12.87+rpt-rpi-v8  
> **Hardware:** Raspberry Pi 4 | Chip WiFi BCM43455 (brcmfmac)

---

## Problema

O Raspberry Pi perdia a conexão WiFi com a internet e **não reconectava sozinho**, exigindo reboot manual.

---

## Causa Raiz

1. **WiFi Power Management ativado** — o driver brcmfmac colocava o adaptador em modo de economia de energia. Quando a conexão caía, o adaptador não acordava para reconectar.
2. **Sem watchdog de conectividade** — nenhum serviço monitorava a internet para forçar reconexão automática.

---

## Comandos de Diagnóstico (executados na investigação)

### Identificar o sistema

```bash
cat /etc/os-release
uname -a
cat /boot/firmware/cmdline.txt
cat /boot/firmware/config.txt | grep -iE 'wifi|wlan|power|brcm|dtoverlay'
```

### Status dos serviços de rede

```bash
systemctl is-active NetworkManager dhcpcd wpa_supplicant systemd-networkd
systemctl list-unit-files | grep -E 'NetworkManager|dhcpcd|wpa_supplicant|systemd-networkd|networking'
```

### Interfaces e rotas

```bash
ip addr show
ip route show
cat /etc/resolv.conf
```

### Configuração de rede

```bash
cat /etc/dhcpcd.conf
cat /etc/network/interfaces
cat /etc/network/interfaces.d/*
```

### NetworkManager — conexões

```bash
nmcli device status
nmcli connection show
sudo cat /etc/NetworkManager/system-connections/GS.nmconnection
sudo cat /etc/NetworkManager/system-connections/Secador.nmconnection
cat /etc/NetworkManager/NetworkManager.conf
```

### Parâmetros da conexão WiFi GS

```bash
nmcli -f connection.autoconnect,connection.autoconnect-priority,connection.autoconnect-retries,802-11-wireless.powersave,ipv4.method connection show GS
nmcli -f all connection show GS | grep -iE 'autoconnect|power|retry|timeout|dhcp|dns|gateway|ignore'
```

### Status do WiFi (aqui descobrimos o problema)

```bash
iwconfig wlan0          # mostrou Power Management:on
iw dev wlan0 link
iw dev wlan0 info
```

### Logs do kernel

```bash
dmesg | grep -iE 'wlan|wifi|brcm|ieee80211|disconnect|deauth' | tail -30
# Resultado chave: brcmf_cfg80211_set_power_mgmt: power save enabled
```

### Logs do NetworkManager

```bash
sudo journalctl -u NetworkManager --no-pager -n 50 -o cat
```

### wpa_supplicant

```bash
sudo cat /etc/wpa_supplicant/wpa_supplicant.conf
```

### Histórico de reboots (evidência de intervenção manual)

```bash
last reboot | head -5
uptime
```

### Verificar se já existia algum watchdog

```bash
ls /etc/systemd/system/ | grep -iE 'network|wifi|watch|connect|ping'
find /etc/systemd/system/ -name "*.service" -exec grep -l -iE 'wifi|wlan|network|reconnect' {} \;
crontab -l
sudo crontab -l
```

---

## Correções Aplicadas

### Passo 1 — Desativar Power Save na conexão GS

```bash
sudo nmcli connection modify GS 802-11-wireless.powersave 2
```

### Passo 2 — Desativar Power Save globalmente (para todas as conexões futuras)

```bash
sudo tee /etc/NetworkManager/conf.d/wifi-powersave-off.conf > /dev/null << 'EOF'
[connection]
wifi.powersave = 2
EOF
```

### Passo 3 — Aplicar Power Save off na interface atual (sem reiniciar)

```bash
sudo iw dev wlan0 set power_save off
```

### Passo 4 — Criar script do watchdog

```bash
sudo tee /usr/local/bin/network-watchdog.sh > /dev/null << 'SCRIPT'
#!/bin/bash
# Network watchdog - monitors internet connectivity and reconnects WiFi if needed

GATEWAY="192.168.25.1"
INTERFACE="wlan0"
CONNECTION="GS"
MAX_FAILURES=3
LOG_TAG="network-watchdog"

failure_count=0

while true; do
    if ping -c 1 -W 5 -I "$INTERFACE" "$GATEWAY" > /dev/null 2>&1; then
        if [ "$failure_count" -gt 0 ]; then
            logger -t "$LOG_TAG" "Connectivity restored after $failure_count failures"
            failure_count=0
        fi
    else
        failure_count=$((failure_count + 1))
        logger -t "$LOG_TAG" "Ping to $GATEWAY failed (attempt $failure_count/$MAX_FAILURES)"

        if [ "$failure_count" -ge "$MAX_FAILURES" ]; then
            logger -t "$LOG_TAG" "Max failures reached. Restarting WiFi connection '$CONNECTION'"
            nmcli connection down "$CONNECTION"
            sleep 3
            nmcli connection up "$CONNECTION"
            sleep 10

            if ping -c 1 -W 5 -I "$INTERFACE" "$GATEWAY" > /dev/null 2>&1; then
                logger -t "$LOG_TAG" "WiFi reconnected successfully"
                failure_count=0
            else
                logger -t "$LOG_TAG" "WiFi reconnection failed. Will retry next cycle."
            fi
        fi
    fi

    sleep 30
done
SCRIPT

sudo chmod +x /usr/local/bin/network-watchdog.sh
```

### Passo 5 — Criar serviço systemd do watchdog

```bash
sudo tee /etc/systemd/system/network-watchdog.service > /dev/null << 'UNIT'
[Unit]
Description=Network Connectivity Watchdog
After=network-online.target
Wants=network-online.target

[Service]
Type=simple
ExecStart=/usr/local/bin/network-watchdog.sh
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
UNIT
```

### Passo 6 — Ativar e iniciar o watchdog

```bash
sudo systemctl daemon-reload
sudo systemctl enable network-watchdog.service
sudo systemctl start network-watchdog.service
```

---

## Verificação (rodar para confirmar que está tudo certo)

```bash
# Power save deve estar OFF
iwconfig wlan0 | grep -i power
# Esperado: Power Management:off

# Powersave da conexão deve ser 2 (disable)
nmcli -f 802-11-wireless.powersave connection show GS
# Esperado: 802-11-wireless.powersave: 2 (disable)

# Watchdog deve estar active (running) e enabled
systemctl status network-watchdog.service --no-pager
# Esperado: Active: active (running)
```

---

## Arquivos Criados/Modificados

| Arquivo | Ação |
|---------|------|
| `/etc/NetworkManager/conf.d/wifi-powersave-off.conf` | Criado — desativa power save globalmente |
| `/usr/local/bin/network-watchdog.sh` | Criado — script do watchdog |
| `/etc/systemd/system/network-watchdog.service` | Criado — serviço systemd do watchdog |
| Conexão GS (NetworkManager) | Modificada — `powersave` alterado para `2 (disable)` |

---

## Como monitorar o watchdog no dia a dia

```bash
# Ver logs do watchdog em tempo real
sudo journalctl -t network-watchdog -f

# Ver últimos 20 logs do watchdog
sudo journalctl -t network-watchdog -n 20

# Ver status do serviço
systemctl status network-watchdog.service

# Reiniciar o serviço manualmente (se necessário)
sudo systemctl restart network-watchdog.service
```

---

## Resumo

Vou fazer uma varredura no sistema para investigar o problema de reconexão de rede. Deixa eu coletar várias informações em paralelo.

Varredura concluída. Encontrei a causa raiz do problema. Vou apresentar os achados e aplicar as correções.

### Diagnóstico

O sistema é um Raspberry Pi (hostname `toolbox-plc-1`) rodando Debian 12 com kernel 6.12.87, usando WiFi (wlan0) conectado à rede "GS" com IP estático 192.168.25.50. O chip WiFi é BCM43455 (brcmfmac).

### Causa raiz: WiFi Power Management ativado

```
iwconfig wlan0 → Power Management:on
dmesg → brcmf_cfg80211_set_power_mgmt: power save enabled
```

O `802-11-wireless.powersave` no NetworkManager está em `0 (default)`, que para o driver brcmfmac significa **ativado**. Quando o WiFi entra em modo de economia de energia e a conexão cai, o adaptador não acorda corretamente para escanear e reconectar.

### Problema secundário: Sem watchdog de conectividade

Não existe nenhum script/serviço que monitore a internet e force a reconexão quando o link cai. O NetworkManager tem `autoconnect=yes` com retries infinitos, mas se o adaptador estiver dormindo, ele nem detecta a queda.

### Evidência adicional: Reboots frequentes

```
Aug 17 → Aug 19 → Aug 20 (x2) → Aug 21
```
Os reboots frequentes indicam que você vem reiniciando manualmente para restaurar a conexão.

### Correções aplicadas

1. **Power Save desativado na conexão GS** — `802-11-wireless.powersave` alterado de `0 (default)` para `2 (disable)`:
   - Confirmado: `Power Management:off` agora ativo em `wlan0`

2. **Power Save desativado globalmente** — criado `/etc/NetworkManager/conf.d/wifi-powersave-off.conf` com `wifi.powersave = 2` para todas as conexões WiFi futuras

3. **Watchdog de conectividade criado e ativado** — serviço `network-watchdog.service` que:
   - Faz ping no gateway (192.168.25.1) a cada 30 segundos
   - Após 3 falhas consecutivas, derruba e reergue a conexão WiFi via `nmcli`
   - Registra tudo no syslog (`logger -t network-watchdog`)
   - Já está **enabled** (inicia no boot) e **active (running)**

### Observações
- O sistema reiniciou frequentemente nos últimos dias (Aug 17, 19, 20 x2, 21) — provavelmente reboots manuais para restaurar a conexão
- A conexão "Secador" (segunda rede WiFi) também existe mas não estava ativa
- O `autoconnect` já estava habilitado com retries infinitos no NetworkManager, mas sem o watchdog não havia mecanismo para forçar a reconexão quando o adaptador travava

As mudanças são **persistentes** e sobrevivem a reboots. Não é mais necessário reiniciar o Raspberry Pi quando a internet cair.
