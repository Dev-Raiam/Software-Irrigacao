using System;
using System.Collections.Generic;
using System.Text;
using WorkerService.Features.Comandos;
using WorkerService.Features.Comandos._Interfaces;
using WorkerService.Features.Shared.Abstractions;

namespace WorkerService.Features.Comandos
{
    public class ProcessarComando : IProcessarComando
    {
        public async Task Processar(Comando comando)
        {
            return;
        }
    }
}
