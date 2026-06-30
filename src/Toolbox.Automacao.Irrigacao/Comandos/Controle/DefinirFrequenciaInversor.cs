using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Automacao.Irrigacao.Comandos.Controle
{
    public class DefinirFrequenciaInversor : IrrigacaoCommand
    {
        public double Frequencia { get; set; } = 0.0;
    }
}
