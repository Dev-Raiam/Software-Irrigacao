using System.Text.Json;
using System.Text.Json.Serialization;

namespace Toolbox.Automacao.Core.Messages
{
    public class CommandEnvelope
    {
        [JsonPropertyName("commandType")]
        public string CommandType { get; set; } = string.Empty;

        [JsonPropertyName("payload")]
        public JsonElement Payload { get; set; }
    }
}
