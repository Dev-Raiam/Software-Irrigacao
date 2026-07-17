using System.IO.Ports;
using NModbus;
using NModbus.Serial;

namespace Toolbox.Automacao.Core.Services.Modbus;

/// <summary>
/// Facade para operações Modbus usando NModbus
/// Simplifica a interação com dispositivos Modbus RTU via porta serial
/// </summary>
internal sealed class ModbusFacade : IModbusFacade, IDisposable
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

    public ModbusFacade(ModbusConfig config)
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
    /// Abre a conexão Modbus RTU
    /// </summary>
    public void Conectar()
    {
        if (_serialPort != null && _serialPort.IsOpen)
            return;

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
        _master = new ModbusFactory().CreateRtuMaster(adapter);
    }

    /// <summary>
    /// Lê registros holding do dispositivo Modbus
    /// </summary>
    public async Task<ushort[]> LerHoldingRegistersAsync(
        byte slaveAddress,
        ushort startAddress,
        ushort numberOfPoints
    )
    {
        if (_master == null)
            throw new InvalidOperationException(
                "Conexão Modbus não estabelecida. Chame Conectar() primeiro."
            );

        return await _master.ReadHoldingRegistersAsync(slaveAddress, startAddress, numberOfPoints);
    }

    /// <summary>
    /// Lê coils do dispositivo Modbus
    /// </summary>
    public async Task<bool[]> LerCoilsAsync(
        byte slaveAddress,
        ushort startAddress,
        ushort numberOfPoints
    )
    {
        if (_master == null)
            throw new InvalidOperationException(
                "Conexão Modbus não estabelecida. Chame Conectar() primeiro."
            );

        return await _master.ReadCoilsAsync(slaveAddress, startAddress, numberOfPoints);
    }

    /// <summary>
    /// Escreve um único coil no dispositivo Modbus
    /// </summary>
    public async Task EscreverCoilAsync(byte slaveAddress, ushort coilAddress, bool value)
    {
        if (_master == null)
            throw new InvalidOperationException(
                "Conexão Modbus não estabelecida. Chame Conectar() primeiro."
            );

        await _master.WriteSingleCoilAsync(slaveAddress, coilAddress, value);
    }

    /// <summary>
    /// Fecha a conexão Modbus e libera recursos
    /// </summary>
    public void Desconectar()
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
