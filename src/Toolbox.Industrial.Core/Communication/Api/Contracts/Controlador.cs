using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ConexaoControlador = Toolbox.Industrial.Core.Communication.Api.Contracts.Controlador.Conexao;
using ConexaoDispositivo = Toolbox.Industrial.Core.Communication.Api.Contracts.Dispositivo.Conexao;
using ConexaoInterface = Toolbox.Industrial.Core.Communication.Api.Contracts.Interface.Conexao;
using ConexaoModulo = Toolbox.Industrial.Core.Communication.Api.Contracts.Modulo.Conexao;
using ConexaoPorta = Toolbox.Industrial.Core.Communication.Api.Contracts.Porta.Conexao;

namespace Toolbox.Industrial.Core.Communication.Api.Contracts;

public sealed record Controlador(
    Guid Id,
    bool Master,
    string Estagio,
    string Descricao,
    string Marca,
    string Modelo,
    ConexaoControlador Conexoes,
    IEnumerable<Modulo> Modulos,
    IEnumerable<Interface> Interfaces,
    IEnumerable<Dispositivo> Dispositivos
)
{
    public sealed record Conexao(
        string Host,
        IEnumerable<Porta> Saidas,
        IEnumerable<Porta> Entradas,
        IEnumerable<Interface> Interfaces
    );
}

public sealed record Modulo(
    Guid Id,
    string Descricao,
    bool Master,
    string Marca,
    string Modelo,
    string Estagio,
    string Protocolo,
    Parametros Parametros,
    ConexaoModulo Conexoes
)
{
    public sealed record Conexao(
        Dispositivo.Conexao? Conectado,
        IEnumerable<Porta> Saidas,
        IEnumerable<Porta> Entradas,
        IEnumerable<Interface> Interfaces
    );
}

public sealed record Dispositivo(
    Guid Id,
    bool Habilitado,
    string Descricao,
    string Tipo,
    string Sinal,
    string Categoria,
    Parametros? Parametros,
    ConexaoDispositivo Conectado
)
{
    public sealed record Conexao(Guid Id, string Tipo, Canal Canal);
}

public sealed record Interface(
    Guid Id,
    string Tipo,
    string Status,
    string Nome,
    string? Borne,
    string? Endereco,
    Parametros? Parametros,
    IEnumerable<ConexaoInterface> Conectados
)
{
    public sealed record Conexao(Guid Id, string Tipo);
}

public sealed record Porta(
    Guid Id,
    string Sinal,
    string[] Faixa,
    string Status,
    string? Borne,
    string? Endereco,
    Parametros? Parametros,
    ConexaoPorta? Conectado
)
{
    public sealed record Conexao(Guid Id, string Tipo);
}

public sealed record Canal(Guid Id, string Tipo);

[JsonConverter(typeof(ParametrosJsonConverter))]
public sealed record Parametros
{
    [JsonExtensionData]
    public Dictionary<string, object> Parametro { get; set; } =
        new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
}

public sealed class ParametrosJsonConverter : JsonConverter<Parametros>
{
    public override Parametros? ReadJson(
        JsonReader reader,
        Type objectType,
        Parametros? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        var parametros = existingValue ?? new Parametros();

        var obj = JObject.Load(reader);

        foreach (var property in obj.Properties())
        {
            parametros.Parametro[property.Name] = ConvertToken(property.Value)!;
        }

        return parametros;
    }

    public override void WriteJson(JsonWriter writer, Parametros? value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        if (value is not null)
        {
            foreach (var item in value.Parametro)
            {
                writer.WritePropertyName(item.Key);
                serializer.Serialize(writer, item.Value);
            }
        }

        writer.WriteEndObject();
    }

    private static object? ConvertToken(JToken token)
    {
        return token.Type switch
        {
            //JTokenType.Null => null,

            JTokenType.Integer => ConvertInteger(token),

            JTokenType.Float => token.Value<double>(),

            JTokenType.Boolean => token.Value<bool>(),

            JTokenType.String => ConvertString(token),

            JTokenType.Guid => token.Value<Guid>(),

            JTokenType.Date => token.Value<DateTime>(),

            JTokenType.Array => token.Children().Select(ConvertToken).ToList(),

            JTokenType.Object => token
                .Children<JProperty>()
                .ToDictionary(
                    p => p.Name,
                    p => ConvertToken(p.Value),
                    StringComparer.OrdinalIgnoreCase
                ),

            _ => ((JValue)token).Value,
        };
    }

    private static object? ConvertString(JToken token)
    {
        var value = token.Value<string>();
        if (Guid.TryParse(value, out var guid))
            return guid;

        if (DateTime.TryParse(value, out var dt))
            return dt;

        return value;
    }

    private static object? ConvertInteger(JToken token)
    {
        var value = token.Value<long>();
        if (value >= int.MinValue && value <= int.MaxValue)
            return Convert.ToInt32(value);

        return value;
    }
}

/*
public static class BsonJsonConverter
{
    public static BsonValue ToBsonValue(object? value)
    {
        if (value is null)
            return BsonValue.Null;

        return value switch
        {
            BsonValue bson => bson,
            JsonElement json => ToBsonValue(json),
            string s => s,
            bool b => b,
            short v => v,
            int v => v,
            long v => v,
            float v => (double)v,
            double v => v,
            decimal v => (double)v,
            Guid g => g,
            DateTime dt => dt,
            DateTimeOffset dto => dto.UtcDateTime,
            IDictionary<string, object?> dic => ToBsonDocument(dic),
            IEnumerable<object?> list => ToBsonArray(list),
            _ => BsonMapper.Global.Serialize(value)
        };
    }

    private static BsonValue ToBsonValue(JsonElement json)
    {
        return json.ValueKind switch
        {
            JsonValueKind.Null => BsonValue.Null,

            JsonValueKind.True => true,

            JsonValueKind.False => false,

            JsonValueKind.String => ReadString(json),

            JsonValueKind.Number => ReadNumber(json),

            JsonValueKind.Object => ToBsonDocument(json),

            JsonValueKind.Array => ToBsonArray(json),

            _ => json.GetRawText()
        };
    }

    private static BsonValue ReadString(JsonElement json)
    {
        if (json.TryGetGuid(out var guid))
            return guid;

        if (json.TryGetDateTime(out var dt))
            return dt;

        return json.GetString()!;
    }

    private static BsonValue ReadNumber(JsonElement json)
    {
        if (json.TryGetInt32(out var i))
            return i;

        if (json.TryGetInt64(out var l))
            return l;

        if (json.TryGetDecimal(out var d))
            return (double)d;

        return json.GetDouble();
    }

    private static BsonDocument ToBsonDocument(JsonElement json)
    {
        var doc = new BsonDocument();
        foreach (var property in json.EnumerateObject())
        {
            doc[property.Name] = ToBsonValue(property.Value);
        }

        return doc;
    }

    private static BsonDocument ToBsonDocument(IDictionary<string, object?> dic)
    {
        var doc = new BsonDocument();

        foreach (var item in dic)
        {
            doc[item.Key] = ToBsonValue(item.Value);
        }

        return doc;
    }

    private static BsonArray ToBsonArray(JsonElement json)
    {
        var array = new BsonArray();

        foreach (var item in json.EnumerateArray())
        {
            array.Add(ToBsonValue(item));
        }

        return array;
    }

    private static BsonArray ToBsonArray(IEnumerable<object?> list)
    {
        var array = new BsonArray();

        foreach (var item in list)
        {
            array.Add(ToBsonValue(item));
        }

        return array;
    }
}
*/
