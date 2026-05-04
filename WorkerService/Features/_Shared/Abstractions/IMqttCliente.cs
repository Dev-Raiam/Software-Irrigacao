using System;
using System.Collections.Generic;
using System.Text;
using MQTTnet;

namespace WorkerService.Features.Shared.Abstractions
{
    public interface IMqttCliente
    {
        bool Conectado { get; }
        Task<bool> ConectarAsync(
            string servidor,
            int porta,
            string clienteId,
            string? usuario,
            string? senha,
            CancellationToken cancellationToken
        );
        Task AssinarTopicoAsync(string topico, CancellationToken cancellationToken);
        void ExecutarCallbackMensageria(CancellationToken cancellationToken);
        void ExecutarCallbackDesconectado(CancellationToken cancellationToken);
        Task PublicarAsync(
            string topico,
            string mensagem,
            CancellationToken cancellationToken,
            MqttApplicationMessageBuilder? messageBuilder = null
        );
        Task PublicarAsync(
            string topico,
            object mensagem,
            CancellationToken cancellationToken,
            MqttApplicationMessageBuilder? messageBuilder = null
        );
        Task DesconectarAsync(CancellationToken cancellationToken);
    }
}
