using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Modulo.Tekon.Interfaces;
using Toolbox.Modulo.Tekon.Models;

namespace Toolbox.Modulo.Tekon.Dispositivos
{
    public class TWP_1AIPerfil : ITekonDispositivoPerfil
    {
        public string Modelo => TekonConstants.Modelos.TWP_1AI;

        public ConfiguracaoLeitura? CoilRegisters(int? index) => null;

        ////Voltar para Verificar como sera tratado o Null
        //public ConfiguracaoLeitura? HoldingRegisters(int? index)
        //{
        //    return new ConfiguracaoLeitura
        //    {
        //        StartAddress = (ushort)(((index! - 1) * 20) + 0),
        //        NumberOfRegister = 20,
        //    };
        //}

        public ConfiguracaoEscritaDigital ObterConfiguracaoEscritaDigital(string port, ushort index)
        {
            throw new NotImplementedException();
        }

        public ConfiguracaoLeitura ObterConfiguracaoLeituraAnalogica(string port, ushort index)
        {
            throw new NotImplementedException();
        }

        public ConfiguracaoLeitura ObterConfiguracaoLeituraAnalogica(string port)
        {
            throw new NotImplementedException();
        }

        public ConfiguracaoLeitura ObterConfiguracaoLeituraDigital(string port, ushort index)
        {
            throw new NotImplementedException();
        }

        public ConfiguracaoLeitura ObterConfiguracaoLeituraDigital(string port)
        {
            throw new NotImplementedException();
        }

        public ConfiguracaoLeituraDispositivo ObterConfiguracaoLeituraDispositivo(ushort index)
        {
            throw new NotImplementedException();
        }

        public ConfiguracaoLeituraDispositivo ObterConfiguracaoLeituraDispositivo()
        {
            throw new NotImplementedException();
        }

        public ITekonDispositivoDado Parse(DispositivoContextoLeitura context)
        {
            return new TWP_1AI
            {
                NumeroSerie = (context.HoldingRegisters![0] << 16 | context.HoldingRegisters[1]),

                Modelo = IdentificarModelo(context.HoldingRegisters[2]),

                RSSI = context.HoldingRegisters[3] / -2,

                PeriodoComunicacao = context.HoldingRegisters[4],
                TempoDecorrido = context.HoldingRegisters[5],

                TensaoAlimentacao = context.HoldingRegisters[6] / 10.0f,

                TemperaturaInterna = (float)
                    Math.Round(
                        Conversor.ToFloat(context.HoldingRegisters[8], context.HoldingRegisters[7]),
                        2
                    ),

                ValorEntradaAnalogica_1 = (float)
                    Math.Round(
                        Conversor.ToFloat(
                            context.HoldingRegisters[10],
                            context.HoldingRegisters[9]
                        ),
                        2
                    ),

                VersaoFirmware = context.HoldingRegisters[17],
                RevisaoVersao = context.HoldingRegisters[18],
                VersaoHardware = context.HoldingRegisters[19],
            };
        }

        private string IdentificarModelo(int modelo)
        {
            return modelo switch
            {
                47 => "TWP-1AI 868 MHZ",
                53 => "TWP-1AI 915 MHZ",
                _ => "Desconhecido",
            };
        }
    }
}
