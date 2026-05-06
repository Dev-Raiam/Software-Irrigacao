namespace IrrigacaoInteligente.State
{
    public class MqttConfiguracao
    {
        public string Servidor { get; init; } = null!;
        public int Porta { get; init; }
        public string Usuario { get; init; } = null!;
        public string Senha { get; init; } = null!;
        public string TopicoCmdLocal { get; init; } = null!;
    }
}
