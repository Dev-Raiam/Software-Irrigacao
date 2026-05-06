dotnet tool install --global dotnet-outdated-tool
dotnet outdated --upgrade
dotnet ef migrations add "NomeDaMigracao" -o Infrastructure/Data/Migrations