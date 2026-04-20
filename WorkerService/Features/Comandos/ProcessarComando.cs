using System;
using System.Collections.Generic;
using System.Text;
using WorkerService.Features.Comandos.Executores;

namespace WorkerService.Features.Comandos
{
    public class ProcessarComando(AcionarPorta _acionarPorta)
    {
        public async Task Processar(ComandoAcionar comando)
        {
            switch (comando.TipoComando)
            {
                case "acionar_porta":
                    await _acionarPorta.Executar(comando);
                    break;
                default:
                    Console.WriteLine("Mensagem Chegou em Processar Comando");
                    break;
            }

            return;
        }
    }
}
