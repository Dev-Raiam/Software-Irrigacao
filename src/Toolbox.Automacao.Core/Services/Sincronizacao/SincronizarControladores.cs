using LiteDB;
using Microsoft.Extensions.Logging;
using Toolbox.Automacao.Core.Data;
using Toolbox.Automacao.Core.Models;

namespace Toolbox.Automacao.Core.Services;

public interface ISincronizarControladores
{
    Task ExecutarAsync(Guid PainelId, CancellationToken cancellationToken);
}
internal sealed class SincronizarControladores : ISincronizarControladores
{
    private readonly IServicoAutomacao _servicoAutomacao;
    private readonly ILiteDatabase _database;
    private readonly ILogger<SincronizarControladores> _logger;

    public SincronizarControladores(
        IServicoAutomacao servicoAutomacao,
        ILiteDatabase database,
        ILogger<SincronizarControladores> logger
    )
    {
        _servicoAutomacao = servicoAutomacao;
        _database = database;
        _logger = logger;
    }

    public async Task ExecutarAsync(Guid painelId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sincronizando dados");

        var result = await _servicoAutomacao.ObterControladoresPorPainelAsync(
            painelId,
            cancellationToken
        );

        if (result.Sucesso && result.Dado != null)
        {
            foreach (var controlador in result.Dado)
            {
                _database
                    .GetCollection<ControladorConfiguracao>(Tabela.Controladores)
                    .Upsert(new ControladorConfiguracao(controlador));
            }

            _logger.LogInformation("Dados Sincronizados com sucesso");
        }
        else
        {
            _logger.LogError("Falha ao obter controladores: {Error} ", result.Error);
        }
    }
}
