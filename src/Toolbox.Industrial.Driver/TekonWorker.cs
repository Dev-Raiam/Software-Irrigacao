//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using System.Text.Json;

//namespace Toolbox.Modulo.Tekon;

//public class Parametros
//{
//    public byte SlaveAddress { get; set; }
//    public int BaudRate { get; set; }
//    public System.IO.Ports.Parity Parity { get; set; }
//    public System.IO.Ports.StopBits StopBits { get; set; }
//    public int DataBits { get; set; }
//    public int ReadTimeout { get; set; }
//    public int WriteTimeout { get; set; }
//}

//public class TekonWorker : BackgroundService
//{
//    private readonly IServiceProvider _serivceProvider;
//    private readonly IConfiguration _configuration;
//    private readonly ArmazenamentoAutomacao _armazenamento;
//    private readonly ILogger<TekonWorker> _logger;
//    private readonly JsonSerializerOptions _options;
//    private bool lerCoils = false;
//    private ushort[] buffer_holding = [];
//    private bool[] buffer_coils = [];
//    private string path_log;

//    public TekonWorker(
//        IServiceProvider serivceProvider,
//        IConfiguration configuration,
//        ArmazenamentoAutomacao armazenamento,
//        ILogger<TekonWorker> logger
//    )
//    {
//        _serivceProvider = serivceProvider;
//        _configuration = configuration;
//        _armazenamento = armazenamento;
//        _logger = logger;
//        path_log = _configuration["Log:Path"] ?? AppContext.BaseDirectory;
//        _options = new JsonSerializerOptions
//        {
//            WriteIndented = true,
//            IndentSize = 4,
//            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
//        };
//        Directory.CreateDirectory(path_log);
//    }

//    private async Task JsonParse(object objeto)
//    {
//        await File.WriteAllTextAsync(
//            Path.Combine(path_log, "tekon.json"),
//            JsonSerializer.Serialize(objeto, _options)
//        );
//    }

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {

//        var controlador = _armazenamento.Controlador;
//        var dispositivos = _armazenamento.Dispositivos;
//        var _modulos = _armazenamento.Modulos.Where(m => m.Marca == "Tekon").ToList();
//        var _interfaces = controlador?.Conexoes.Interfaces.ToList() ?? [];

//        var config_modbus = _modulos.FirstOrDefault(m => m.Master);

//        var json = JsonSerializer.Serialize(config_modbus!.Parametros.Parametro);
//        var parametros = JsonSerializer.Deserialize<Parametros>(json, _options);
//        var porta = _interfaces
//            .FirstOrDefault(i => i.Id == config_modbus.Conexoes.Conectado!.Canal.Id)!
//            .Endereco;

//        ModbusMaster? _modbus = null;
//        DriverTekon? driver = null;
//        bool conexaoModbusAtiva = false;

//        while (!stoppingToken.IsCancellationRequested)
//        {
//            try
//            {
//                if (_modbus is null)
//                {
//                    _modbus = new ModbusMaster(
//                        port: porta!,
//                        baudRate: parametros!.BaudRate,
//                        parity: parametros.Parity,
//                        stopBits: parametros.StopBits,
//                        dataBits: parametros.DataBits,
//                        readTimeout: parametros.ReadTimeout,
//                        writeTimeout: parametros.WriteTimeout
//                    );
//                }

//                if (!conexaoModbusAtiva)
//                {
//                    try
//                    {
//                        _modbus.OpenConnection();
//                        driver = new ModbusDriverTekon(_modbus);
//                        conexaoModbusAtiva = true;
//                    }
//                    catch (Exception ex)
//                    {
//                        _logger.LogError(
//                            ex,
//                            "Erro ao abrir a Porta Modbus. Tentando novamente em 5 segundos..."
//                        );
//                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
//                        continue;
//                    }
//                }

//                if (driver is null)
//                {
//                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
//                    continue;
//                }

//                byte slaveAddress = 0;
//                byte index = 0;

//                foreach (var modulo in _modulos)
//                {
//                    if (modulo.Parametros.PossuiParametros)
//                    {
//                        if (
//                            modulo.Parametros.Parametro.TryGetValue("slaveAddress", out var slaveId)
//                        )
//                            slaveAddress = (byte)((JsonElement)slaveId).GetInt32();

//                        if (modulo.Parametros.Parametro.TryGetValue("index", out var indice))
//                            index = (byte)((JsonElement)indice).GetInt32();
//                    }

//                    if (slaveAddress > 0)
//                    {
//                        buffer_holding = await driver.ReadHoldingRegistersAsync(
//                            slaveAddress,
//                            modulo.Modelo,
//                            index
//                        );

//                        lerCoils =
//                            modulo.Conexoes.Entradas.Any(p => p.Sinal == "Digital")
//                            || modulo.Conexoes.Saidas.Any(p => p.Sinal == "Digital");

//                        if (lerCoils && index != 0)
//                        {
//                            buffer_coils = await driver.ReadCoilsRegistersAsync(
//                                slaveAddress,
//                                modulo.Modelo,
//                                index
//                            );
//                        }

//                        var telemetria = driver.DecodificarModeloHoldingRegisters();

//                        await JsonParse(telemetria);

//                        if (modulo.Conexoes.Entradas.Any())
//                        {
//                            foreach (var porta_entrada in modulo.Conexoes.Entradas)
//                            {
//                                if (porta_entrada.Sinal == "Digital")
//                                {
//                                    var offset =
//                                        int.Parse(
//                                            porta_entrada
//                                                .Parametros.Parametro["startAddress"]
//                                                .ToString()!
//                                        ) - driver.ConfigReadCoils!.StartAddress;
//                                    var valor = buffer_holding[offset];
//                                    var dispositivo = dispositivos.FirstOrDefault(d =>
//                                        d.Id == porta_entrada.Conectado!.Id
//                                    );

//                                    await JsonParse(
//                                        new { Dispositivo = dispositivo, Valor = valor }
//                                    );
//                                }

//                                if (porta_entrada.Sinal == "Anal�gico")
//                                {
//                                    var offset =
//                                        int.Parse(
//                                            porta_entrada
//                                                .Parametros.Parametro["startAddress"]
//                                                .ToString()!
//                                        ) - driver.ConfigReadHoldingRegister!.StartAddress;
//                                    var valor = Conversor.ToFloat(
//                                        buffer_holding[offset + 1],
//                                        buffer_holding[offset]
//                                    );

//                                    var dispositivo = dispositivos.FirstOrDefault(d =>
//                                        d.Id == porta_entrada.Conectado!.Id
//                                    );
//                                    await JsonParse(
//                                        new { Dispositivo = dispositivo, Valor = valor }
//                                    );
//                                }
//                                if (porta_entrada.Sinal == "Temperatura")
//                                {
//                                    var offset =
//                                        int.Parse(
//                                            porta_entrada
//                                                .Parametros.Parametro["startAddress"]
//                                                .ToString()!
//                                        ) - driver.ConfigReadHoldingRegister!.StartAddress;
//                                    var valor = Conversor.ToFloat(
//                                        buffer_holding[offset + 1],
//                                        buffer_holding[offset]
//                                    );

//                                    var dispositivo = dispositivos.FirstOrDefault(d =>
//                                        d.Id == porta_entrada.Conectado!.Id
//                                    );

//                                    await JsonParse(
//                                        new { Dispositivo = dispositivo, Valor = valor }
//                                    );
//                                }
//                            }
//                        }
//                        if (modulo.Conexoes.Saidas.Any())
//                        {
//                            foreach (var porta_saida in modulo.Conexoes.Entradas)
//                            {
//                                if (porta_saida.Sinal == "Digital")
//                                {
//                                    var offset =
//                                        int.Parse(
//                                            porta_saida
//                                                .Parametros.Parametro["startAddress"]
//                                                .ToString()!
//                                        ) - driver.ConfigReadCoils!.StartAddress;
//                                    var valor = buffer_holding[offset];

//                                    var dispositivo = dispositivos.FirstOrDefault(d =>
//                                        d.Id == porta_saida.Conectado!.Id
//                                    );

//                                    await JsonParse(
//                                        new { Dispositivo = dispositivo, Valor = valor }
//                                    );
//                                }

//                                if (porta_saida.Sinal == "Anal�gico")
//                                {
//                                    var offset =
//                                        int.Parse(
//                                            porta_saida
//                                                .Parametros.Parametro["startAddress"]
//                                                .ToString()!
//                                        ) - driver.ConfigReadHoldingRegister!.StartAddress;
//                                    var valor = Conversor.ToFloat(
//                                        buffer_holding[offset + 1],
//                                        buffer_holding[offset]
//                                    );

//                                    var dispositivo = dispositivos.FirstOrDefault(d =>
//                                        d.Id == porta_saida.Conectado!.Id
//                                    );

//                                    await JsonParse(
//                                        new { Dispositivo = dispositivo, Valor = valor }
//                                    );
//                                }
//                            }
//                        }

//                        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
//                    }
//                }
//            }
//            catch (OperationCanceledException)
//            {
//                break;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Erro ao publicar telemetria");
//                conexaoModbusAtiva = false;
//            }

//            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
//        }
//    }
//}
