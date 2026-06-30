using System.IO.Ports;
using System.Text.Json;
using NModbus;
using NModbus.Extensions.Enron;
using NModbus.Serial;

namespace Toolbox.Automacao.Irrigacao.Modbus
{
    public sealed class ModbusMaster : IDisposable
    {
        public string Port { get; private set; } = null!;
        public int BaudRate { get; private set; }
        public int DataBits { get; private set; }
        public Parity Parity { get; private set; }
        public StopBits StopBits { get; private set; }
        public int ReadTimeout { get; private set; }
        public int WriteTimeout { get; private set; }

        private SerialPort _serialPort = null!;
        private IModbusMaster _master = null!;

        public ModbusMaster(
            string port,
            int baudRate,
            int dataBits,
            Parity parity,
            StopBits stopBits,
            int readTimeout,
            int writeTimeout
        )
        {
            Port = port;
            BaudRate = baudRate;
            DataBits = dataBits;
            Parity = parity;
            StopBits = stopBits;
            ReadTimeout = readTimeout;
            WriteTimeout = writeTimeout;
        }

        public void OpenConnection()
        {
            _serialPort = new SerialPort(Port);

            if (_serialPort.IsOpen == true)
                return;

            _serialPort.BaudRate = BaudRate;
            _serialPort.DataBits = DataBits;
            _serialPort.Handshake = Handshake.None;
            _serialPort.RtsEnable = true;
            _serialPort.DtrEnable = false;
            _serialPort.Parity = Parity;
            _serialPort.StopBits = StopBits;
            _serialPort.ReadTimeout = ReadTimeout;
            _serialPort.WriteTimeout = WriteTimeout;

            //Console.WriteLine(JsonSerializer.Serialize(_serialPort.BaudRate));
            //Console.WriteLine(JsonSerializer.Serialize(_serialPort.Handshake));
            //Console.WriteLine(JsonSerializer.Serialize(_serialPort.Parity));
            //Console.WriteLine(JsonSerializer.Serialize(_serialPort.StopBits));
            //Console.WriteLine(JsonSerializer.Serialize(_serialPort.DataBits));
            //Console.WriteLine(_serialPort.CtsHolding);

            _serialPort.Open();
            var adapter = new SerialPortAdapter(_serialPort);
            _master = new ModbusFactory().CreateRtuMaster(adapter);
        }

        internal async Task<ushort[]> ReadHoldingRegistersAsync(
            byte slaveAddress,
            ushort startAddress,
            ushort numberOfRegisters
        )
        {
            ushort[] buffer = await _master.ReadHoldingRegistersAsync(
                slaveAddress,
                startAddress,
                numberOfRegisters
            );

            return buffer;
        }

        internal async Task<bool[]> ReadCoilsRegistersAsync(
            byte slaveAddress,
            ushort startAddress,
            ushort numberOfRegisters
        )
        {
            bool[] buffer = await _master.ReadCoilsAsync(
                slaveAddress,
                startAddress,
                numberOfRegisters
            );

            return buffer;
        }

        internal async Task WriteSingleCoilAsync(byte slaveAddress, ushort coilAddress, bool value)
        {
            await _master.WriteSingleCoilAsync(slaveAddress, coilAddress, value);
        }

        public void Dispose()
        {
            _serialPort?.Dispose();
            _master?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
