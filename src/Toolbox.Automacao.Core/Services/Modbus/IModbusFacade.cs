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
    /// <param name="slaveAddress">Endereço do dispositivo (slave address)</param>
    /// <param name="startAddress">Endereço inicial do registro</param>
    /// <param name="numberOfPoints">Quantidade de registros a ler</param>
    /// <returns>Array com os valores dos registros</returns>
    Task<ushort[]> LerHoldingRegistersAsync(
        byte slaveAddress,
        ushort startAddress,
        ushort numberOfPoints
    );

    /// <summary>
    /// Lê coils do dispositivo Modbus
    /// </summary>
    /// <param name="slaveAddress">Endereço do dispositivo (slave address)</param>
    /// <param name="startAddress">Endereço inicial do coil</param>
    /// <param name="numberOfPoints">Quantidade de coils a ler</param>
    /// <returns>Array com os valores dos coils</returns>
    Task<bool[]> LerCoilsAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints);

    /// <summary>
    /// Escreve um único coil no dispositivo Modbus
    /// </summary>
    /// <param name="slaveAddress">Endereço do dispositivo (slave address)</param>
    /// <param name="coilAddress">Endereço do coil</param>
    /// <param name="value">Valor a escrever (true/false)</param>
    Task EscreverCoilAsync(byte slaveAddress, ushort coilAddress, bool value);

    /// <summary>
    /// Fecha a conexão Modbus e libera recursos
    /// </summary>
    void Desconectar();
}
