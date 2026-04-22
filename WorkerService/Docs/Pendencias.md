--- AMANHÃ
Pensar quais dados realmentes deveram vir para nossa base de dados Local 
R : Vamos Criar uma Tabela separada para Armazenar as Interfaces

--- Urgente ---
Temos que Retirar as inscriçõens de Topicos do Worker MQTT pois quando o Sistema Inicializa do Zero ele nao consegue se inscrever nos topicos pois Worker de Sincronização inicia Junto e dispositivos ainda nao esta na base de dados



MONITOR DE SAUDE DE PROCESSO

2. dotnet-counters (recomendado para .NET)
Ferramenta oficial da Microsoft para métricas em tempo real:

bash
# Instalar (uma vez)
dotnet tool install --global dotnet-counters
 
# Rodar (enquanto o app está executando)
dotnet-counters monitor --name WorkerService