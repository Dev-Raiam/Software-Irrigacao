using Toolbox.Modulo.Tekon.Models;

namespace Toolbox.Modulo.Tekon.Interfaces
{
    public interface ITekonDispositivoPerfil
    {
        string Modelo { get; }
        ConfiguracaoLeituraDispositivo ObterConfiguracaoLeituraDispositivo(ushort index);
        ConfiguracaoLeituraDispositivo ObterConfiguracaoLeituraDispositivo();
        ConfiguracaoLeitura ObterConfiguracaoLeituraDigital(string port, ushort index);
        ConfiguracaoLeitura ObterConfiguracaoLeituraDigital(string port);
        ConfiguracaoLeitura ObterConfiguracaoLeituraAnalogica(string port, ushort index);
        ConfiguracaoLeitura ObterConfiguracaoLeituraAnalogica(string port);
        ConfiguracaoEscritaDigital ObterConfiguracaoEscritaDigital(string port, ushort index);

        ITekonDispositivoDado Parse(DispositivoContextoLeitura context);
    }
}
