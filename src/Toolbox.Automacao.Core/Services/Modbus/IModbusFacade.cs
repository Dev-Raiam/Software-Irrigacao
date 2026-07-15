namespace Toolbox.Automacao.Core.Services.Modbus;

/// <summary>
/// Interface Facade para simplificar operações Modbus
/// </summary>
public interface IModbusFacade
{
    /// <summary>
    /// Abre a conexão Modbus
    /// </summary>
    void Conectar();

    /// <summary>
    /// Lê registros holding do dispositivo Modbus
    /// </summary>
    /// <param name="enderecoDispositivo">Endereço do dispositivo (slave address)</param>
    /// <param name="enderecoInicial">Endereço inicial do registro</param>
    /// <param name="quantidadeRegistros">Quantidade de registros a ler</param>
    /// <returns>Array com os valores dos registros</returns>
    Task<ushort[]> LerHoldingRegistersAsync(
        byte enderecoDispositivo,
        ushort enderecoInicial,
        ushort quantidadeRegistros
    );

    /// <summary>
    /// Lê coils do dispositivo Modbus
    /// </summary>
    /// <param name="enderecoDispositivo">Endereço do dispositivo (slave address)</param>
    /// <param name="enderecoInicial">Endereço inicial do coil</param>
    /// <param name="quantidadeCoils">Quantidade de coils a ler</param>
    /// <returns>Array com os valores dos coils</returns>
    Task<bool[]> LerCoilsAsync(
        byte enderecoDispositivo,
        ushort enderecoInicial,
        ushort quantidadeCoils
    );

    /// <summary>
    /// Escreve um único coil no dispositivo Modbus
    /// </summary>
    /// <param name="enderecoDispositivo">Endereço do dispositivo (slave address)</param>
    /// <param name="enderecoCoil">Endereço do coil</param>
    /// <param name="valor">Valor a escrever (true/false)</param>
    Task EscreverCoilAsync(byte enderecoDispositivo, ushort enderecoCoil, bool valor);

    /// <summary>
    /// Fecha a conexão Modbus e libera recursos
    /// </summary>
    void Desconectar();
}
