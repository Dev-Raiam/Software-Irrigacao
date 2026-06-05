using System.Text.Json;
using IrrigacaoInteligente.State;
using Toolbox.Automacao.Irrigacao.Drivers;
using Toolbox.Automacao.Irrigacao.Marcas.Tekon;
using Toolbox.Automacao.Irrigacao.Modbus;

namespace IrrigacaoInteligente.Workers.Telemetria;

public class Parametros
{
    public byte SlaveAddress { get; set; }
    public int BaudRate { get; set; }
    public System.IO.Ports.Parity Parity { get; set; }
    public System.IO.Ports.StopBits StopBits { get; set; }
    public int DataBits { get; set; }
    public int ReadTimeout { get; set; }
    public int WriteTimeout { get; set; }
}

public class TekonWorker : BackgroundService
{
    private readonly Aplicacao _aplicacao;
    private readonly IServiceProvider _serivceProvider;
    private readonly ArmazenamentoAutomacao _armazenamento;
    private readonly ILogger<TekonWorker> _logger;
    private bool lerCoils = false;
    private ushort[] buffer_holding = [];
    private bool[] buffer_coils = [];

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

    private void JsonParse(object objeto)
    {
        Console.WriteLine(
            JsonSerializer.Serialize(
                objeto,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true,
                    IndentSize = 4,
                }
            )
        );
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _aplicacao.AguardarLiberacaoAplicacao(stoppingToken);

        var controlador = _armazenamento.Controladores.First(c => c.Master == true);
        var dispositivos = controlador.Dispositivos;
        var _modulos = controlador.Modulos.Where(m => m.Marca == "Tekon").ToList();
        var _interfaces = controlador.Conexoes.Interfaces.ToList();

        var config_modbus = _modulos.FirstOrDefault(m => m.Master);

        var json = JsonSerializer.Serialize(config_modbus!.Parametros.Parametro);
        var parametros = JsonSerializer.Deserialize<Parametros>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        var porta = _interfaces
            .FirstOrDefault(i => i.Id == config_modbus.Conexoes.Conectado!.Canal.Id)!
            .Endereco;

        using var _modbus = new ModbusMaster(
            port: porta!,
            baudRate: parametros!.BaudRate,
            parity: parametros.Parity,
            stopBits: parametros.StopBits,
            dataBits: parametros.DataBits,
            readTimeout: parametros.ReadTimeout,
            writeTimeout: parametros.WriteTimeout
        );
        try
        {
            _modbus.OpenConnection();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao abrir a Porta: {ex}");
            return;
        }
        var driver = new ModbusDriverTekon(_modbus);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                byte slaveAddress = 0;
                byte index = 0;

                foreach (var modulo in _modulos)
                {
                    if (modulo.Parametros.PossuiParametros)
                    {
                        if (
                            modulo.Parametros.Parametro.TryGetValue("slaveAddress", out var slaveId)
                        )
                            slaveAddress = (byte)((JsonElement)slaveId).GetInt32();

                        if (modulo.Parametros.Parametro.TryGetValue("index", out var indice))
                            index = (byte)((JsonElement)indice).GetInt32();
                    }

                    if (slaveAddress > 0)
                    {
                        buffer_holding = await driver.ReadHoldingRegistersAsync(
                            slaveAddress,
                            modulo.Modelo,
                            index
                        );

                        lerCoils =
                            modulo.Conexoes.Entradas.Any(p => p.Sinal == "Digital")
                            || modulo.Conexoes.Saidas.Any(p => p.Sinal == "Digital");

                        if (lerCoils && index != 0)
                        {
                            buffer_coils = await driver.ReadCoilsRegistersAsync(
                                slaveAddress,
                                modulo.Modelo,
                                index
                            );
                        }

                        var telemetria = driver.DecodificarModeloHoldingRegisters();

                        JsonParse(telemetria);

                        if (modulo.Conexoes.Entradas.Any())
                        {
                            foreach (var porta_entrada in modulo.Conexoes.Entradas)
                            {
                                if (porta_entrada.Sinal == "Digital")
                                {
                                    var offset =
                                        int.Parse(
                                            porta_entrada
                                                .Parametros.Parametro["startAddress"]
                                                .ToString()!
                                        ) - driver.ConfigReadCoils!.StartAddress;
                                    var valor = buffer_holding[offset];
                                    var dispositivo = dispositivos.FirstOrDefault(d =>
                                        d.Id == porta_entrada.Conectado!.Id
                                    );

                                    JsonParse(new { Dispositivo = dispositivo, Valor = valor });
                                }

                                if (porta_entrada.Sinal == "Analógico")
                                {
                                    var offset =
                                        int.Parse(
                                            porta_entrada
                                                .Parametros.Parametro["startAddress"]
                                                .ToString()!
                                        ) - driver.ConfigReadHoldingRegister!.StartAddress;
                                    var valor = Conversor.ToFloat(
                                        buffer_holding[offset + 1],
                                        buffer_holding[offset]
                                    );

                                    var dispositivo = dispositivos.FirstOrDefault(d =>
                                        d.Id == porta_entrada.Conectado!.Id
                                    );
                                    JsonParse(new { Dispositivo = dispositivo, Valor = valor });
                                }
                                if (porta_entrada.Sinal == "Temperatura")
                                {
                                    var offset =
                                        int.Parse(
                                            porta_entrada
                                                .Parametros.Parametro["startAddress"]
                                                .ToString()!
                                        ) - driver.ConfigReadHoldingRegister!.StartAddress;
                                    var valor = Conversor.ToFloat(
                                        buffer_holding[offset + 1],
                                        buffer_holding[offset]
                                    );

                                    var dispositivo = dispositivos.FirstOrDefault(d =>
                                        d.Id == porta_entrada.Conectado!.Id
                                    );

                                    JsonParse(new { Dispositivo = dispositivo, Valor = valor });
                                }
                            }
                        }
                        if (modulo.Conexoes.Saidas.Any())
                        {
                            foreach (var porta_saida in modulo.Conexoes.Entradas)
                            {
                                if (porta_saida.Sinal == "Digital")
                                {
                                    var offset =
                                        int.Parse(
                                            porta_saida
                                                .Parametros.Parametro["startAddress"]
                                                .ToString()!
                                        ) - driver.ConfigReadCoils!.StartAddress;
                                    var valor = buffer_holding[offset];

                                    var dispositivo = dispositivos.FirstOrDefault(d =>
                                        d.Id == porta_saida.Conectado!.Id
                                    );

                                    JsonParse(new { Dispositivo = dispositivo, Valor = valor });
                                }

                                if (porta_saida.Sinal == "Analógico")
                                {
                                    var offset =
                                        int.Parse(
                                            porta_saida
                                                .Parametros.Parametro["startAddress"]
                                                .ToString()!
                                        ) - driver.ConfigReadHoldingRegister!.StartAddress;
                                    var valor = Conversor.ToFloat(
                                        buffer_holding[offset + 1],
                                        buffer_holding[offset]
                                    );

                                    var dispositivo = dispositivos.FirstOrDefault(d =>
                                        d.Id == porta_saida.Conectado!.Id
                                    );

                                    JsonParse(new { Dispositivo = dispositivo, Valor = valor });
                                }
                            }
                        }

                        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    }
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
