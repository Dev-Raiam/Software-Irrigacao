using WorkerService.Domain.Entities;

namespace WorkerService.Features.Shared.Extensions;

public static class Dispositivo
{
    public static Domain.Entities.Dispositivo ToEntity(
        this Response.Dispositivo response,
        Guid painelId
    )
    {
        if (response is null)
            ArgumentNullException.ThrowIfNull(response);

        var parametros = response.Parametros is not null
            ? new Parametros
            {
                ValorMinimo = response.Parametros.ValorMinimo,
                ValorMaximo = response.Parametros.ValorMaximo,
                UnidadeMedida = response.Parametros.UnidadeMedida,
                TempoResposta = response.Parametros.TempoResposta,
                Precisao = response.Parametros.Precisao,
                TemperaturaOperacaoMin = response.Parametros.TemperaturaOperacaoMin,
                TemperaturaOperacaoMax = response.Parametros.TemperaturaOperacaoMax,
            }
            : null;

        return new Domain.Entities.Dispositivo(
            response.Id,
            painelId,
            response.Arquivado,
            response.Tipo,
            parametros,
            response.Descricao
        );
    }
}
