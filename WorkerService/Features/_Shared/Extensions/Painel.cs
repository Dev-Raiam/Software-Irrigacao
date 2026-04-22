namespace WorkerService.Features.Shared.Extensions;

public static class Painel
{
    public static Domain.Entities.Painel ToEntity(this Features.Shared.Response.Painel response)
    {
        if (response is null)
            ArgumentNullException.ThrowIfNull(response);

        var modulos = response
            .Modulos.Select(m => m.ToEntity(response.Id, controlador: false))
            .ToList();
        var controladores = response
            .Controladores.Select(m => m.ToEntity(response.Id, controlador: true))
            .ToList();

        modulos.AddRange(controladores);

        return new Domain.Entities.Painel(
            response.Id,
            response.Arquivado,
            response.Primario,
            response.Descricao,
            response.Referencia,
            modulos
        );
    }
}
