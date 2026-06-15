dotnet ef migrations add RenomeandoTabelaConfiguracaoControlador -o Infrastructure/Data/Migrations
dotnet ef database update
dotnet tool update --global dotnet-ef

dotnet ef migrations add InicialSincronizacao --context IrrigacaoInteligenteContext --output-dir Infrastructure/Data/Migrations

dotnet ef migrations add InicialSincronizacao --context SincronizacaoDbContext --output-dir Infrastructure/Data/Migrations/Sincronizacao
