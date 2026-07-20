namespace Toolbox.Automacao.Core.Services.Modbus;

/// <summary>
/// Interface Facade para simplificar operações Modbus
/// </summary>
public interface IModbus
{
    /// <summary>
    /// Opens the Modbus connection
    /// </summary>
    void Connect();

    /// <summary>
    /// Reads holding registers from the Modbus device
    /// </summary>
    /// <param name="slaveAddress">Device address (slave address)</param>
    /// <param name="startAddress">Starting register address</param>
    /// <param name="numberOfPoints">Number of registers to read</param>
    /// <returns>Array with register values</returns>
    Task<ushort[]> ReadHoldingRegistersAsync(
        byte slaveAddress,
        ushort startAddress,
        ushort numberOfPoints
    );

    /// <summary>
    /// Reads coils from the Modbus device
    /// </summary>
    /// <param name="slaveAddress">Device address (slave address)</param>
    /// <param name="startAddress">Starting coil address</param>
    /// <param name="numberOfPoints">Number of coils to read</param>
    /// <returns>Array with coil values</returns>
    Task<bool[]> ReadCoilsAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints);

    /// <summary>
    /// Writes a single coil to the Modbus device
    /// </summary>
    /// <param name="slaveAddress">Device address (slave address)</param>
    /// <param name="coilAddress">Coil address</param>
    /// <param name="value">Value to write (true/false)</param>
    Task WriteCoilAsync(byte slaveAddress, ushort coilAddress, bool value);

    /// <summary>
    /// Closes the Modbus connection and releases resources
    /// </summary>
    void Disconnect();
}
