using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.Messages;

namespace Toolbox.Automacao.Irrigacao.Comandos
{
    public abstract class IrrigacaoCommand : Command
    {
        public Guid Id { get; init; }
        public Guid CorrelationId { get; init; } = Guid.NewGuid();
        public DateTime CriadoEm { get; init; } = DateTime.UtcNow;
    }
}
