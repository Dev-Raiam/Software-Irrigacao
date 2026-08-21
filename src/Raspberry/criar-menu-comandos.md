A ideia será:

```bash
cmd
```

mostrar:

```text
===== IRRIGAÇÃO =====
1) Status
2) Reiniciar
3) Logs

===== REDE =====
4) Ver IP
5) Ver WiFi
6) Testar internet

===== SISTEMA =====
7) Disco
8) Memória
9) Processos

Escolha:
```

E você digita `3`, por exemplo, para abrir os logs.

---

# 1. Criar a pasta

No Raspberry:

```bash
mkdir -p ~/toolbox-plc-cmd/config/commands
mkdir -p ~/toolbox-plc-cmd/list
```

Vamos colocar nosso programa ali.

---

# 2. Criar o arquivo de comandos

```bash
nano ~/toolbox-plc-cmd/config/commands
```

Coloque:

```text
# IRRIGAÇÃO
1|Status|systemctl status irrigacao
2|Iniciar|sudo systemctl start irrigacao
3|Parar|sudo systemctl stop irrigacao
4|Reiniciar|sudo systemctl restart irrigacao
5|Logs (últimos 100)|journalctl -u irrigacao -n 100
6|Logs em tempo real|journalctl -u irrigacao -f
7|Logs de erro|journalctl -u irrigacao -p err -n 100
8|Logs de hoje|journalctl -u irrigacao --since today

# ATUALIZADOR
9|Status|systemctl status irrigacao-atualizador
10|Iniciar|sudo systemctl start irrigacao-atualizador
11|Parar|sudo systemctl stop irrigacao-atualizador
12|Reiniciar|sudo systemctl restart irrigacao-atualizador
13|Logs (últimos 100)|journalctl -u irrigacao-atualizador -n 100
14|Logs em tempo real|journalctl -u irrigacao-atualizador -f
15|Logs de erro|journalctl -u irrigacao-atualizador -p err -n 100

# REDE
16|Ver IP|ip addr show
17|Ver WiFi|nmcli device status
18|Ver conexões WiFi|nmcli connection show
19|Testar internet|ping -c 4 8.8.8.8
20|Portas abertas|ss -tlnp
21|Logs do WiFi|journalctl -u NetworkManager -n 100
22|Logs do WiFi em tempo real|journalctl -u NetworkManager -f
23|Reiniciar rede|sudo systemctl restart NetworkManager

# SISTEMA
24|Disco|df -h
25|Memória|free -h
26|Processos|top
27|Processos por memória|ps aux --sort=-%mem | head -10
28|Versão do sistema|cat /etc/os-release
29|Listar arquivos /opt/edge-plc|ls -la /opt/edge-plc
30|Reiniciar Raspberry|sudo reboot

# PACOTES .DEB
31|Instalar .deb|sudo dpkg -i pacote.deb
32|Listar pacotes irrigacao|dpkg -l | grep irrigacao
33|Ver arquivos do pacote|dpkg -L nome-pacote
```

Salve:

```text
Ctrl + O
Enter
Ctrl + X
```

---

# 3. Criar o programa `list`

Agora:

```bash
nano ~/toolbox-plc-cmd/list
```

Coloque:

```bash
#!/bin/bash
ARQUIVO="$HOME/toolbox-plc-cmd/config/commands"

# Cores
VERDE='\033[1;32m'
VERDE_ESC='\033[0;32m'
CIANO='\033[1;36m'
BRANCO='\033[1;37m'
CINZA='\033[0;90m'
RESET='\033[0m'

LARGURA=56

linha() {
    printf "${CINZA}%s${RESET}\n" "$(printf '─%.0s' $(seq 1 $LARGURA))"
}

topo() {
    printf "${CINZA}┌%s┐${RESET}\n" "$(printf '─%.0s' $(seq 1 $LARGURA))"
}

fundo() {
    printf "${CINZA}└%s┘${RESET}\n" "$(printf '─%.0s' $(seq 1 $LARGURA))"
}

titulo() {
    local texto="🧰  PLC TOOLBOX"
    local pad=$(( (LARGURA - ${#texto} - 1) / 2 ))
    printf "${CINZA}│${RESET}%*s${VERDE}${texto}${RESET}%*s${CINZA}│${RESET}\n" "$pad" "" "$((LARGURA - pad - ${#texto} - 1))" ""
}

while true; do
    clear
    topo
    titulo
    linha
    echo
    while IFS='|' read -r numero descricao comando; do
        [ -z "$numero" ] && continue
        if [[ "$numero" == \#* ]]; then
            echo
            printf "  ${CIANO}▸ %s${RESET}\n" "${numero#\# }"
            continue
        fi
        printf "  ${BRANCO}%2s${RESET} ${CINZA})${RESET} %s\n" "$numero" "$descricao"
    done < "$ARQUIVO"
    echo
    linha
    printf "  ${VERDE_ESC}%2s${RESET} ${CINZA})${RESET} Sair\n" "0"
    fundo
    echo
    read -p "$(printf "${VERDE}Escolha: ${RESET}")" escolha

    if [ "$escolha" = "0" ]; then
        clear
        exit 0
    fi

    comando=$(grep "^$escolha|" "$ARQUIVO" | cut -d'|' -f3-)
    if [ -z "$comando" ]; then
        echo
        printf "${CINZA}Comando inválido.${RESET}\n"
        sleep 2
        continue
    fi

    clear
    topo
    printf "${CINZA}│${RESET} ${VERDE}Executando:${RESET}\n"
    printf "${CINZA}│${RESET} %s\n" "$comando"
    fundo
    echo
    eval "$comando"
    echo
    read -p "Pressione ENTER para voltar ao menu..."
done
```

Salve:

```text
Ctrl + O
Enter
Ctrl + X
```

---

# 4. Dar permissão

Agora:

```bash
chmod +x ~/toolbox-plc-cmd/list
```

---

# 5. Adicionar como comando:

```bash
sudo ln -s /home/plcadmin/toolbox-plc-cmd/list /usr/local/bin/cmd
```

# 6. Testar

Agora simplesmente:

```bash
cmd
```

Você deverá receber algo parecido com:

```text
========================================
          COMANDOS DO PLC
========================================


----- IRRIGAÇÃO -----
1) Status da irrigação
2) Reiniciar irrigação
3) Logs da irrigação
4) Logs da irrigação em tempo real

----- REDE -----
5) Ver IP
6) Ver conexões WiFi
7) Status da rede
8) Logs do WiFi em tempo real

----- SISTEMA -----
10) Espaço em disco
11) Memória
12) Processos
13) Versão do sistema
14) Reiniciar Raspberry

0) Sair

Escolha:
```

Se você digitar:

```text
3
```

ele executará:

```bash
journalctl -u irrigacao -n 100
```

Depois que terminar, pressione `ENTER` e volta para o menu.

---

