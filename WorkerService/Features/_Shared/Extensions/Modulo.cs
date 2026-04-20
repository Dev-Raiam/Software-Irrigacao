namespace WorkerService.Application.Extensions;

public static class Modulo
{
    public static Domain.Entities.Modulo ToEntity(
        this Features.Shared.Response.Modulo response,
        Guid painelId,
        bool controlador
    )
    {
        if (response is null)
            ArgumentNullException.ThrowIfNull(response);

        return new Domain.Entities.Modulo(
            response.Id,
            painelId,
            response.Arquivado,
            response.Master,
            controlador,
            response.Estagio,
            response.Marca,
            response.Modelo,
            response.Descricao
        );
    }
}
