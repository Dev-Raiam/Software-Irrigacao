using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Modulo.Tekon.Abstractions;
using Toolbox.Modulo.Tekon.Models;

namespace Toolbox.Modulo.Tekon.Dispositivos
{
    internal class TWP_4AI4DI1UTPerfil : ITekonDispositivoPerfil
    {
        public string Modelo => TekonConstants.Modelos.TWP_4AI4DI1UT;

        public ConfiguracaoLeitura? CoilRegisters(int? index = null)
        {
            return new ConfiguracaoLeitura
            {
                StartAddress = (ushort)(((index! - 1) * 16) + 0),
                NumberOfRegister = 10,
            };
        }

        public ConfiguracaoLeitura? HoldingRegisters(int? index = null)
        {
            return new ConfiguracaoLeitura
            {
                StartAddress = (ushort)(((index! - 1) * 20) + 0),
                NumberOfRegister = 20,
            };
        }

        public ITekonDispositivoDado Parse(DispositivoContextoLeitura context)
        {
            return new TWP_4AI4DI1UT
            {
                NumeroSerie = (long)(context.HoldingRegisters[0] << 16 | context.HoldingRegisters[1]),

                Modelo = IdentificarModelo(context.HoldingRegisters[2]),

                RSSI = context.HoldingRegisters[3] / -2,

                PeriodoComunicacao = context.HoldingRegisters[4],
                TempoDecorrido = context.HoldingRegisters[5],

                TensaoAlimentacao = context.HoldingRegisters[6] / 10.0f,

                TemperaturaExterna = (float) Math.Round(Conversor.ToFloat(context.HoldingRegisters[8], context.HoldingRegisters[7]), 2),
                ValorEntradaAnalogica_1 = (float) Math.Round(Conversor.ToFloat(context.HoldingRegisters[10], context.HoldingRegisters[9]), 2),
                ValorEntradaAnalogica_2 = (float) Math.Round(Conversor.ToFloat(context.HoldingRegisters[12], context.HoldingRegisters[11]), 2),
                ValorEntradaAnalogica_3 = (float) Math.Round(Conversor.ToFloat(context.HoldingRegisters[14], context.HoldingRegisters[13]), 2),
                ValorEntradaAnalogica_4 = (float) Math.Round(Conversor.ToFloat(context.HoldingRegisters[16], context.HoldingRegisters[15]), 2),

                VersaoFirmware = context.HoldingRegisters[17],
                RevisaoVersao = context.HoldingRegisters[18],
                VersaoHardware = context.HoldingRegisters[19],

                EstadoSaidaRemotaDigital = context.CoilRegisters[0],
                EstadoSaidaEnergiaExterna = context.CoilRegisters[1],
                EstadoEntradaInterruptor = context.CoilRegisters[2],
                EstadoEntradaDigital_1 = context.CoilRegisters[3],
                EstadoEntradaDigital_2 = context.CoilRegisters[4],
                EstadoEntradaDigital_3 = context.CoilRegisters[5],
                EstadoEntradaDigital_4 = context.CoilRegisters[6],
            };
        }
        private string IdentificarModelo(int modelo)
        {
            return modelo switch
            {
                37 => "Transmissor TWP_4AI4DI1UT 868 MHZ",
                38 => "Transmissor TWP_4AI4DI1UT 915 MHZ",
                _ => "Desconhecido",
            };
        }
    }
}
