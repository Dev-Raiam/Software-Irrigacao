# Toolbox.Industrial.Shields

Módulo de comunicação com shields industriais via NamedPipe (Windows) ou Socket (Linux).

## Configuração do Ambiente

### 1. Criar ambiente virtual
```powershell
cd src/Toolbox.Industrial.Shields
python -m venv .venv
```

### 2. Ativar ambiente virtual
```powershell
# Windows
.venv\Scripts\activate

# Linux/Mac
source .venv/bin/activate
```

### 3. Instalar dependências
```powershell
pip install -r requirements.txt
```

## Executar Servidor

```powershell
python src/hub/server.py
```

## Estrutura do Projeto

- `src/hub/server.py` - Servidor NamedPipe/Socket
- `src/hub/shields_io.py` - Classe de controle I/O
- `src/hub/Docs/` - Documentação técnica

## Dependências

- `pywin32==312` - Comunicação NamedPipe (Windows)

## Notas

- O `.venv` não é versionado (veja `.gitignore`)
- Cada desenvolvedor deve criar seu próprio ambiente virtual
- Para Raspberry/Linux, usar Socket em vez de NamedPipe

## Atualizar Requirements
```powershell
pip freeze > requirements.txt
```