using Toolbox.Modulo.Tekon.Models;

namespace Toolbox.Modulo.Tekon.Interfaces
{
    /// <summary>
    /// Interface para driver de comunicação com dispositivos Tekon via Modbus.
    /// </summary>
    public interface ITekonDriver
    {
        /// <summary>
        /// Lê todos os dados de um dispositivo Tekon.
        /// </summary>
        /// <param name="modelo">Modelo do dispositivo (ex: "TWP-4AI4DI1UT", "WGW420").</param>
        /// <param name="slaveAddress">Endereço do escravo Modbus.</param>
        /// <returns>Dados completos do dispositivo.</returns>
        Task<ITekonDispositivoDado> LerDispositivo(string modelo, byte slaveAddress);

        /// <summary>
        /// Lê todos os dados de um dispositivo Tekon com índice para múltiplos dispositivos.
        /// </summary>
        /// <param name="modelo">Modelo do dispositivo (ex: "TWP-4AI4DI1UT", "WGW420").</param>
        /// <param name="slaveAddress">Endereço do escravo Modbus.</param>
        /// <param name="index">Índice do dispositivo (para suporte a múltiplos dispositivos do mesmo modelo).</param>
        /// <returns>Dados completos do dispositivo.</returns>
        Task<ITekonDispositivoDado> LerDispositivo(string modelo, byte slaveAddress, byte index);

        /// <summary>
        /// Lê o valor de uma porta analógica específica de um dispositivo.
        /// </summary>
        /// <param name="modelo">Modelo do dispositivo.</param>
        /// <param name="slaveAddress">Endereço do escravo Modbus.</param>
        /// <param name="index">Índice do dispositivo.</param>
        /// <param name="port">Identificador da porta (ex: "A1", "A2").</param>
        /// <returns>Valor analógico convertido.</returns>
        Task<double> LerPortaAnalogica(string modelo, byte slaveAddress, byte index, string port);

        /// <summary>
        /// Lê o valor de uma porta analógica específica de um dispositivo.
        /// </summary>
        /// <param name="modelo">Modelo do dispositivo.</param>
        /// <param name="slaveAddress">Endereço do escravo Modbus.</param>
        /// <param name="port">Identificador da porta (ex: "A1", "A2").</param>
        /// <returns>Valor analógico convertido.</returns>
        Task<double> LerPortaAnalogica(string modelo, byte slaveAddress, string port);

        /// <summary>
        /// Lê o valor de temperatura de uma porta específica de um dispositivo.
        /// </summary>
        /// <param name="modelo">Modelo do dispositivo.</param>
        /// <param name="slaveAddress">Endereço do escravo Modbus.</param>
        /// <param name="index">Índice do dispositivo.</param>
        /// <param name="port">Identificador da porta (ex: "UT").</param>
        /// <returns>Valor de temperatura convertido.</returns>
        Task<double> LerPortaTemperatura(string modelo, byte slaveAddress, byte index, string port);

        /// <summary>
        /// Lê o valor de temperatura de uma porta específica de um dispositivo.
        /// </summary>
        /// <param name="modelo">Modelo do dispositivo.</param>
        /// <param name="slaveAddress">Endereço do escravo Modbus.</param>
        /// <param name="port">Identificador da porta (ex: "UT").</param>
        /// <returns>Valor de temperatura convertido.</returns>
        Task<double> LerPortaTemperatura(string modelo, byte slaveAddress, string port);

        /// <summary>
        /// Lê o estado de uma porta digital de um dispositivo.
        /// </summary>
        /// <param name="modelo">Modelo do dispositivo.</param>
        /// <param name="slaveAddress">Endereço do escravo Modbus.</param>
        /// <param name="port">Identificador da porta (ex: "B1", "Q1").</param>
        /// <returns>Estado da porta (true/false).</returns>
        Task<bool> LerPortaDigital(string modelo, byte slaveAddress, string port);

        /// <summary>
        /// Lê o estado de uma porta digital de um dispositivo com índice.
        /// </summary>
        /// <param name="modelo">Modelo do dispositivo.</param>
        /// <param name="slaveAddress">Endereço do escravo Modbus.</param>
        /// <param name="index">Índice do dispositivo.</param>
        /// <param name="port">Identificador da porta (ex: "B1", "Q1").</param>
        /// <returns>Estado da porta (true/false).</returns>
        Task<bool> LerPortaDigital(string modelo, byte slaveAddress, byte index, string port);

        /// <summary>
        /// Escreve um valor em uma porta analógica de um dispositivo.
        /// </summary>
        /// <param name="modelo">Modelo do dispositivo.</param>
        /// <param name="slaveAddress">Endereço do escravo Modbus.</param>
        /// <param name="port">Identificador da porta.</param>
        /// <param name="value">Valor a ser escrito.</param>
        //Task EscreverPortaAnalogica(string modelo, byte slaveAddress, string port, int value);

        ///// <summary>
        ///// Escreve um valor em uma porta analógica de um dispositivo com índice.
        ///// </summary>
        ///// <param name="modelo">Modelo do dispositivo.</param>
        ///// <param name="slaveAddress">Endereço do escravo Modbus.</param>
        ///// <param name="index">Índice do dispositivo.</param>
        ///// <param name="port">Identificador da porta.</param>
        ///// <param name="value">Valor a ser escrito.</param>
        //Task EscreverPortaAnalogica(
        //    string modelo,
        //    byte slaveAddress,
        //    byte index,
        //    string port,
        //    int value
        //);

        /// <summary>
        /// Escreve um estado em uma porta digital de um dispositivo.
        /// </summary>
        /// <param name="modelo">Modelo do dispositivo.</param>
        /// <param name="slaveAddress">Endereço do escravo Modbus.</param>
        /// <param name="port">Identificador da porta (ex: "Q1").</param>
        /// <param name="value">Estado a ser escrito (true/false).</param>
        Task EscreverPortaDigital(string modelo, byte slaveAddress, string port, bool value);

        /// <summary>
        /// Escreve um estado em uma porta digital de um dispositivo com índice.
        /// </summary>
        /// <param name="modelo">Modelo do dispositivo.</param>
        /// <param name="slaveAddress">Endereço do escravo Modbus.</param>
        /// <param name="index">Índice do dispositivo.</param>
        /// <param name="port">Identificador da porta (ex: "Q1").</param>
        /// <param name="value">Estado a ser escrito (true/false).</param>
        Task EscreverPortaDigital(
            string modelo,
            byte slaveAddress,
            byte index,
            string port,
            bool value
        );
    }
}
