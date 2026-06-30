using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Automacao.Irrigacao.Comandos.Controle
{
    public class DefinirValvulaProporcional : IrrigacaoCommand
    {
        public int Abertura { get; set; } = 0;
    }
}
