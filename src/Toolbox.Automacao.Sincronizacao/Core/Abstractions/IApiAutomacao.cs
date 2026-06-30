using Toolbox.Automacao.Core.Api;
using Toolbox.Automacao.Sincronizacao.Core.Entities;

namespace Toolbox.Automacao.Sincronizacao.Core.Abstractions;

internal interface IApiAutomacao
{
    Task<Result<List<Controlador>>> ObterControladoresPorPainelAsync(
        Guid painelId,
        CancellationToken cancellationToken
    );
}
