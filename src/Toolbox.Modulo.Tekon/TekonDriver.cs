using Toolbox.Automacao.Core.Services.Modbus;
using Toolbox.Modulo.Tekon.Abstractions;
using Toolbox.Modulo.Tekon.Models;

namespace Toolbox.Modulo.Tekon
{
    public class TekonDriver
    {
        private readonly IModbusFacade _modbus;
        private readonly TekonDispositivoFactory _factory;

        public TekonDriver(
            IModbusFacade modbus)
        {
            _modbus = modbus;
            _factory = new TekonDispositivoFactory();
        }

        public async Task<ITekonDispositivoDado> ReadDevice(
            DispositivoSolicitacaoLeitura request)
        {

            var dispositivo = _factory.CriarModelo(request.Modelo);

            ushort[] holding = [];
            bool[] coils = [];

            var configuracaoHoldingRegisters = dispositivo.HoldingRegisters(request.Index);
            var configuracaoCoilRegisters = dispositivo.CoilRegisters(request.Index);


            if (configuracaoHoldingRegisters != null)
            {
                holding = await _modbus.LerHoldingRegistersAsync(
                    (byte) request.SlaveId,
                    configuracaoHoldingRegisters.StartAddress,
                    configuracaoHoldingRegisters.NumberOfRegister);
            }

            if (configuracaoCoilRegisters != null)
            {
                coils = await _modbus.LerCoilsAsync(
                    (byte) request.SlaveId,
                    configuracaoCoilRegisters.StartAddress,
                    configuracaoCoilRegisters.NumberOfRegister);
            }


            var contexto = new DispositivoContextoLeitura
            {
                HoldingRegisters = holding,
                CoilRegisters = coils
            };

            return dispositivo.Parse(contexto);
        }

        //public void WriteCoil(DispositivoSolicitacaoEscrita request)
        //{
        //    var device =
        //        _factory.Obter(request.Modelo);

        //    bool[] coils;

        //    var configuracao = device.CoilRegisters();

        //    if (configuracao != null)
        //    {

        //        coils =
        //        await _modbus.WriteCoil(
        //            request.SlaveId,
        //            request.Valor,
        //            configuracao.StartAddress);
        //    }


        //    var context = new DispositivoContextoLeitura
        //    {
        //        CoilRegisters = coils
        //    };


        //    return device.Parse(context);
        //}
    }
}
