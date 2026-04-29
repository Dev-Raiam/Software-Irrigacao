namespace WorkerService.Features.Shared.Extensions;

public static class Porta
{
    public static Domain.Entities.Porta ToEntity(this Response.Porta response, Guid moduloId)
    {
        if (response is null)
            ArgumentNullException.ThrowIfNull(response);
        return new Domain.Entities.Porta(
            response.Id,
            moduloId,
            response.Dispositivo.Id,
            response.Tipo,
            response.Sinal,
            response.Status,
            response.Nome,
            response.EnderecoBorne,
            response.EnderecoLogico
        );
    }
}
