using Toolbox.Automacao.Irrigacao.Modbus;
using Toolbox.Automacao.Irrigacao.Models;

namespace Toolbox.Automacao.Irrigacao.Drivers
{
    public abstract class ModbusDriver
    {
        protected readonly ModbusMaster _modbus;
        public ConfiguracaoLeitura? ConfigReadHoldingRegister { get; private set; }
        public ConfiguracaoLeitura? ConfigReadCoils { get; private set; }
        private ushort[] BufferHolding = [];
        private bool[] BufferCoils = [];
        private string Modelo = string.Empty;

        protected ModbusDriver(ModbusMaster modbus)
        {
            _modbus = modbus;
        }

        public async Task<ushort[]> ReadHoldingRegistersAsync(
            byte slaveAddress,
            ushort startAddress,
            ushort numberOfRegister
        )
        {
            BufferHolding = await _modbus.ReadHoldingRegistersAsync(
                slaveAddress,
                startAddress,
                numberOfRegister
            );

            return BufferHolding;
        }

        public async Task<ushort[]> ReadHoldingRegistersAsync(
            byte slaveAddress,
            string modelo,
            byte index
        )
        {
            Modelo = modelo;
            ConfigReadHoldingRegister = ObterConfiguracaoHoldingRegister(modelo, index);

            BufferHolding = await _modbus.ReadHoldingRegistersAsync(
                slaveAddress,
                ConfigReadHoldingRegister.StartAddress,
                ConfigReadHoldingRegister.NumberOfRegister
            );

            return BufferHolding;
        }

        public async Task<bool[]> ReadCoilsRegistersAsync(
            byte slaveAddress,
            ushort startAddress,
            ushort numberOfRegister
        )
        {
            BufferCoils = await _modbus.ReadCoilsRegistersAsync(
                slaveAddress,
                startAddress,
                numberOfRegister
            );

            return BufferCoils;
        }

        public async Task<bool[]> ReadCoilsRegistersAsync(
            byte slaveAddress,
            string modelo,
            byte index
        )
        {
            Modelo = modelo;
            ConfigReadCoils = ObterConfiguracaoCoils(modelo, index);

            BufferCoils = await _modbus.ReadCoilsRegistersAsync(
                slaveAddress,
                ConfigReadCoils.StartAddress,
                ConfigReadCoils.NumberOfRegister
            );

            return BufferCoils;
        }

        public async Task WriteSingleCoilAsync(byte slaveAddress, ushort coilAddress, bool value)
        {
            await _modbus.WriteSingleCoilAsync(slaveAddress, coilAddress, value);
        }

        public Telemetria DecodificarModeloHoldingRegisters()
        {
            return Decodificar(Guid.NewGuid(), Modelo, BufferHolding, BufferCoils);
        }

        protected abstract ConfiguracaoLeitura ObterConfiguracaoHoldingRegister(
            string modelo,
            byte index
        );
        protected abstract ConfiguracaoLeitura ObterConfiguracaoCoils(string modelo, byte index);
        protected abstract Telemetria Decodificar(
            Guid id,
            string modelo,
            ushort[] buffer,
            bool[] bufferCoils
        );
    }
}
