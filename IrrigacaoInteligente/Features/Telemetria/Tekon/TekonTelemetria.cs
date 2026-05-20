// using System.Text.Encodings.Web;
// using System.Text.Json;
// using IrrigacaoInteligente.Infrastructure.Data;
// using IrrigacaoInteligente.Infrastructure.Mqtt;
// using IrrigacaoInteligente.State;
// using Toolbox.Automacao.Irrigacao.Modbus;
// using Toolbox.Core.Messages;

// namespace IrrigacaoInteligente.Features.Telemetria
// {
//     public class StartTelemetriaTekon : Command { }

//     public class TekonHandler
//     {
//         private readonly MqttClienteLocal _mqttClienteLocal;
//         private readonly ArmazenamentoAutomacao _armazenamentoAutomacao;
//         private readonly IrrigacaoInteligenteContext _context;
//         private readonly State.Controlador _controlador;
//         private readonly JsonSerializerOptions _jsonOptions = new()
//         {
//             Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
//             WriteIndented = true,
//             IndentSize = 4,
//         };
//         private readonly ModbusGerenciador _conexao_1;

//         public TekonHandler(
//             MqttClienteLocal mqttClienteLocal,
//             ArmazenamentoAutomacao armazenamentoAutomacao,
//             IrrigacaoInteligenteContext context
//         )
//         {
//             _mqttClienteLocal = mqttClienteLocal;
//             _armazenamentoAutomacao = armazenamentoAutomacao;
//             _controlador = armazenamentoAutomacao.Controladores.First(c => c.Master == true);
//             _context = context;
//             _conexao_1 = new ModbusGerenciador(
//                 port: "COM6",
//                 baudRate: 19200,
//                 parity: System.IO.Ports.Parity.None,
//                 stopBits: System.IO.Ports.StopBits.Two,
//                 dataBits: 8,
//                 readTimeout: 2000,
//                 writeTimeout: 2000
//             );

//             _conexao_1.OpenConnection();
//         }

//         public async Task<bool> Executar(CancellationToken cancellationToken)
//         {
//             try
//             {
//                 //Main.start();
//                 foreach (var modulo in _controlador.Modulos)
//                 {
//                     if (modulo.Conexoes.Interfaces.Any())
//                     {
//                         foreach (var dispositivo in modulo.Conexoes.Interfaces)
//                         {
//                             Console.WriteLine($"Dispositivo: {dispositivo.Tipo}");
//                             ushort[]? registradores = await _conexao_1.ReadHoldingRegistersAsync(
//                                 1,
//                                 1080,
//                                 20
//                             );
//                             Console.WriteLine($"Registradores: {string.Join(", ", registradores)}");
//                             // var configuracaoLeitura =
//                             //     GerenciadorConfiguracao.ObterConfiguracaoLeitura(
//                             //         modulo.Marca,
//                             //         dispositivo.Modelo,
//                             //         dispositivo.Indice
//                             //     );

//                             // ushort[]? registradores = await conexao_1.ReadHoldingRegistersAsync(
//                             //     modulo.SlaveAddress,
//                             //     configuracaoLeitura.StartAddress,
//                             //     configuracaoLeitura.NumberOfRegister
//                             // );

//                             // if (registradores != null)
//                             // {
//                             //     var telemetria = GerenciadorConfiguracao.CriarTelemetria(
//                             //         modulo.Id,
//                             //         dispositivo.Modelo,
//                             //         registradores
//                             //     );
//                             //     Telemetrias.AddRange(telemetria);
//                             //     Console.WriteLine(
//                             //         JsonSerializer.Serialize(
//                             //             telemetria,
//                             //             options: new JsonSerializerOptions
//                             //             {
//                             //                 WriteIndented = true,
//                             //                 IndentSize = 4,
//                             //                 Encoder = System
//                             //                     .Text
//                             //                     .Encodings
//                             //                     .Web
//                             //                     .JavaScriptEncoder
//                             //                     .UnsafeRelaxedJsonEscaping,
//                             //             }
//                             //         )
//                             //     );
//                             // }
//                         }
//                         // Console.WriteLine(
//                         //     JsonSerializer.Serialize(
//                         //         Telemetrias,
//                         //         new JsonSerializerOptions
//                         //         {
//                         //             WriteIndented = true,
//                         //             IndentSize = 4,
//                         //             Encoder = System
//                         //                 .Text
//                         //                 .Encodings
//                         //                 .Web
//                         //                 .JavaScriptEncoder
//                         //                 .UnsafeRelaxedJsonEscaping,
//                         //         }
//                         //     )
//                         // );
//                     }
//                     //Console.WriteLine("Ligando Porta Remota");

//                     //await conexao_1.WriteSingleCoilAsync(modulo.SlaveAddress, 0, true);

//                     await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

//                     //Console.WriteLine("Desligando Porta Remota");

//                     //await conexao_1.WriteSingleCoilAsync(modulo.SlaveAddress, 0, false);

//                     //await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"{ex.Message}");
//             }

//             return true;
//         }
//     }
// }
