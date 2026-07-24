namespace Toolbox.Industrial.Driver.Tekon.Models
{
    /// <summary>
    ///  Classes de Configuracao
    /// </summary>
    public class ConfiguracaoLeitura
    {
        public ushort StartAddress { get; init; }
        public ushort NumberOfPoints { get; init; }
    }

    public class ConfiguracaoLeituraDispositivo
    {
        public ConfiguracaoHoldingRegisters? HoldingRegisters { get; set; }
        public ConfiguracaoCoilRegisters? CoilRegisters { get; set; }

        public class ConfiguracaoHoldingRegisters
        {
            public ushort StartAddress { get; init; }
            public ushort NumberOfPoints { get; init; }
        }

        public class ConfiguracaoCoilRegisters
        {
            public ushort StartAddress { get; init; }
            public ushort NumberOfPoints { get; init; }
        }
    }

    public class ConfiguracaoEscritaDigital
    {
        public ushort CoilAddress { get; init; }
    }

    public class ConfiguracaoEscritaAnalogica
    {
        public ushort RegisterAddress { get; init; }
    }
}
