using System.Text.Json;
using IrrigacaoInteligente.Features.Telemetria;
using IrrigacaoInteligente.State;
using Toolbox.Automacao.Irrigacao.Marcas;
using Toolbox.Automacao.Irrigacao.Modbus;
using Toolbox.Core.Mediator;

namespace IrrigacaoInteligente.Workers.Telemetria;

public class TekonWorker : BackgroundService
{
    private readonly Aplicacao _aplicacao;
    private readonly IServiceProvider _serivceProvider;
    private readonly ArmazenamentoAutomacao _armazenamento;
    private readonly ILogger<TekonWorker> _logger;

    public TekonWorker(
        Aplicacao aplicacao,
        IServiceProvider serivceProvider,
        ArmazenamentoAutomacao armazenamento,
        ILogger<TekonWorker> logger
    )
    {
        _aplicacao = aplicacao;
        _serivceProvider = serivceProvider;
        _armazenamento = armazenamento;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _aplicacao.AguardarLiberacaoAplicacao(stoppingToken);

        var _conexao_1 = new ModbusGerenciador(
            port: "COM6",
            baudRate: 19200,
            parity: System.IO.Ports.Parity.None,
            stopBits: System.IO.Ports.StopBits.Two,
            dataBits: 8,
            readTimeout: 2000,
            writeTimeout: 2000
        );
        _conexao_1.OpenConnection();

        var controlador = _armazenamento.Controladores.First(c => c.Master == true);
        var _modulos = controlador.Modulos.Where(m => m.Marca == "Tekon").ToList();
        var _interfaces = controlador.Conexoes.Interfaces.ToList();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var slaveAddress = 0;
                var index = 0;
                foreach (var modulo in _modulos)
                {
                    if (modulo.Parametros.PossuiParametros)
                    {
                        try
                        {
                            if (
                                modulo.Parametros.Parametro.TryGetValue(
                                    "slaveAddress",
                                    out var slaveId
                                )
                            )
                            {
                                slaveAddress = ((JsonElement)slaveId).GetInt32();
                            }
                            if (modulo.Parametros.Parametro.TryGetValue("index", out var indice))
                            {
                                index = ((JsonElement)indice).GetInt32();
                            }

                            //var slid = ((JsonElement)modulo.Parametros.Parametro["slaveAddress"]).GetInt32();
                            //var idx = (JsonElement)modulo.Parametros.Parametro["index"];

                            //var idx_valor = idx.GetInt32();
                        }
                        catch (Exception ex) { }
                    }

                    // Se nao existir Index nos parametros de modulo siguinifica que eu quero os metadados dele
                    if (slaveAddress > 0)
                    {
                        // if (modulo.Conexoes.Interfaces.Any())
                        // {
                        //     foreach (var interfaceItem in modulo.Conexoes.Interfaces)
                        //     {
                        //         if (interfaceItem.Conectados.Any())
                        //         {
                        //             foreach (var conectado in interfaceItem.Conectados)
                        //             {
                        //                 if (conectado.Tipo == "Módulo")
                        //                 {
                        //                     var moduloFilho = _modulos.FirstOrDefault(m =>
                        //                         m.Id == conectado.Id
                        //                     );

                        //                     if (moduloFilho != null)
                        //                     {
                        //                         Console.WriteLine(
                        //                             $"Módulo Filho: {JsonSerializer.Serialize(moduloFilho, new JsonSerializerOptions { WriteIndented = true, IndentSize = 4 })}"
                        //                         );
                        //                     }
                        //                 }
                        //             }
                        //         }
                        //     }
                        // }

                        //{
                        //    Modelo ,
                        //    Marca,
                        //    slaveAddress,Qual Modulo Ler
                        //    indice, Qual informação
                        //}

                        var configuracaoLeitura = GerenciadorConfiguracao.ObterConfiguracaoLeitura(
                            modulo.Marca,
                            modulo.Modelo,
                            1
                        );

                        var configuracaoLeituraCoils =
                            GerenciadorConfiguracao.ObterConfiguracaoLeituraCoils(
                                modulo.Marca,
                                modulo.Modelo,
                                1
                            );

                        // Modulo SlaveId 2 e Index 1
                        // Le as Metricas dele apenas os metadatas
                        // Em seguida Ler as entradas onde tera
                        // porta analogica ou digital local do dado da porta StartAdrees 190 [registradores 2]
                        // executo uma leitura x das portas de entradas o monto o pay pego dispositivo vinculado a ela

                        ushort[]? buffer_1 = await _conexao_1.ReadHoldingRegistersAsync(
                            (byte)slaveAddress,
                            configuracaoLeitura.StartAddress,
                            configuracaoLeitura.NumberOfRegister
                        );

                        bool[]? buffer_2 = await _conexao_1.ReadCoilsRegistersAsync(
                            (byte)slaveAddress,
                            configuracaoLeituraCoils.StartAddress, // --> 89
                            configuracaoLeituraCoils.NumberOfRegister // 4 --> 89, 90, 91,92, (12,34,54,21)  --> 89, 90, 91,92 (12,54,21,34)
                        );
                        /// Buffer 2 [12, 34, 54, 21] (0 --> 12), (1 --> 34), (2 --> 54), (3 --> 21)
                        await _conexao_1.WriteSingleCoilAsync((byte)slaveAddress, 0, true);

                        if (buffer_1 != null)
                        {
                            var telemetria = GerenciadorConfiguracao.CriarTelemetria(
                                modulo.Id,
                                modulo.Modelo,
                                buffer_1
                            );

                            Console.WriteLine(
                                JsonSerializer.Serialize(
                                    telemetria,
                                    options: new JsonSerializerOptions
                                    {
                                        WriteIndented = true,
                                        IndentSize = 4,
                                        Encoder = System
                                            .Text
                                            .Encodings
                                            .Web
                                            .JavaScriptEncoder
                                            .UnsafeRelaxedJsonEscaping,
                                    }
                                )
                            );
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar telemetria");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
