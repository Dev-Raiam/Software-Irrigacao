namespace WorkerService.State
{
    public class MqttConfiguracao
    {
        public string BrokerLocal { get; set; } = null!;
        public string BrokerRemoto { get; set; } = null!;
        public int Porta { get; set; }
        public string? UsuarioRemoto { get; set; }
        public string? SenhaRemoto { get; set; }
    }
}
