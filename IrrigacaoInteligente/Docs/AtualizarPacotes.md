dotnet tool install --global dotnet-outdated-tool
dotnet outdated --upgrade
dotnet ef migrations add "Telemetria" -o Infrastructure/Data/Migrations