using System.Text.Json;
using IrrigacaoInteligente.State;
using Toolbox.Automacao.Irrigacao.Drivers;
using Toolbox.Automacao.Irrigacao.Modbus;

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

        var _conexao_1 = new ModbusMaster(
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
                byte slaveAddress = 0;
                byte index = 0;
                //ushort[]? buffer_holding_registers = null;
                //bool[]? buffer_coils_registers = null;

                //ConfiguracaoLeitura configuracao_holding_register;
                //ConfiguracaoLeitura configuracao_coils_registers;

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
                    // Modulo Pai
                    //if(slaveAddress > 0 && index == 0)
                    //{
                    //    /// Leitura de Valores e Configuracoens

                    //    // Leitura de Estados das portas
                    //    //var configuracaoLeituraCoils =
                    //    //    GerenciadorConfiguracao.ObterConfiguracaoLeituraCoils(
                    //    //        modulo.Marca,
                    //    //        modulo.Modelo,
                    //    //        null
                    //    //    );
                    //    // Teste

                    //    configuracao_holding_register = GerenciadorConfiguracao.ObterConfiguracaoLeitura(
                    //        modulo.Marca,
                    //        modulo.Modelo
                    //    );

                    //    configuracao_coils_registers = GerenciadorConfiguracao.ObterConfiguracaoLeituraCoils(
                    //        modulo.Marca,
                    //        modulo.Modelo
                    //    );

                    //    await _conexao_1.WriteSingleCoilAsync(slaveAddress, 0, false);

                    //    buffer_holding_registers = await _conexao_1.ReadHoldingRegistersAsync(
                    //        slaveAddress,
                    //        configuracao_holding_register.StartAddress,
                    //        configuracao_holding_register.NumberOfRegister
                    //    );

                    //    buffer_coils_registers = await _conexao_1.ReadCoilsRegistersAsync(
                    //        slaveAddress,
                    //        configuracao_holding_register.StartAddress,
                    //        configuracao_holding_register.NumberOfRegister
                    //    );

                    //    if (buffer_holding_registers != null)
                    //    {
                    //        var telemetria = GerenciadorConfiguracao.CriarTelemetria(
                    //            modulo.Id,
                    //            modulo.Modelo,
                    //            buffer_holding_registers
                    //        );

                    //        Console.WriteLine(
                    //            JsonSerializer.Serialize(
                    //                telemetria,
                    //                options: new JsonSerializerOptions
                    //                {
                    //                    WriteIndented = true,
                    //                    IndentSize = 4,
                    //                    Encoder = System
                    //                        .Text
                    //                        .Encodings
                    //                        .Web
                    //                        .JavaScriptEncoder
                    //                        .UnsafeRelaxedJsonEscaping,
                    //                }
                    //            )
                    //        );
                    //    }

                    //    //buffer_coils_registers = await _conexao_1.ReadCoilsRegistersAsync(
                    //    //    slaveAddress,
                    //    //    configuracaoLeituraCoils.StartAddress,
                    //    //    configuracaoLeituraCoils.NumberOfRegister
                    //    //);
                    //}

                    if (slaveAddress > 0)
                    {
                        /// Leitura de Valores e Configuracoens
                        //configuracao_holding_register = GerenciadorConfiguracao.ObterConfiguracaoLeitura(
                        //    modulo.Marca,
                        //    modulo.Modelo,
                        //    index
                        //);

                        //// Leitura de Estados das portas
                        //configuracao_coils_registers =
                        //    GerenciadorConfiguracao.ObterConfiguracaoLeituraCoils(
                        //        modulo.Marca,
                        //        modulo.Modelo,
                        //        index
                        //    );
                        // Teste
                        //await _conexao_1.WriteSingleCoilAsync(slaveAddress, 0, true);

                        //var configuracoes_leituras =
                        //    GerenciadorConfiguracao.ObterConfiguracaoLeitura(
                        //        modulo.Marca,
                        //        modulo.Modelo,
                        //        index
                        //    );
                        //var map_configuracoes_leituras = configuracoes_leituras.ToDictionary(c =>
                        //    c.Tipo
                        //);
                        //var driver = new ModbusDriver(configuracoes_leituras);

                        var driver = new ModbusDriverTekon(_conexao_1);

                        var buffer_holding = await driver.ReadHoldingRegistersAsync(
                            slaveAddress,
                            modulo.Modelo,
                            index
                        );
                        var telemetria = driver.DecodificarModeloHoldingRegisters();

                        //var buffer_coils = await driver.ReadCoilsRegistersAsync(slaveAddress, modulo.Modelo, index);

                        //foreach (var configuracao_leitura in configuracoes_leituras)
                        //{
                        //    if (configuracao_leitura.Tipo == TipoLeitura.HoldingRegister)
                        //    {
                        //        buffer_holding_registers =
                        //            await _conexao_1.ReadHoldingRegistersAsync(
                        //                slaveAddress,
                        //                configuracao_leitura.StartAddress,
                        //                configuracao_leitura.NumberOfRegister
                        //            );
                        //    }
                        //    else if (configuracao_leitura.Tipo == TipoLeitura.Coils)
                        //    {
                        //        buffer_coils_registers = await _conexao_1.ReadCoilsRegistersAsync(
                        //            slaveAddress,
                        //            configuracao_leitura.StartAddress,
                        //            configuracao_leitura.NumberOfRegister
                        //        );
                        //    }
                        //}

                        //buffer_holding_registers = await _conexao_1.ReadHoldingRegistersAsync(
                        //    slaveAddress,
                        //    configuracao_holding_register.StartAddress,
                        //    configuracao_holding_register.NumberOfRegister
                        //);

                        //buffer_coils_registers = await _conexao_1.ReadCoilsRegistersAsync(
                        //    slaveAddress,
                        //    configuracao_coils_registers.StartAddress,
                        //    configuracao_coils_registers.NumberOfRegister
                        //);

                        //    if (buffer_holding_registers != null)
                        //    {
                        //        // Telemetria do Modulo
                        //        var telemetria = GerenciadorConfiguracao.CriarTelemetria(
                        //            modulo.Id,
                        //            modulo.Modelo,
                        //            buffer_holding_registers
                        //        );

                        //        Console.WriteLine(
                        //            JsonSerializer.Serialize(
                        //                telemetria,
                        //                options: new JsonSerializerOptions
                        //                {
                        //                    WriteIndented = true,
                        //                    IndentSize = 4,
                        //                    Encoder = System
                        //                        .Text
                        //                        .Encodings
                        //                        .Web
                        //                        .JavaScriptEncoder
                        //                        .UnsafeRelaxedJsonEscaping,
                        //                }
                        //            )
                        //        );

                        //foreach (var porta_saida in modulo.Conexoes.Saidas)
                        //{
                        //    if (porta_saida.Endereco != null && porta_saida.Conectado != null && porta_saida.Sinal == "Analogico")
                        //    {
                        //        var offset = int.Parse(porta_saida.Endereco) - configuracaoLeitura.StartAddress;

                        //        var valor = Conversor.ToFloat(buffer_1[offset + 1], buffer_1[offset]);

                        //        var dispositivo = controlador.Dispositivos.FirstOrDefault(d => d.Id == porta_saida.Conectado.Id);

                        //        Console.WriteLine($"Dispositivo {dispositivo}, Valor {valor}");

                        //    };
                        //}

                        //        /// Telemetria das Portas
                        //        ///
                        //        if (buffer_coils_registers != null)
                        //        {
                        //            foreach (var porta_entrada in modulo.Conexoes.Entradas)
                        //            {
                        //                // Leitura Coils
                        //                if (
                        //                    porta_entrada.Endereco != null
                        //                    && porta_entrada.Sinal == "Digital"
                        //                )
                        //                {
                        //                    var offset =
                        //                        int.Parse(porta_entrada.Endereco)
                        //                        - map_configuracoes_leituras[
                        //                            TipoLeitura.Coils
                        //                        ].StartAddress;

                        //                    bool valor = buffer_coils_registers[offset];

                        //                    var dispositivo = controlador.Dispositivos.FirstOrDefault(
                        //                        d => d.Id == porta_entrada.Conectado!.Id
                        //                    );

                        //                    Console.WriteLine(
                        //                        $"Dispositivo {dispositivo}, Valor {valor}"
                        //                    );
                        //                }
                        //                ;
                        //                // Leitura Holldings
                        //                if (
                        //                    porta_entrada.Endereco != null
                        //                    && porta_entrada.Sinal == "Analogica"
                        //                )
                        //                {
                        //                    var offset =
                        //                        int.Parse(porta_entrada.Endereco)
                        //                        - map_configuracoes_leituras[
                        //                            TipoLeitura.HoldingRegister
                        //                        ].StartAddress;

                        //                    var valor = Conversor.ToFloat(
                        //                        buffer_holding_registers[offset + 1],
                        //                        buffer_holding_registers[offset]
                        //                    );

                        //                    var dispositivo = controlador.Dispositivos.FirstOrDefault(
                        //                        d => d.Id == porta_entrada.Conectado!.Id
                        //                    );

                        //                    Console.WriteLine(
                        //                        $"Dispositivo {dispositivo.Descricao}, Valor {valor}, {porta_entrada.Faixa}"
                        //                    );
                        //                }
                        //            }
                        //            ;
                        //            foreach (var porta_saida in modulo.Conexoes.Saidas)
                        //            {
                        //                // Leitura Coils
                        //                if (
                        //                    porta_saida.Endereco != null
                        //                    && porta_saida.Sinal == "Digital"
                        //                )
                        //                {
                        //                    var offset =
                        //                        int.Parse(porta_saida.Endereco)
                        //                        - map_configuracoes_leituras[
                        //                            TipoLeitura.Coils
                        //                        ].StartAddress;

                        //                    bool valor = buffer_coils_registers[offset];

                        //                    var dispositivo = controlador.Dispositivos.FirstOrDefault(
                        //                        d => d.Id == porta_saida.Conectado!.Id
                        //                    );

                        //                    Console.WriteLine(
                        //                        $"Dispositivo {dispositivo}, Estado {valor}"
                        //                    );
                        //                }
                        //                ;

                        //                //if (porta_saida.Endereco != null && porta_saida.Sinal == "Analogico")
                        //                //{
                        //                //    var offset = int.Parse(porta_saida.Endereco) - configuracaoLeitura.StartAddress;

                        //                //    var valor = buffer_holding_registers[offset + 1];

                        //                //    var dispositivo = controlador.Dispositivos.FirstOrDefault(d => d.Id == porta_saida.Conectado!.Id);

                        //                //    Console.WriteLine($"Dispositivo {dispositivo}, Valor {valor}");
                        //                //}
                        //            }
                        //        }
                        //    }
                        //}

                        // Se nao existir Index nos parametros de modulo siguinifica que eu quero os metadados dele
                        // Modulo Filho ou Wirelles
                        //if (slaveAddress > 0 && index > 0)
                        //{
                        //    /// Leitura de Valores e Configuracoens
                        //    configuracao_holding_register = GerenciadorConfiguracao.ObterConfiguracaoLeitura(
                        //        modulo.Marca,
                        //        modulo.Modelo,
                        //        index
                        //    );

                        //    // Leitura de Estados das portas
                        //    configuracao_coils_registers =
                        //        GerenciadorConfiguracao.ObterConfiguracaoLeituraCoils(
                        //            modulo.Marca,
                        //            modulo.Modelo,
                        //            index
                        //        );
                        //    // Teste
                        //    await _conexao_1.WriteSingleCoilAsync(slaveAddress, 0, true);

                        //    buffer_holding_registers = await _conexao_1.ReadHoldingRegistersAsync(
                        //        slaveAddress,
                        //        configuracao_holding_register.StartAddress,
                        //        configuracao_holding_register.NumberOfRegister
                        //    );

                        //    buffer_coils_registers = await _conexao_1.ReadCoilsRegistersAsync(
                        //        slaveAddress,
                        //        configuracao_coils_registers.StartAddress,
                        //        configuracao_coils_registers.NumberOfRegister
                        //    );

                        //    if (buffer_holding_registers != null)
                        //    {
                        //        // Telemetria do Modulo
                        //        var telemetria = GerenciadorConfiguracao.CriarTelemetria(
                        //            modulo.Id,
                        //            modulo.Modelo,
                        //            buffer_holding_registers
                        //        );

                        //        Console.WriteLine(
                        //            JsonSerializer.Serialize(
                        //                telemetria,
                        //                options: new JsonSerializerOptions
                        //                {
                        //                    WriteIndented = true,
                        //                    IndentSize = 4,
                        //                    Encoder = System
                        //                        .Text
                        //                        .Encodings
                        //                        .Web
                        //                        .JavaScriptEncoder
                        //                        .UnsafeRelaxedJsonEscaping,
                        //                }
                        //            )
                        //        );

                        //        //foreach (var porta_saida in modulo.Conexoes.Saidas)
                        //        //{
                        //        //    if(porta_saida.Endereco != null && porta_saida.Conectado != null && porta_saida.Sinal == "Analogico")
                        //        //    {
                        //        //        var offset = int.Parse(porta_saida.Endereco) - configuracaoLeitura.StartAddress;

                        //        //        var valor = Conversor.ToFloat(buffer_1[offset + 1], buffer_1[offset]);

                        //        //        var dispositivo = controlador.Dispositivos.FirstOrDefault(d => d.Id == porta_saida.Conectado.Id);

                        //        //        Console.WriteLine($"Dispositivo {dispositivo}, Valor {valor}");

                        //        //    };
                        //        //}

                        //        /// Telemetria das Portas
                        //        ///
                        //        if (buffer_coils_registers != null)
                        //        {
                        //            foreach (var porta_entrada in modulo.Conexoes.Entradas)
                        //            {
                        //                // Leitura Coils
                        //                if (porta_entrada.Endereco != null && porta_entrada.Sinal == "Digital")
                        //                {
                        //                    var offset = int.Parse(porta_entrada.Endereco) - configuracao_coils_registers.StartAddress;

                        //                    bool valor = buffer_coils_registers[offset];

                        //                    var dispositivo = controlador.Dispositivos.FirstOrDefault(d => d.Id == porta_entrada.Conectado!.Id);

                        //                    Console.WriteLine($"Dispositivo {dispositivo}, Valor {valor}");
                        //                }
                        //                ;
                        //                // Leitura Holldings
                        //                if (porta_entrada.Endereco != null && porta_entrada.Sinal == "Analogico")
                        //                {
                        //                    var offset = int.Parse(porta_entrada.Endereco) - configuracao_holding_register.StartAddress;

                        //                    var valor = Conversor.ToFloat(buffer_holding_registers[offset + 1], buffer_holding_registers[offset + 1]);

                        //                    var dispositivo = controlador.Dispositivos.FirstOrDefault(d => d.Id == porta_entrada.Conectado!.Id);

                        //                    Console.WriteLine($"Dispositivo {dispositivo}, Valor {valor}, {porta_entrada.Faixa}");
                        //                }

                        //            };
                        //            foreach (var porta_saida in modulo.Conexoes.Saidas)
                        //            {
                        //                // Leitura Coils
                        //                if (porta_saida.Endereco != null && porta_saida.Sinal == "Digital")
                        //                {
                        //                    var offset = int.Parse(porta_saida.Endereco) - configuracao_coils_registers.StartAddress;

                        //                    bool valor = buffer_coils_registers[offset];

                        //                    var dispositivo = controlador.Dispositivos.FirstOrDefault(d => d.Id == porta_saida.Conectado!.Id);

                        //                    Console.WriteLine($"Dispositivo {dispositivo}, Estado {valor}");
                        //                };

                        //                //if (porta_saida.Endereco != null && porta_saida.Sinal == "Analogico")
                        //                //{
                        //                //    var offset = int.Parse(porta_saida.Endereco) - configuracaoLeitura.StartAddress;

                        //                //    var valor = buffer_holding_registers[offset + 1];

                        //                //    var dispositivo = controlador.Dispositivos.FirstOrDefault(d => d.Id == porta_saida.Conectado!.Id);

                        //                //    Console.WriteLine($"Dispositivo {dispositivo}, Valor {valor}");
                        //                //}

                        //            }
                        //        }
                        //    }
                        //}

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
