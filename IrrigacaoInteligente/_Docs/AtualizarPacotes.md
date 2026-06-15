dotnet tool install --global dotnet-outdated-tool
dotnet outdated --upgrade
dotnet ef migrations add "Update Telemetria" -o Infrastructure/Data/Migrations