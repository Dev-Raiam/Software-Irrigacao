namespace IrrigacaoInteligente.State
{
    public class MqttConfiguracao
    {
        public string TopicoLocal { get; init; } = null!;
        public string BrokerLocal { get; init; } = null!;
        public string BrokerRemoto { get; init; } = null!;
        public int Porta { get; init; }
        public string? UsuarioRemoto { get; init; }
        public string? SenhaRemoto { get; init; }
    }
}
