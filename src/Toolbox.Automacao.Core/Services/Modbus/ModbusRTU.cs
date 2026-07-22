using NModbus;
using NModbus.Serial;
using System.IO.Ports;
using Toolbox.Automacao.Core.Services.Mqtt;
using static Toolbox.Automacao.Core.Services.Modbus.IModbusRTU;

namespace Toolbox.Automacao.Core.Services.Modbus;

/// <summary>
/// Interface Facade para simplificar operações Modbus
/// </summary>
public interface IModbusRTU : IDisposable
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

    public sealed record Configuration(string Port, int BaudRate, int DataBits, Parity Parity, StopBits StopBits, int ReadTimeout, int WriteTimeout);

}

/// <summary>
/// Facade para operações Modbus usando NModbus
/// Simplifica a interação com dispositivos Modbus RTU via porta serial
/// </summary>
internal sealed class ModbusRTU : IModbusRTU
{
    private readonly string _loggerInfo;
    private readonly SerialPort _serialPort;
    private readonly IModbusMaster _master;
    private bool _disposed = false;
    public string LoggerInfo => _loggerInfo;
    public ModbusRTU(Configuration config, string loggerInfo)
    {
        _loggerInfo = loggerInfo;
        _serialPort = new SerialPort(config.Port)
        {
            BaudRate = config.BaudRate,
            DataBits = config.DataBits,
            Handshake = Handshake.None,
            RtsEnable = true,
            DtrEnable = false,
            Parity = config.Parity,
            StopBits = config.StopBits,
            ReadTimeout = config.ReadTimeout,
            WriteTimeout = config.WriteTimeout,
        };
        _master = new ModbusFactory().CreateRtuMaster(new SerialPortAdapter(_serialPort));
    }

    /// <summary>
    /// Opens the Modbus RTU connection
    /// </summary>
    public void Connect()
    {
        if (_serialPort.IsOpen)
            return;

        try
        {
            _serialPort.Open();
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Erro ao conectar na porta {_serialPort.PortName}, {_loggerInfo}: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Reads holding registers from the Modbus device
    /// </summary>
    public Task<ushort[]> ReadHoldingRegistersAsync(
        byte slaveAddress,
        ushort startAddress,
        ushort numberOfPoints
    )
    {
        Connect();
        return _master.ReadHoldingRegistersAsync(
            slaveAddress,
            startAddress,
            numberOfPoints
        );
    }

    /// <summary>
    /// Reads coils from the Modbus device
    /// </summary>
    public Task<bool[]> ReadCoilsAsync(
        byte slaveAddress,
        ushort startAddress,
        ushort numberOfPoints
    )
    {
        Connect();
        return _master.ReadCoilsAsync(slaveAddress, startAddress, numberOfPoints);
    }

    /// <summary>
    /// Writes a single coil to the Modbus device
    /// </summary>
    public Task WriteCoilAsync(byte slaveAddress, ushort coilAddress, bool value)
    {
        Connect();
        return _master.WriteSingleCoilAsync(slaveAddress, coilAddress, value);
    }

    /// <summary>
    /// Closes the Modbus connection and releases resources
    /// </summary>
    public void Disconnect()
    {
        _serialPort?.Close();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _master?.Dispose();
        _serialPort?.Close();
        _serialPort?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

public sealed class ModbusRTUManager
{
    private ModbusRTU _current;
    public ModbusRTUManager(Configuration config, string loggerInfo)
    {
        _current = new ModbusRTU(config, loggerInfo);
    }

    public IModbusRTU Current => _current;
    public ModbusRTUManager Reload(Configuration config)
    {
        var loggerInfo = _current.LoggerInfo;
        _current.Dispose();
        _current = new ModbusRTU(config, loggerInfo);
        return this;
    }
}
