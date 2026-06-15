//using IrrigacaoInteligente.Core.Cache;
//using IrrigacaoInteligente.Core.Mqtt;

//namespace IrrigacaoInteligente.Core.State;
//public class GerenciadorAplicacao
//{
//    private readonly AplicacaoState _aplicacaoState;
//    private readonly CredenciaisAplicacao _credenciaisAplicacao;
//    private readonly MqttClienteRemoto _mqttClienteRemoto;
//    private readonly MqttClienteLocal _mqttClienteLocal;

//    public GerenciadorAplicacao(
//        AplicacaoState aplicacaoState,
//        CredenciaisAplicacao credenciaisAplicacao,
//        MqttClienteRemoto mqttClienteRemoto,
//        MqttClienteLocal mqttClienteLocal
//    )
//    {
//        _aplicacaoState = aplicacaoState;
//        _credenciaisAplicacao = credenciaisAplicacao;
//        _mqttClienteRemoto = mqttClienteRemoto;
//        _mqttClienteLocal = mqttClienteLocal;
//    }

//    public void VerificarAplicacao()
//    {
//        VerificarCredenciais();
//    }

//    private void VerificarCredenciais()
//    {
//        if (_credenciaisAplicacao.Invalida)
//        {
//            _aplicacaoState.AtualizarEstado(AplicacaoState.EstadoAplicacao.AguardandoCredenciais);
//            return;
//        }

//        _aplicacaoState.AtualizarEstado(AplicacaoState.EstadoAplicacao.InicializandoServicos);
//    }

//    private void VerificarServicoMqtt()
//    {
//        if (_mqttClienteLocal.Conectado || _mqttClienteRemoto.Conectado) { }
//    }

//    public void VerificarDados() { }
//}
