using Toolbox.Modulo.Tekon.Models;

namespace Toolbox.Modulo.Tekon.Interfaces
{
    public interface ITekonDispositivoPerfil
    {
        string Modelo { get; }
        ConfiguracaoLeituraDispositivo ObterConfiguracaoLeituraDispositivo(byte index);
        ConfiguracaoLeituraDispositivo ObterConfiguracaoLeituraDispositivo();
        ConfiguracaoLeitura ObterConfiguracaoLeituraDigital(string port, byte index);
        ConfiguracaoLeitura ObterConfiguracaoLeituraDigital(string port);
        ConfiguracaoLeitura ObterConfiguracaoLeituraTemperatura(string port);
        ConfiguracaoLeitura ObterConfiguracaoLeituraTemperatura(string port, byte index);
        ConfiguracaoLeitura ObterConfiguracaoLeituraAnalogica(string port, byte index);
        ConfiguracaoLeitura ObterConfiguracaoLeituraAnalogica(string port);
        ConfiguracaoEscritaDigital ObterConfiguracaoEscritaDigital(string port, byte index);
        ConfiguracaoEscritaDigital ObterConfiguracaoEscritaDigital(string port);

        double ConverterValorAnalogico(ushort[] buffer, ConfiguracaoLeitura configuracao);
        double ConverterValorTemperatura(ushort[] buffer, ConfiguracaoLeitura configuracao);

        ITekonDispositivoDado Parse(DispositivoContextoLeitura context);
    }
}
