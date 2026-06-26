# Entity Framework Core Comandos

## Ferramentas
```bash
dotnet tool update --global dotnet-ef
```

## Migrations - IrrigacaoInteligenteContext
```bash
dotnet ef migrations add <NomeMigration> --context IrrigacaoInteligenteContext --output-dir Infrastructure/Data/Migrations
```

## Migrations - SincronizacaoDbContext
```bash
dotnet ef migrations add <NomeMigration> --context SincronizacaoDbContext --output-dir Infrastructure/Data/Migrations/Sincronizacao
```

## Database Update
```bash
dotnet ef database update
```

## Exemplos de Uso
```bash
# Criar migration para IrrigacaoInteligenteContext
dotnet ef migrations add RenomeandoTabelaConfiguracaoControlador --context IrrigacaoInteligenteContext --output-dir Infrastructure/Data/Migrations

# Criar migration para SincronizacaoDbContext
dotnet ef migrations add InicialSincronizacao --context SincronizacaoDbContext --output-dir Infrastructure/Data/Migrations/Sincronizacao
```
