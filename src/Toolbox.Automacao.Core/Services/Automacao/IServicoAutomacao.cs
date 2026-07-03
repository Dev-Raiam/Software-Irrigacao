using Toolbox.Automacao.Core.Api;
using Toolbox.Automacao.Core.Models;

namespace Toolbox.Automacao.Core.Services;

public interface IServicoAutomacao
{
    Task<Result<List<Controlador>>> ObterControladoresPorPainelAsync(
        Guid painelId,
        CancellationToken cancellationToken
    );
}
