using System.Text.Json;
using Toolbox.Automacao.Core.Services.Modbus;
using Toolbox.Modulo.Tekon.Models;

namespace Toolbox.Modulo.Tekon
{
    public class TesteWGW420
    {
        private readonly IModbusFacade _modbus;
        private readonly TekonDriver _driver;

        public TesteWGW420(IModbusFacadeFactory factoryModbus)
        {
            _modbus = factoryModbus.CriarRtuMaster(new ModbusConfig());
            _driver = new TekonDriver(_modbus);
        }
        public async Task Ler()
        {
            _modbus.Conectar();

            var TWP_4AI4DI1UT = await _driver.ReadDevice(
                new DispositivoSolicitacaoLeitura(modelo: TekonConstants.Modelos.TWP_4AI4DI1UT, slaveId: 1, index: 5));

            var WGW420 = await _driver.ReadDevice(
                new DispositivoSolicitacaoLeitura(modelo: TekonConstants.Modelos.WGW420, slaveId: 1));

            var options = new JsonSerializerOptions { WriteIndented = true, IndentSize = 4 };

            Console.ForegroundColor = ConsoleColor.DarkGreen;

            Console.WriteLine($"Modelo: {TWP_4AI4DI1UT.Modelo}    NumeroSerie: {TWP_4AI4DI1UT.NumeroSerie}");
            Console.WriteLine(JsonSerializer.Serialize(TWP_4AI4DI1UT.ObterMetricas(),options));

            Console.ForegroundColor = ConsoleColor.DarkRed;

            Console.WriteLine($"Modelo: {WGW420.Modelo}    NumeroSerie: {WGW420.NumeroSerie}");
            Console.WriteLine(JsonSerializer.Serialize(WGW420.ObterMetricas(), options));
        }
    }
}