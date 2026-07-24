using System.IO.Ports;
using System.Text.Json;
using Toolbox.Industrial.Driver.Tekon;
using Toolbox.Industrial.Driver.Tekon.Exceptions;
using Toolbox.Industrial.Driver.Tekon.Interfaces;
using Toolbox.Industrial.Driver.Tekon.Models;

namespace SoftwareIrrigacao.Features.Telemetria.Tekon
{
    public class WorkerTeste : BackgroundService
    {
        private readonly ITekonDriver _driver;
        private readonly ILogger<WorkerTeste> _logger;

        public WorkerTeste(ITekonDriverFactory factory, ILogger<WorkerTeste> logger)
        {
            _logger = logger;
            var config = new TekonDriverConfig
            {
                Porta = "COM6",
                BaudRate = 19200,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.Two,
                ReadTimeout = 1000,
                WriteTimeout = 1000,
            };
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
                    var TWP_4AI4DI1UT = await _driver.LerDispositivo(
                        modelo: modelo,
                        slaveAddress: slaveAddress,
                        index: index
                    );

                    var temperatura = await _driver.LerPortaTemperatura(
                        modelo: modelo,
                        slaveAddress: slaveAddress,
                        index: index,
                        "UT"
                    );

                    var analogica1 = await _driver.LerPortaAnalogica(
                        modelo: modelo,
                        slaveAddress: slaveAddress,
                        index: index,
                        "A1"
                    );
                    var analogica4 = await _driver.LerPortaAnalogica(
                        modelo: modelo,
                        slaveAddress: slaveAddress,
                        index: index,
                        "A2"
                    );
                    var analogica7 = await _driver.LerPortaAnalogica(
                        modelo: modelo,
                        slaveAddress: slaveAddress,
                        index: index,
                        "A3"
                    );
                    var analogica10 = await _driver.LerPortaAnalogica(
                        modelo: modelo,
                        slaveAddress: slaveAddress,
                        index: index,
                        "A4"
                    );

                    var entradaB1 = await _driver.LerPortaDigital(
                        modelo: modelo,
                        slaveAddress: slaveAddress,
                        index: index,
                        "B1"
                    );
                    var entradaB2 = await _driver.LerPortaDigital(
                        modelo: modelo,
                        slaveAddress: slaveAddress,
                        index: index,
                        "B2"
                    );
                    var entradaB3 = await _driver.LerPortaDigital(
                        modelo: modelo,
                        slaveAddress: slaveAddress,
                        index: index,
                        "B3"
                    );
                    var entradaB4 = await _driver.LerPortaDigital(
                        modelo: modelo,
                        slaveAddress: slaveAddress,
                        index: index,
                        "B4"
                    );

                    var saidaQ1 = await _driver.LerPortaDigital(
                        modelo: modelo,
                        slaveAddress: slaveAddress,
                        index: index,
                        "Q1"
                    );
                    var saidaQ2 = await _driver.LerPortaDigital(
                        modelo: modelo,
                        slaveAddress: slaveAddress,
                        index: index,
                        "Q2"
                    );
                    var saidaQ3 = await _driver.LerPortaDigital(
                        modelo: modelo,
                        slaveAddress: slaveAddress,
                        index: index,
                        "Q3"
                    );

                    await _driver.EscreverPortaDigital(
                        modelo: modelo,
                        slaveAddress: slaveAddress,
                        index,
                        port: "Q1",
                        value: true
                    );

                    Console.ForegroundColor = ConsoleColor.DarkGreen;

                    Console.WriteLine(
                        $"Modelo: {TWP_4AI4DI1UT.Modelo}    NumeroSerie: {TWP_4AI4DI1UT.NumeroSerie}"
                    );
                    Console.WriteLine(
                        JsonSerializer.Serialize(TWP_4AI4DI1UT.ObterMetricas(), options)
                    );

                    //Modulo WGW420
                    var analogicaA4 = await _driver.LerPortaAnalogica(
                        modelo: Modelos.WGW420,
                        slaveAddress: slaveAddress,
                        "A4"
                    );
                    var analogicaA5 = await _driver.LerPortaAnalogica(
                        modelo: Modelos.WGW420,
                        slaveAddress: slaveAddress,
                        "A5"
                    );
                    var analogicaA6 = await _driver.LerPortaAnalogica(
                        modelo: Modelos.WGW420,
                        slaveAddress: slaveAddress,
                        "A6"
                    );
                    var analogicaA10 = await _driver.LerPortaAnalogica(
                        modelo: Modelos.WGW420,
                        slaveAddress: slaveAddress,
                        "A10"
                    );
                    var analogicaA11 = await _driver.LerPortaAnalogica(
                        modelo: Modelos.WGW420,
                        slaveAddress: slaveAddress,
                        "A11"
                    );
                    var analogicaA12 = await _driver.LerPortaAnalogica(
                        modelo: Modelos.WGW420,
                        slaveAddress: slaveAddress,
                        "A12"
                    );
                    var analogicaA16 = await _driver.LerPortaAnalogica(
                        modelo: Modelos.WGW420,
                        slaveAddress: slaveAddress,
                        "A16"
                    );
                    var analogicaA17 = await _driver.LerPortaAnalogica(
                        modelo: Modelos.WGW420,
                        slaveAddress: slaveAddress,
                        "A17"
                    );
                }
                catch (TekonComunicacaoException ex)
                {
                    _logger.LogError(ex, "Erro de comunicação Tekon");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (TekonLeituraException ex)
                {
                    _logger.LogWarning(ex, "Erro de leitura Tekon");
                }
                catch (TekonEscritaException ex)
                {
                    _logger.LogWarning(ex, "Erro de escrita Tekon");
                }
                catch (TekonPortaInvalidaException ex)
                {
                    _logger.LogWarning(ex, "Porta inválida Tekon");
                }
                catch (TekonOperacaoNaoSuportadaException ex)
                {
                    _logger.LogWarning(ex, "Operação não suportada Tekon");
                }
                catch (TekonException ex)
                {
                    _logger.LogError(ex, "Erro Tekon");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro geral");
                }

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}
