using System.Text.Encodings.Web;
using System.Text.Json;
using IrrigacaoInteligente.Features.Sincronizacao.Interfaces;
using IrrigacaoInteligente.Infrastructure.Data;
using IrrigacaoInteligente.State;
using Microsoft.EntityFrameworkCore;
using Toolbox.Automacao.Irrigacao.Comandos.Sincronizacao;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Extensions;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

public class SincronizarControladorHandler
    : CommandHandler,
        ICommandHandler<SincronizarControladores>
{
    private readonly IAutomacaoApi _automacaoApi;
    private readonly IrrigacaoInteligenteContext _context;
    private readonly ILogger<SincronizarControladorHandler> _logger;
    private readonly CredenciaisAplicacao _credenciaisAplicacao;
    private readonly ArmazenamentoAutomacao _armazenamento;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public SincronizarControladorHandler(
        IAutomacaoApi automacaoApi,
        IUnitOfWork<IrrigacaoInteligenteContext> uow,
        ILogger<SincronizarControladorHandler> logger,
        CredenciaisAplicacao credenciaisAplicacao,
        ArmazenamentoAutomacao armazenamento
    )
        : base(uow)
    {
        _automacaoApi = automacaoApi;
        _context = uow.Context;
        _logger = logger;
        _credenciaisAplicacao = credenciaisAplicacao;
        _armazenamento = armazenamento;
    }

    public async Task<ResponseResult> Handle(
        SincronizarControladores request,
        CancellationToken cancellationToken
    )
    {
        var controladores = await _automacaoApi.ObterControladoresPorPainelAsync(
            _credenciaisAplicacao.PainelId,
            cancellationToken
        );

        if (controladores != null && !controladores.IsEmpty())
        {
            var controladoresDeserializados = JsonSerializer.Deserialize<List<Controlador>>(
                JsonSerializer.Serialize(controladores),
                _jsonSerializerOptions
            );

            _armazenamento.Controladores.AddRange(controladoresDeserializados!);

            await _context.Controladores.ExecuteDeleteAsync(cancellationToken);

            foreach (var controlador in controladoresDeserializados!)
            {
                _context.Controladores.Add(
                    new IrrigacaoInteligente.Domain.Entities.Controlador(
                        Guid.Parse(controlador.Id.ToString()),
                        JsonSerializer.Serialize(controlador, _jsonSerializerOptions)
                    )
                );
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Controladores sincronizados com sucesso !!!");
        }

        return Ok<ResponseResult>();
    }
}
