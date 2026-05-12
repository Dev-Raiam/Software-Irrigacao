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
            ICommandHandler<SalvarTelemetria>,
            ICommandHandler<PublicarTelemetria>
    {
        private readonly MqttClienteLocal _mqttClienteLocal;
        private readonly ArmazenamentoAutomacao _armazenamentoAutomacao;
        private readonly IrrigacaoInteligenteContext _context;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            IndentSize = 4,
        };

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
            SalvarTelemetria request,
            CancellationToken cancellationToken
        )
        {
            var telemetriaResposta = JsonSerializer.Deserialize<TelemetriaResposta>(
                request.Dados,
                _jsonOptions
            );

            if (telemetriaResposta is not null)
            {
                var telemetrias = telemetriaResposta
                    .Leituras.Select(l => new Domain.Entities.Telemetria(
                        telemetriaResposta.ControladorId,
                        l.DispositivoId,
                        l.Descricao,
                        l.Dados
                    ))
                    .ToList();

                await _context.Telemetrias.AddRangeAsync(telemetrias, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Ok<ResponseResult>();
        }

        public async Task<ResponseResult> Handle(
            PublicarTelemetria request,
            CancellationToken cancellationToken
        )
        {
            var blocosSinais = new List<BlocoTelemetriaSinal>();
            // var blocosLeiturasInterface = new List<BlocoTelemetriaProtocolo>();

            foreach (var controlador in _armazenamentoAutomacao.Controladores)
            {
                var blocoSinal = new BlocoTelemetriaSinal()
                {
                    ControladorId = controlador.Id,
                    IntervaloLeitura = 2,
                };

                // Entradas
                controlador
                    .Conexoes?.Entradas?.Where(p => p.Status == "Habilitada" && p.Conectado != null)
                    .ToList()
                    .ForEach(p =>
                    {
                        var dispositivo = new BlocoTelemetriaSinal.Leitura
                        {
                            DispositivoId = p.Conectado!.Id,
                            Endereco = p.Endereco!,
                            Descricao = p.Conectado!.Descricao!,
                            Sinal = p.Sinal,
                        };

                        blocoSinal.Leituras.Add(dispositivo);
                    });

                blocosSinais.Add(blocoSinal);
            }

            foreach (var blocoSinal in blocosSinais)
            {
                await _mqttClienteLocal.PublicarAsync(
                    $"telemetria/{blocoSinal.ControladorId}/bloco",
                    JsonSerializer.Serialize(blocoSinal, _jsonOptions),
                    cancellationToken
                );
            }

            return Ok<ResponseResult>();
        }
    }
}
