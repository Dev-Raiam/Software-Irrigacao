using System.Text.Json;
using Toolbox.Modulo.Tekon;
using Toolbox.Modulo.Tekon.Interfaces;
using Toolbox.Modulo.Tekon.Models;

namespace SoftwareIrrigacao.Features.Telemetria.Tekon
{
    public class WorkerTeste : BackgroundService
    {
        private readonly ITekonDriver _driver;
        private bool flag = false;

        public WorkerTeste(ITekonDriverFactory factory)
        {
            var config = new TekonDriverConfig();
            _driver = factory.CriarDriver(config);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Iniciando o Worker Teste");
            
            var modelo = Modelos.TWP_4AI4DI1UT;
            byte slaveAddress = 1;
            byte index = 5;

            var options = new JsonSerializerOptions { WriteIndented = true, IndentSize = 4 };

            while (!stoppingToken.IsCancellationRequested) 
            {
                try
                {
                    ///Modulo TWP_4AI4DI1UT
                    var TWP_4AI4DI1UT = await _driver.LerDispositivo(modelo: modelo, slaveAddress: slaveAddress, index: index);
                    
                    var temperatura = await _driver.LerPortaTemperatura(modelo: modelo, slaveAddress: slaveAddress, index: index, "UT");

                    var analogica1 = await _driver.LerPortaAnalogica(modelo: modelo, slaveAddress: slaveAddress, index: index, "A1");
                    var analogica4 = await _driver.LerPortaAnalogica(modelo: modelo, slaveAddress: slaveAddress, index: index, "A4");
                    var analogica7 = await _driver.LerPortaAnalogica(modelo: modelo, slaveAddress: slaveAddress, index: index, "A7");
                    var analogica10 = await _driver.LerPortaAnalogica(modelo: modelo, slaveAddress: slaveAddress, index: index, "A10");

                    var entradaB1 = await _driver.LerPortaDigital(modelo: modelo, slaveAddress: slaveAddress, index: index, "B1");
                    var entradaB2 = await _driver.LerPortaDigital(modelo: modelo, slaveAddress: slaveAddress, index: index, "B2");
                    var entradaB3 = await _driver.LerPortaDigital(modelo: modelo, slaveAddress: slaveAddress, index: index, "B3");
                    var entradaB4 = await _driver.LerPortaDigital(modelo: modelo, slaveAddress: slaveAddress, index: index, "B4");

                    var saidaQ1 = await _driver.LerPortaDigital(modelo: modelo, slaveAddress: slaveAddress, index: index, "Q1");
                    var saidaQ2 = await _driver.LerPortaDigital(modelo: modelo, slaveAddress: slaveAddress, index: index, "Q2");
                    var saidaQ3 = await _driver.LerPortaDigital(modelo: modelo, slaveAddress: slaveAddress, index: index, "Q3");

                    await _driver.EscreverPortaDigital(modelo: modelo, slaveAddress: slaveAddress, index, port: "Q1", value: true);

                    Console.ForegroundColor = ConsoleColor.DarkGreen;

                    Console.WriteLine($"Modelo: {TWP_4AI4DI1UT.Modelo}    NumeroSerie: {TWP_4AI4DI1UT.NumeroSerie}");
                    Console.WriteLine(JsonSerializer.Serialize(TWP_4AI4DI1UT.ObterMetricas(), options));


                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
                catch (Exception ex) 
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
