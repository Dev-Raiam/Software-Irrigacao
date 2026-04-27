using System;
using System.Collections.Generic;
using System.Text;
using MQTTnet;

namespace WorkerService.Features.Shared.Abstractions
{
    public interface IMqttCliente
    {
        bool Conectado { get; }
        Task<bool> Conectar(
            string servidor,
            int porta,
            string clienteId,
            string? usuario,
            string? senha,
            CancellationToken cancellationToken
        );
        Task AssinarTopico(string topico, CancellationToken cancellationToken);
        Task Publicar(
            string topico,
            string mensagem,
            CancellationToken cancellationToken,
            MqttApplicationMessageBuilder? messageBuilder = null
        );
        Task Publicar(string topico, object mensagem, CancellationToken cancellationToken);
        void ExecutarCallbackMensageria(CancellationToken cancellationToken);
        void ExecutarCallbackDesconectado(CancellationToken cancellationToken);
        Task Desconectar(CancellationToken cancellationToken);
    }
}
