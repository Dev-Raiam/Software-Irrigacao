using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Automacao.Sincronizacao.Sync
{
    public class SincronizacaoConfiguracao
    {
        public Guid PainelId { get; set; }
        public bool Automatica { get; set; } = true;
        public Agendamento Agendamento { get; set; } = new Agendamento();
    }
    public class Agendamento 
    {
        /// <summary>
        /// Valor padrão de Sincronização de 20 segundos.
        /// </summary>
        public TimeSpan Timer { get; set; } = TimeSpan.FromSeconds(20);
    }
}
