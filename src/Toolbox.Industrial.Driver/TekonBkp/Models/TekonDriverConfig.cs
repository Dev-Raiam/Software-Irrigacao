using System.IO.Ports;

namespace Toolbox.Industrial.Driver.TekonBkp.Models
{
    public class TekonDriverConfig
    {
        /// <summary>
        /// Porta serial (ex: COM1, COM2)
        /// </summary>
        public string Porta { get; set; } = "COM6";

        /// <summary>
        /// Taxa de transmissão em bauds
        /// </summary>
        public int BaudRate { get; set; } = 19200;

        /// <summary>
        /// Bits de dados
        /// </summary>
        public int DataBits { get; set; } = 8;

        /// <summary>
        /// Paridade
        /// </summary>
        public Parity Parity { get; set; } = Parity.None;

        /// <summary>
        /// Bits de parada
        /// </summary>
        public StopBits StopBits { get; set; } = StopBits.Two;

        /// <summary>
        /// Timeout de leitura em milissegundos
        /// </summary>
        public int ReadTimeout { get; set; } = 1000;

        /// <summary>
        /// Timeout de escrita em milissegundos
        /// </summary>
        public int WriteTimeout { get; set; } = 1000;
    }
}
