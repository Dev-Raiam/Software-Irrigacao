using Toolbox.Automacao.Irrigacao.Marcas.Tekon.Modelos;
using Toolbox.Automacao.Irrigacao.Modbus;
using Toolbox.Automacao.Irrigacao.Models;

namespace Toolbox.Automacao.Irrigacao.Marcas.Tekon
{
    public static class TekonConfiguracao
    {
        public static ConfiguracaoLeitura ObterConfiguracaoHoldingRegister(
            string modelo,
            byte index
        )
        {
            return modelo switch
            {
                Modelo.Gateway_WGW420 => WGW420.ConfiguracaoHoldingRegisters(),
                Modelo.Transmitter_TWP_1AI
                or Modelo.Transmitter_TWP_1DI
                or Modelo.Transmitter_TWP_1UT
                or Modelo.Transmitter_TWPH_1UT
                or Modelo.Transmitter_TWP_2AI
                or Modelo.Transmitter_TWP_2DI
                or Modelo.Transmitter_TWP_2UT
                or Modelo.Transmitter_TWP_4AI4DI1UT => TWP_4AI4DI1UT.ConfiguracaoHoldingRegisters(
                    index
                ),
                _ => throw new InvalidOperationException("Modelo Inexistente"),
            };
        }

        public static ConfiguracaoLeitura ObterConfiguracaoCoils(string modelo, byte index)
        {
            return modelo switch
            {
                Modelo.Transmitter_TWP_4AI4DI1UT => TWP_4AI4DI1UT.ConfiguracaoCoilsRegisters(index),
                _ => throw new InvalidOperationException("Modelo Inexistente"),
            };
        }

        public static Telemetria CriarTelemetria(
            Guid moduloId,
            string modelo,
            ushort[] buffer,
            bool[] bufferCoils
        )
        {
            return modelo switch
            {
                Modelo.Gateway_WGW420 => new WGW420(buffer).ObterTelemetria(moduloId, modelo),
                Modelo.Transmitter_TWP_1AI => new TWP_1AI(buffer).ObterTelemetria(moduloId),
                Modelo.Transmitter_TWP_1DI => new TWP_1DI(buffer).ObterTelemetria(moduloId),
                Modelo.Transmitter_TWP_1UT => new TWP_1UT(buffer).ObterTelemetria(moduloId),
                Modelo.Transmitter_TWPH_1UT => new TWPH_1UT(buffer).ObterTelemetria(moduloId),
                Modelo.Transmitter_TWP_2AI => new TWP_2AI(buffer).ObterTelemetria(moduloId),
                Modelo.Transmitter_TWP_2DI => new TWP_2DI(buffer).ObterTelemetria(moduloId),
                Modelo.Transmitter_TWP_2UT => new TWP_2UT(buffer).ObterTelemetria(moduloId),
                Modelo.Transmitter_TWP_4AI4DI1UT => new TWP_4AI4DI1UT(
                    buffer,
                    bufferCoils
                ).ObterTelemetria(moduloId),

                _ => throw new InvalidOperationException("Modelo Inexistente"),
            };
        }
    }
}
