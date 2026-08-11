#!/bin/bash
set -e

# Configurações
APP_DIR="/opt/edge-plc"
BACKUP_DIR="/opt/irrigacao-backup"
LOG_FILE="/var/log/irrigacao-update.log"
TMP_DIR="/tmp/irrigacao-update"
UPDATE_URL="https://github.com/Dev-Raiam/Software-Irrigacao/releases/latest/download/irrigacao.zip"

log() {
    echo "$(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a "$LOG_FILE"
}

limpar_tmp() {
    rm -rf "$TMP_DIR"
}

trap limpar_tmp EXIT

# Prepara diretórios
mkdir -p "$TMP_DIR"
mkdir -p "$BACKUP_DIR"

log "Verificando atualizações em: $UPDATE_URL"

# Consulta a API do GitHub para saber a tag da ultima release
API_URL="https://api.github.com/repos/Dev-Raiam/Software-Irrigacao/releases/latest"
TAG=$(curl -s -L \
    -H "Accept: application/vnd.github+json" \
    -H "User-Agent: edge-plc-updater" \
    "$API_URL" | grep -oP '"tag_name":\s*"\K[^"]+' || true)
TAG_NORMALIZADA=$(echo "$TAG" | sed -e 's/^[vV]//' -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//')

VERSAO_ATUAL=$( "$APP_DIR/irrigacao" --version 2>/dev/null || echo "nenhuma" )
VERSAO_ATUAL_NORMALIZADA=$(echo "$VERSAO_ATUAL" | sed -e 's/^[vV]//' -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//')

# Se a tag for igual ou versao instalada comecar com a tag, nao baixa nada
if [ -n "$TAG" ]; then
    case "$VERSAO_ATUAL_NORMALIZADA" in
        "$TAG_NORMALIZADA"|"$TAG_NORMALIZADA".*|"$TAG_NORMALIZADA"-*)
            log "Sistema já está na versão mais recente ($VERSAO_ATUAL). Nada a fazer."
            exit 0
            ;;
    esac
fi

log "Nova versão detectada no GitHub: $TAG (atual: $VERSAO_ATUAL)"

# Baixa o zip do GitHub
if ! curl -L -f -o "$TMP_DIR/irrigacao.zip" "$UPDATE_URL"; then
    log "Não foi possível baixar o arquivo de atualização."
    exit 0
fi

log "Download concluído"

# Extrai o pacote de atualização
unzip -o "$TMP_DIR/irrigacao.zip" -d "$TMP_DIR/extract"


# Verifica se encontrou os arquivos do app
if [ ! -f "$TMP_DIR/extract/irrigacao" ]; then
    log "Binário 'irrigacao' não encontrado no zip. Cancelando."
    exit 1
fi

# Garante permissão de execução no binário extraído
chmod +x "$TMP_DIR/extract/irrigacao"

VERSAO_NOVA=$("$TMP_DIR/extract/irrigacao" --version 2>/dev/null || echo "desconhecida")
VERSAO_ATUAL=$("$APP_DIR/irrigacao" --version 2>/dev/null || echo "nenhuma")
log "Versão atual: $VERSAO_ATUAL | Nova versão: $VERSAO_NOVA"

# Se a versão for a mesma, não há o que atualizar
if [ "$VERSAO_NOVA" != "desconhecida" ] && [ "$VERSAO_NOVA" = "$VERSAO_ATUAL" ]; then
    log "Sistema já está na versão mais recente. Nada a fazer."
    exit 0
fi

# Para o serviço antes de trocar os arquivos
log "Parando serviço irrigacao"
systemctl stop irrigacao.service || true

# Faz backup dos arquivos atuais
TIMESTAMP=$(date '+%Y%m%d%H%M%S')
BACKUP_PATH="$BACKUP_DIR/irrigacao-$TIMESTAMP"
log "Fazendo backup da instalação atual em $BACKUP_PATH"
cp -r "$APP_DIR" "$BACKUP_PATH"

# Copia novos arquivos do zip, sobrescrevendo os existentes sem apagar nada
log "Copiando novos arquivos"
cp -rf "$TMP_DIR/extract/"* "$APP_DIR/"

# Garante permissão de execução
chmod +x "$APP_DIR/irrigacao"

# Inicia o serviço
log "Iniciando serviço irrigacao"
systemctl start irrigacao.service

# Verifica se subiu corretamente
sleep 2
if systemctl is-active --quiet irrigacao.service; then
    log "Atualização concluída com sucesso"
else
    log "Falha ao iniciar o novo binário. Fazendo rollback."
    systemctl stop irrigacao.service || true
    ULTIMO_BACKUP=$(ls -td "$BACKUP_DIR"/*/ | head -n1)
    rm -rf "$APP_DIR"
    cp -r "$ULTIMO_BACKUP" "$APP_DIR"
    chmod +x "$APP_DIR/irrigacao"
    systemctl start irrigacao.service
    log "Rollback concluído"
fi
