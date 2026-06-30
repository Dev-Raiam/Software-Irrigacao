using Toolbox.Automacao.Irrigacao.Marcas.Tekon;
using Toolbox.Automacao.Irrigacao.Modbus;
using Toolbox.Automacao.Irrigacao.Models;

namespace Toolbox.Automacao.Irrigacao.Drivers
{
    public class ModbusDriverTekon : ModbusDriver
    {
        public ModbusDriverTekon(ModbusMaster modbus)
            : base(modbus) { }

        protected override ConfiguracaoLeitura ObterConfiguracaoHoldingRegister(
            string modelo,
            byte index
        ) => TekonConfiguracao.ObterConfiguracaoHoldingRegister(modelo, index);

        protected override ConfiguracaoLeitura ObterConfiguracaoCoils(string modelo, byte index) =>
            TekonConfiguracao.ObterConfiguracaoCoils(modelo, index);

        protected override Telemetria Decodificar(
            Guid id,
            string modelo,
            ushort[] buffer,
            bool[] bufferCoils
        ) => TekonConfiguracao.CriarTelemetria(id, modelo, buffer, bufferCoils);
    }
}
