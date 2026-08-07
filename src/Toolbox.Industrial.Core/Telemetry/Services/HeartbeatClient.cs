using System.Net.Http.Json;
using Newtonsoft.Json;
using Toolbox.Industrial.Core.Data;
using static Toolbox.Industrial.Core.Data.Configuracao;

namespace Toolbox.Industrial.Core.Telemetry.Services;

internal interface IHeartbeatClient
{
    ValueTask<HttpResponseMessage> SendAsync(CancellationToken cancellationToken);
    ValueTask<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    );
}

internal sealed class HeartbeatClient : IHeartbeatClient
{
    private readonly HttpClient _http;
    private readonly IEntityStore _store;
    private readonly ISystemMetricsCollector _collector;

    public HeartbeatClient(HttpClient http, IEntityStore store, ISystemMetricsCollector collector)
    {
        _http = http;
        _store = store;
        _collector = collector;
    }

    public async ValueTask<HttpResponseMessage> SendAsync(CancellationToken cancellationToken)
    {
        var delta = _collector.Current.Take();
        var requestUri =
            $"automacao/v1/paineis/{Controlador.PainelId}/controladores/{Controlador.ControladorId}/telemetria";
        var teste = JsonConvert.SerializeObject(delta, Formatting.Indented);
        var response = await _http.PostAsJsonAsync(requestUri, delta, cancellationToken);

        if (response?.IsSuccessStatusCode == true)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var json = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var data = JsonConvert.DeserializeObject<HeartbeatResponse>(json);
                    if (data?.HeartbeatOptions != null)
                    {
                        Heartbeat.Options = data.HeartbeatOptions;
                    }
                }
            }
        }
        else
        {
            await _store.InsertAsync(
                new Telemetria(
                    id: SequentialGuid.NewGuid(),
                    telemetria: delta,
                    tipo: Telemetria.tipo.Controlador
                )
            );
        }
        return response!;
    }

    public async ValueTask<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        return await _http.SendAsync(request, cancellationToken);
    }
}
