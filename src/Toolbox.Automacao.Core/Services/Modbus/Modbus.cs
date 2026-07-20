using System.IO.Ports;
using NModbus;
using NModbus.Serial;
using Toolbox.Automacao.Core.Services.Modbus.Exceptions;

namespace Toolbox.Automacao.Core.Services.Modbus;

/// <summary>
/// Facade para operações Modbus usando NModbus
/// Simplifica a interação com dispositivos Modbus RTU via porta serial
/// </summary>
internal sealed class Modbus : IModbus, IDisposable
{
    private readonly string _port;
    private readonly int _baudRate;
    private readonly int _dataBits;
    private readonly Parity _parity;
    private readonly StopBits _stopBits;
    private readonly int _readTimeout;
    private readonly int _writeTimeout;

    private SerialPort? _serialPort;
    private IModbusMaster? _master;
    private bool _disposed;

    public Modbus(ModbusConfig config)
    {
        _port = config.Porta;
        _baudRate = config.BaudRate;
        _dataBits = config.DataBits;
        _parity = config.Parity;
        _stopBits = config.StopBits;
        _readTimeout = config.ReadTimeout;
        _writeTimeout = config.WriteTimeout;
    }

    /// <summary>
    /// Opens the Modbus RTU connection
    /// </summary>
    public void Connect()
    {
        if (_serialPort != null && _serialPort.IsOpen)
            return;

        try
        {
            _serialPort = new SerialPort(_port)
            {
                BaudRate = _baudRate,
                DataBits = _dataBits,
                Handshake = Handshake.None,
                RtsEnable = true,
                DtrEnable = false,
                Parity = _parity,
                StopBits = _stopBits,
                ReadTimeout = _readTimeout,
                WriteTimeout = _writeTimeout,
            };

            _serialPort.Open();
            var adapter = new SerialPortAdapter(_serialPort);
            _master = new NModbus.ModbusFactory().CreateRtuMaster(adapter);
        }
        catch (Exception ex)
        {
            throw new ModbusConexaoException(
                $"Erro ao conectar na porta {_port}: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Reads holding registers from the Modbus device
    /// </summary>
    public async Task<ushort[]> ReadHoldingRegistersAsync(
        byte slaveAddress,
        ushort startAddress,
        ushort numberOfPoints
    )
    {
        if (_master == null)
            throw new ModbusConexaoException(
                "Modbus connection not established. Call Connect() first."
            );

        try
        {
            return await _master.ReadHoldingRegistersAsync(
                slaveAddress,
                startAddress,
                numberOfPoints
            );
        }
        catch (Exception ex)
        {
            throw new ModbusLeituraException(
                $"Erro ao ler holding registers (slave: {slaveAddress}, start: {startAddress}, count: {numberOfPoints}): {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Reads coils from the Modbus device
    /// </summary>
    public async Task<bool[]> ReadCoilsAsync(
        byte slaveAddress,
        ushort startAddress,
        ushort numberOfPoints
    )
    {
        if (_master == null)
            throw new ModbusConexaoException(
                "Modbus connection not established. Call Connect() first."
            );

        try
        {
            return await _master.ReadCoilsAsync(slaveAddress, startAddress, numberOfPoints);
        }
        catch (Exception ex)
        {
            throw new ModbusLeituraException(
                $"Erro ao ler coils (slave: {slaveAddress}, start: {startAddress}, count: {numberOfPoints}): {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Writes a single coil to the Modbus device
    /// </summary>
    public async Task WriteCoilAsync(byte slaveAddress, ushort coilAddress, bool value)
    {
        if (_master == null)
            throw new ModbusConexaoException(
                "Modbus connection not established. Call Connect() first."
            );

        try
        {
            await _master.WriteSingleCoilAsync(slaveAddress, coilAddress, value);
        }
        catch (Exception ex)
        {
            throw new ModbusEscritaException(
                $"Erro ao escrever coil (slave: {slaveAddress}, address: {coilAddress}, value: {value}): {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Closes the Modbus connection and releases resources
    /// </summary>
    public void Disconnect()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _master?.Dispose();
        _serialPort?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
