using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Toolbox.Modulo.Tekon;
using Toolbox.Modulo.Tekon.Interfaces;
using Toolbox.Modulo.Tekon.Models;

namespace SoftwareIrrigacao.Features.Telemetria.Tekon
{
    public class WorkerTeste : BackgroundService
    {
        private readonly ITekonDriver _tekonDriver;
        private bool flag = false;

        public WorkerTeste(ITekonDriverFactory factory)
        {
            var config = new TekonDriverConfig();
            _tekonDriver = factory.CriarDriver(config);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested) 
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine("Iniciando o Worker Teste");

                    //var TWP_4AI4DI1UT = await _tekonDriver.LerDispositivo(modelo: TekonConstants.Modelos.TWP_4AI4DI1UT, slaveId: 1, index: 5);

                    if (flag) 
                    {
                        await _tekonDriver.EscreverPortaDigital(modelo: TekonConstants.Modelos.TWP_4AI4DI1UT, slaveAddress: 1, 5, port: "Q1", value: true);
                        flag = false;
                    }
                    else 
                    {
                        await _tekonDriver.EscreverPortaDigital(modelo: TekonConstants.Modelos.TWP_4AI4DI1UT, slaveAddress: 1, 5, port: "Q1", value: false);
                        flag = true;
                    }


                    //var options = new JsonSerializerOptions { WriteIndented = true, IndentSize = 4 };

                    //Console.ForegroundColor = ConsoleColor.DarkGreen;

                    //Console.WriteLine($"Modelo: {TWP_4AI4DI1UT.Modelo}    NumeroSerie: {TWP_4AI4DI1UT.NumeroSerie}");
                    //Console.WriteLine(JsonSerializer.Serialize(TWP_4AI4DI1UT.ObterMetricas(), options));

                    //Console.WriteLine($"Modelo: {WGW420.Modelo}    NumeroSerie: {WGW420.NumeroSerie}");
                    //Console.WriteLine(JsonSerializer.Serialize(WGW420.ObterMetricas(), options));

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
                catch (Exception ex) 
                {

                }
            }
        }
    }
}
