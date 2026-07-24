using System.IO.Ports;
using NModbus;
using NModbus.Serial;

namespace Toolbox.Industrial.Core.Communication.Modbus;

public sealed record Configuration(
    string Port,
    int BaudRate,
    int DataBits,
    Parity Parity,
    StopBits StopBits,
    int ReadTimeout,
    int WriteTimeout
);

public interface IModbusRTU : IDisposable
{
    void Connect();
    void Disconnect();
    Task<ushort[]> ReadHoldingRegistersAsync(
        byte slaveAddress,
        ushort startAddress,
        ushort numberOfPoints
    );
    Task<bool[]> ReadCoilsAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints);
    Task WriteCoilAsync(byte slaveAddress, ushort coilAddress, bool value);
}

internal sealed class ModbusRTU : IModbusRTU
{
    private readonly SerialPort _serialPort;
    private readonly IModbusMaster _master;
    private readonly string _loggerInfo;
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

    public void Disconnect()
    {
        try
        {
            _serialPort?.Close();
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Erro ao desconectar da porta {_serialPort.PortName}, {_loggerInfo}: {ex.Message}",
                ex
            );
        }
    }

    public Task<ushort[]> ReadHoldingRegistersAsync(
        byte slaveAddress,
        ushort startAddress,
        ushort numberOfPoints
    )
    {
        Connect();
        return _master.ReadHoldingRegistersAsync(slaveAddress, startAddress, numberOfPoints);
    }

    public Task<bool[]> ReadCoilsAsync(
        byte slaveAddress,
        ushort startAddress,
        ushort numberOfPoints
    )
    {
        Connect();
        return _master.ReadCoilsAsync(slaveAddress, startAddress, numberOfPoints);
    }

    public Task WriteCoilAsync(byte slaveAddress, ushort coilAddress, bool value)
    {
        Connect();
        return _master.WriteSingleCoilAsync(slaveAddress, coilAddress, value);
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
