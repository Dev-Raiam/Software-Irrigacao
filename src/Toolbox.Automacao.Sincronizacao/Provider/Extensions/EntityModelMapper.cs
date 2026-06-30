namespace Toolbox.Automacao.Sincronizacao.Provider.Extensions;

internal static class EntityModelMapper
{
    public static Controlador Mapper(this Core.Entities.Controlador entity) =>
        new()
        {
            Id = entity.Id,
            Master = entity.Master,
            Estagio = entity.Estagio,
            Descricao = entity.Descricao,
            Marca = entity.Marca,
            Modelo = entity.Modelo,
            Conexoes = entity.Conexoes.Mapper(),
            Modulos = entity.Modulos.Select(m => m.Mapper()).ToList(),
            Interfaces = entity.Interfaces.Select(i => i.Mapper()).ToList(),
            Dispositivos = entity.Dispositivos.Select(d => d.Mapper()).ToList(),
        };

    private static Controlador.Conexao Mapper(this Core.Entities.Controlador.Conexao entity) =>
        new()
        {
            Host = entity.Host,
            Saidas = entity.Saidas.Select(p => p.Mapper()).ToList(),
            Entradas = entity.Entradas.Select(p => p.Mapper()).ToList(),
            Interfaces = entity.Interfaces.Select(i => i.Mapper()).ToList(),
        };

    public static Modulo Mapper(this Core.Entities.Modulo entity) =>
        new()
        {
            Id = entity.Id,
            Descricao = entity.Descricao,
            Master = entity.Master,
            Marca = entity.Marca,
            Modelo = entity.Modelo,
            Estagio = entity.Estagio,
            Protocolo = entity.Protocolo,
            Parametros = entity.Parametros.Mapper(),
            Conexoes = entity.Conexoes.Mapper(),
        };

    private static Modulo.Conexao Mapper(this Core.Entities.Modulo.Conexao entity) =>
        new()
        {
            Conectado = entity.Conectado?.Mapper(),
            Saidas = entity.Saidas.Select(p => p.Mapper()).ToList(),
            Entradas = entity.Entradas.Select(p => p.Mapper()).ToList(),
            Interfaces = entity.Interfaces.Select(i => i.Mapper()).ToList(),
        };

    public static Dispositivo Mapper(this Core.Entities.Dispositivo entity) =>
        new()
        {
            Id = entity.Id,
            Habilitado = entity.Habilitado,
            Descricao = entity.Descricao,
            Tipo = entity.Tipo,
            Sinal = entity.Sinal,
            Categoria = entity.Categoria,
            Parametros = entity.Parametros?.Mapper(),
            Conectado = entity.Conectado.Mapper(),
        };

    private static Dispositivo.Conexao Mapper(this Core.Entities.Dispositivo.Conexao entity) =>
        new()
        {
            Id = entity.Id,
            Tipo = entity.Tipo,
            Canal = entity.Canal.Mapper(),
        };

    private static Interface Mapper(this Core.Entities.Interface entity) =>
        new()
        {
            Id = entity.Id,
            Tipo = entity.Tipo,
            Status = entity.Status,
            Nome = entity.Nome,
            Borne = entity.Borne,
            Endereco = entity.Endereco,
            Parametros = entity.Parametros?.Mapper(),
            Conectados = entity.Conectados.Select(c => c.Mapper()).ToList(),
        };

    private static Interface.Conexao Mapper(this Core.Entities.Interface.Conexao entity) =>
        new() { Id = entity.Id, Tipo = entity.Tipo };

    private static Porta Mapper(this Core.Entities.Porta entity) =>
        new()
        {
            Id = entity.Id,
            Sinal = entity.Sinal,
            Faixa = entity.Faixa,
            Status = entity.Status,
            Borne = entity.Borne,
            Endereco = entity.Endereco,
            Conectado = entity.Conectado?.Mapper(),
        };

    private static Porta.Conexao Mapper(this Core.Entities.Porta.Conexao entity) =>
        new() { Id = entity.Id, Tipo = entity.Tipo };

    private static Canal Mapper(this Core.Entities.Canal entity) =>
        new() { Id = entity.Id, Tipo = entity.Tipo };

    private static Parametros Mapper(this Core.Entities.Parametros entity) =>
        new()
        {
            Parametro = new Dictionary<string, object>(
                entity.Parametro ?? new Dictionary<string, object>(),
                StringComparer.OrdinalIgnoreCase
            ),
        };
}
