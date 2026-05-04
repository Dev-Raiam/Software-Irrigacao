namespace IrrigacaoInteligente.Features.Shared.Extensions;

public static class Interface
{
    public static Domain.Entities.Interface ToEntity(
        this Response.Interface response,
        Guid moduloId
    )
    {
        if (response is null)
            ArgumentNullException.ThrowIfNull(response);

        return new Domain.Entities.Interface(
            response.Id,
            moduloId,
            response.Conectado?.Id,
            response.Tipo,
            response.Status,
            response.Marca,
            response.Modelo,
            response.Categoria,
            response.Modbus is null ? null : response.Modbus.Indice,
            response.Modbus is null ? null : response.Modbus.Endereco,
            response.Porta,
            response.EnderecoBorne,
            response.EnderecoLogico
        );
    }
}
