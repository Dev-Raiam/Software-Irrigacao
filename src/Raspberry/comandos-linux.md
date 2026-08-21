# Comandos Linux mais usados

## Listar arquivos e diretórios ocutos
ls -la

## journalctl (logs de serviço)

```bash
# Logs de hoje
journalctl -u irrigacao --since today

# Logs por data e hora específica
journalctl -u irrigacao --since "2026-08-20 14:00:00" --until "2026-08-20 15:00:00"

# Últimos N logs
journalctl -u irrigacao -n 50

# Acompanhar logs em tempo real
journalctl -u irrigacao -f

# Logs de erro apenas
journalctl -u irrigacao -p err

# Logs desde uma data sem paginação
journalctl -u irrigacao --since "2026-08-20" --no-pager

# Logs entre datas
journalctl -u irrigacao --since "2026-08-20" --until "2026-08-21" --no-pager

# Logs de dois serviços ao mesmo tempo
journalctl -u irrigacao -u irrigacao-atualizador -f
```

## systemctl (gerenciamento de serviços)

```bash
# Status do serviço
systemctl status irrigacao

# Verificar se está ativo
systemctl is-active irrigacao

# Iniciar / parar / reiniciar
sudo systemctl start irrigacao
sudo systemctl stop irrigacao
sudo systemctl restart irrigacao

# Habilitar / desabilitar inicialização automática
sudo systemctl enable irrigacao
sudo systemctl disable irrigacao

# Recarregar após alterar arquivo .service
sudo systemctl daemon-reload
```

## Permissões

```bash
# Dar permissão de execução
chmod +x irrigacao

# Dar permissão total (dono)
chmod 755 irrigacao

# Mudar dono do arquivo
sudo chown root:root irrigacao

# Mudar dono recursivo de uma pasta
sudo chown -R root:root /opt/edge-plc
```

## Arquivos e diretórios

```bash
# Copiar arquivo
cp /opt/edge-plc/irrigacao /var/backups/edge-plc/irrigacao

# Copiar mantendo permissões
cp -p /opt/edge-plc/irrigacao /var/backups/edge-plc/irrigacao

# Copiar pasta inteira
cp -r /opt/edge-plc /var/backups/edge-plc-backup

# Mover / renomear arquivo
mv /opt/edge-plc/irrigacao /opt/edge-plc/irrigacao-old

# Mover pasta / renomear pasta
mv /opt/edge-plc /opt/edge-plc-old

# Remover arquivo
rm /opt/edge-plc/irrigacao.zip

# Remover pasta e conteúdo
rm -r /opt/edge-plc/extracted

# Remover sem confirmar
rm -f /opt/edge-plc/irrigacao.zip

# Criar diretório
mkdir -p /var/backups/edge-plc

# Listar arquivos com detalhes
ls -la /opt/edge-plc
```

## Usuários

```bash
# Mudar para root
sudo -i

# Mudar de usuário
su - plcadmin

# Adicionar usuário
sudo adduser novousuario

# Adicionar usuário a grupo
sudo usermod -aG sudo plcadmin

# Verificar usuário atual
whoami
```

## Arquivos .service

```bash
# Editar serviço
sudo nano /etc/systemd/system/irrigacao.service

# Após editar, sempre recarregar
sudo systemctl daemon-reload
sudo systemctl restart irrigacao

# Mudar usuário do serviço (editar arquivo)
# User=root  →  User=plcadmin
sudo nano /etc/systemd/system/irrigacao.service
```

## Rede

```bash
# Verificar IP
ip addr show

# Verificar conectividade
ping 8.8.8.8

# Verificar portas abertas
ss -tlnp

# Reiniciar rede
sudo systemctl restart networking
```

## Sistema

```bash
# Versão do sistema
cat /etc/os-release

# Espaço em disco
df -h

# Processos consumindo mais CPU
top

# Processos consumindo memória
ps aux --sort=-%mem | head -10

# Desligar
sudo shutdown -h now

# Reiniciar
sudo reboot
```

## .deb (pacotes)

```bash
# Instalar pacote .deb
sudo dpkg -i pacote.deb

# Remover pacote
sudo dpkg -r nome-pacote

# Listar pacotes instalados
dpkg -l | grep irrigacao

# Ver arquivos de um pacote instalado
dpkg -L nome-pacote
```

