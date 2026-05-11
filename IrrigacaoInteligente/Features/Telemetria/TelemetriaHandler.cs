using System.Text.Encodings.Web;
using System.Text.Json;
using IrrigacaoInteligente.Domain.Entities;
using IrrigacaoInteligente.Infrastructure.Data;
using IrrigacaoInteligente.Infrastructure.Mqtt;
using IrrigacaoInteligente.State;
using Microsoft.AspNetCore.WebUtilities;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace IrrigacaoInteligente.Features.Telemetria
{
    public class TelemetriaHandler
        : CommandHandler,
            ICommandHandler<AdicionarTelemetria>,
            ICommandHandler<PublicarTelemetria>
    {
        private readonly MqttClienteLocal _mqttClienteLocal;
        private readonly ArmazenamentoAutomacao _armazenamentoAutomacao;
        private readonly IrrigacaoInteligenteContext _context;

        public TelemetriaHandler(
            IUnitOfWork<IrrigacaoInteligenteContext> uow,
            MqttClienteLocal mqttClienteLocal,
            ArmazenamentoAutomacao armazenamentoAutomacao,
            IrrigacaoInteligenteContext context
        )
            : base(uow)
        {
            _mqttClienteLocal = mqttClienteLocal;
            _armazenamentoAutomacao = armazenamentoAutomacao;
            _context = context;
        }

        public async Task<ResponseResult> Handle(
            AdicionarTelemetria request,
            CancellationToken cancellationToken = default
        )
        {
            var leituras = JsonSerializer.Deserialize<List<TelemetriaResponse>>(request.Dados);

            if (leituras is not null)
            {
                var telemetrias = leituras
                    .Select(b => new Domain.Entities.Telemetria(
                        b.ControladorId,
                        b.DispositivoId,
                        b.Descricao,
                        b.Dados
                    ))
                    .ToList();

                await _context.Telemetrias.AddRangeAsync(telemetrias, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);

            await Task.Delay(1000);

            return Ok<ResponseResult>();
        }

        public async Task<ResponseResult> Handle(
            PublicarTelemetria request,
            CancellationToken cancellationToken = default
        )
        {
            var blocosLeituras = new List<BlocoTelemetria>();

            foreach (var controlador in _armazenamentoAutomacao.Controladores)
            {
                var blocoEntrada = new BlocoTelemetria()
                {
                    ControladorId = controlador.Id,
                    IntervaloLeitura = 2,
                };

                controlador
                    .Conexoes?.Entradas?.Where(p => p.Status == "Habilitada" && p.Conectado != null)
                    .ToList()
                    .ForEach(p =>
                    {
                        var leituras = new BlocoTelemetria.Leitura
                        {
                            DispositivoId = p.Conectado!.Id,
                            Endereco = p.Endereco!,
                            Descricao = p.Conectado!.Descricao!,
                            Sinal = p.Sinal,
                        };

                        blocoEntrada.Leituras.Add(leituras);
                    });

                blocosLeituras.Add(blocoEntrada);

                // controlador
                //     .Conexoes?.Interfaces?.Where(i =>
                //         i.Status == "Habilitada" && i.Conectado != null
                //     )
                //     .ToList()
                //     .ForEach(i =>
                //     {
                //         var leituras = new BlocoTelemetria.Leitura
                //         {
                //             DispositivoId = i.Conectado!.Modulos.ForEach(modulo =>
                //             {
                //                 DispositivoId = modulo.Id;
                //             }),
                //             Endereco = i.Endereco!,
                //             Descricao = p.Conectado!.Descricao!,
                //             Sinal = p.Sinal,
                //         };

                //         blocoEntrada.Leituras.Add(leituras);
                //     });

                // blocosLeituras.Add(blocoEntrada);
            }

            await _mqttClienteLocal.PublicarAsync(
                "telemetria/bloco",
                JsonSerializer.Serialize(
                    blocosLeituras,
                    options: new JsonSerializerOptions
                    {
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        WriteIndented = true,
                        IndentSize = 4,
                    }
                ),
                cancellationToken
            );

            return Ok<ResponseResult>();
        }
    }
}
