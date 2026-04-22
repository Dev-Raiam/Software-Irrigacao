using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerService.Features.Shared.Abstractions
{
    public interface IMqttCliente
    {
        public Task Conectar(
            string servidor,
            int porta,
            string clienteId,
            string? usuario,
            string? senha,
            CancellationToken cancellationToken
        );
        public Task Assinar(string topico, CancellationToken cancellationToken);
        public Task Publicar(string topico, object mensagem, CancellationToken cancellationToken);
        public void IniciarMensageria(CancellationToken cancellationToken);
    }
}
