using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Industrial.Core.Messages.Integration;
using Toolbox.Core.Mediator;

namespace Toolbox.Industrial.Core.Messages
{
    public interface ICommandDispatcher
    {
        Task DispatchAsync(string jsonPayload);
    }

    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly Dictionary<string, Type> _commandTypes;
        private readonly IServiceProvider _serviceProvider;

        public CommandDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _commandTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

            // Registrar todos os comandos de controle
            RegisterCommand<AbrirValvula>();
            RegisterCommand<FecharValvula>();
            RegisterCommand<AcionarBomba>();
            RegisterCommand<DesligarBomba>();
            RegisterCommand<AcionarMotoBomba>();
            RegisterCommand<DesligarMotoBomba>();
            RegisterCommand<AcionarInversorFrequencia>();
            RegisterCommand<DesligarInversorFrequencia>();
            RegisterCommand<DefinirFrequenciaInversor>();
            RegisterCommand<AcionarSolenoide>();
            RegisterCommand<DesligarSolenoide>();
            RegisterCommand<DefinirValvulaProporcional>();

            // Registrar todos os comandos de leitura
            RegisterCommand<LerMonitorPosicao>();
            RegisterCommand<LerSensorCorrente>();
            RegisterCommand<LerSensorDistancia>();
            RegisterCommand<LerSensorNivel>();
            RegisterCommand<LerSensorPh>();
            RegisterCommand<LerSensorPressao>();
            RegisterCommand<LerSensorTemperatura>();
            RegisterCommand<LerSensorTensao>();
            RegisterCommand<LerSensorUmidade>();

            // Registrar todos os comandos de sincronização
            RegisterCommand<SincronizarAutomacao>();
        }

        private void RegisterCommand<T>()
            where T : CommandBase
        {
            _commandTypes[typeof(T).Name] = typeof(T);
        }

        public async Task DispatchAsync(string jsonPayload)
        {
            try
            {
                var envelope = JsonSerializer.Deserialize<CommandEnvelope>(jsonPayload);
                if (envelope == null || string.IsNullOrEmpty(envelope.CommandType))
                    throw new InvalidOperationException(
                        "Envelope inválido ou CommandType não informado"
                    );

                if (!_commandTypes.TryGetValue(envelope.CommandType, out var commandType))
                    throw new InvalidOperationException(
                        $"Tipo de comando desconhecido: {envelope.CommandType}"
                    );

                var command = (CommandBase?)
                    JsonSerializer.Deserialize(envelope.Payload.GetRawText(), commandType);

                if (command == null)
                    throw new InvalidOperationException("Falha ao desserializar comando");

                // Usar o Mediator para despachar o comando
                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Execute((dynamic)command);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro ao despachar comando: {ex.Message}", ex);
            }
        }
    }
}
