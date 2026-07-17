using Toolbox.Modulo.Tekon.Models;

namespace Toolbox.Modulo.Tekon.Interfaces
{
    public interface ITekonDriver
    {
        Task<ITekonDispositivoDado> LerDispositivo(string modelo, byte slaveAddress);
        Task<ITekonDispositivoDado> LerDispositivo(string modelo, byte slaveAddress, byte index);
        Task<double> LerPortaAnalogica(string modelo, byte slaveAddress, byte index, string port);
        Task<double> LerPortaAnalogica(string modelo, byte slaveAddress, string port);
        Task<bool> LerPortaDigital(string modelo, byte slaveAddress, string port);
        Task<bool> LerPortaDigital(string modelo, byte slaveAddress, byte index, string port);
        Task EscreverPortaAnalogica(string modelo, byte slaveAddress, string port, int value);
        Task EscreverPortaAnalogica(
            string modelo,
            byte slaveAddress,
            byte index,
            string port,
            int value
        );
        Task EscreverPortaDigital(string modelo, byte slaveAddress, string port, bool value);
        Task EscreverPortaDigital(
            string modelo,
            byte slaveAddress,
            byte index,
            string port,
            bool value
        );
    }
}
