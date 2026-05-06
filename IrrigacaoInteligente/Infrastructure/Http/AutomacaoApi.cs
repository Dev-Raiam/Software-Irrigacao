using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using IrrigacaoInteligente.Features.Automacao.Interfaces;
using IrrigacaoInteligente.Features.Shared.Extensions;
using IrrigacaoInteligente.State;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace IrrigacaoInteligente.Infrastructure.Http;

public sealed class AutomacaoApi : IAutomacaoApi
{
    private readonly HttpClient _httpClient;
    private readonly ApiOptions _apiOptions;
    private readonly ArmazenamentoAutomacao _armazenamentoAutomacao;
    private readonly JsonSerializerSettings _jsonSettings;
    private readonly ILogger<AutomacaoApi> _logger;

    public AutomacaoApi(
        HttpClient httpClient,
        IOptions<ApiOptions> apiOptions,
        ArmazenamentoAutomacao armazenamentoAutomacao,
        ILogger<AutomacaoApi> logger
    )
    {
        _httpClient = httpClient;
        _apiOptions = apiOptions.Value;
        _armazenamentoAutomacao = armazenamentoAutomacao;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_apiOptions.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_apiOptions.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue(_apiOptions.MediaType)
        );

        _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = { new StringEnumConverter() },
        };
    }

    public async Task<List<Features.Shared.Response.Painel>?> ObterPaineisAsync(
        Guid contaId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/automacao/v1/paineis?contaId={contaId}&categoria=todos&status=todos",
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Erro ao obter painéis: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var paineis = JsonConvert.DeserializeObject<List<Features.Shared.Response.Painel>>(
                json,
                _jsonSettings
            );

            if (paineis is not null)
                _armazenamentoAutomacao.Paineis.AddRange(paineis.Select(p => p.ToEntity()));

            return paineis;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Tempo esgotado ao obter painéis");
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de rede ao obter painéis");
            return null;
        }
    }

    public async Task<List<Features.Shared.Response.Dispositivo>?> ObterDispositivosPorPainelAsync(
        Guid painelId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/automacao/v1/paineis/{painelId}/dispositivos",
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Erro ao obter dispositivos: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var dispositivos = JsonConvert.DeserializeObject<
                List<Features.Shared.Response.Dispositivo>
            >(json, _jsonSettings);

            if (dispositivos is not null)
                _armazenamentoAutomacao.Dispositivos.AddRange(
                    dispositivos.Select(d => d.ToEntity(painelId))
                );

            return dispositivos;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Tempo esgotado ao obter Dispositivos");
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de rede ao obter Dispositivos");
            return null;
        }
    }

    public async Task<List<Features.Shared.Response.Porta>?> ObterPortasPorControladorAsync(
        Guid painelId,
        Guid controladorId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/automacao/v1/paineis/{painelId}/controladores/{controladorId}/portas?status=todas",
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Erro ao obter portas por Controlador: {StatusCode}",
                    response.StatusCode
                );
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            var portas = JsonConvert.DeserializeObject<List<Features.Shared.Response.Porta>>(
                json,
                _jsonSettings
            );

            if (portas is not null)
                _armazenamentoAutomacao.Portas.AddRange(
                    portas.Select(p => p.ToEntity(controladorId))
                );

            return portas;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Tempo esgotado ao obter Portas por Controlador");
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de rede ao obter Portas por Controlador");
            return null;
        }
    }

    public async Task<List<Features.Shared.Response.Porta>?> ObterPortasPorModuloAsync(
        Guid painelId,
        Guid moduloId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/automacao/v1/paineis/{painelId}/modulos/{moduloId}/portas?status=todas",
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Erro ao obter portas por Modulo: {StatusCode}",
                    response.StatusCode
                );
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var portas = JsonConvert.DeserializeObject<List<Features.Shared.Response.Porta>>(
                json,
                _jsonSettings
            );

            if (portas is not null)
                _armazenamentoAutomacao.Portas.AddRange(portas.Select(p => p.ToEntity(moduloId)));

            return portas;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Tempo esgotado ao obter Portas por Modulo");
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de rede ao obter Portas por Modulo");
            return null;
        }
    }

    public async Task<List<Features.Shared.Response.Interface>?> ObterInterfacesPorControladorAsync(
        Guid painelId,
        Guid controladorId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/automacao/v1/paineis/{painelId}/controladores/{controladorId}/interfaces?categoria=todas&status=todas",
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Erro ao obter Interfaces por Controlador: {StatusCode}",
                    response.StatusCode
                );
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var interfaces = JsonConvert.DeserializeObject<
                List<Features.Shared.Response.Interface>
            >(json, _jsonSettings);

            if (interfaces is not null)
                _armazenamentoAutomacao.Interfaces.AddRange(
                    interfaces.Select(i => i.ToEntity(controladorId))
                );

            return interfaces;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Tempo esgotado ao obter Interfaces por Controlador");
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de rede ao obter Interfaces por Controlador");
            return null;
        }
    }

    public async Task<List<Features.Shared.Response.Interface>?> ObterInterfacesPorModuloAsync(
        Guid painelId,
        Guid moduloId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/automacao/v1/paineis/{painelId}/modulos/{moduloId}/interfaces?categoria=todas&status=todas",
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Erro ao obter Interfaces por Modulos: {StatusCode}",
                    response.StatusCode
                );
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var interfaces = JsonConvert.DeserializeObject<
                List<Features.Shared.Response.Interface>
            >(json, _jsonSettings);

            if (interfaces is not null)
                _armazenamentoAutomacao.Interfaces.AddRange(
                    interfaces.Select(i => i.ToEntity(moduloId))
                );

            return interfaces;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Tempo esgotado ao obter Interfaces por Modulo");
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de rede ao obter Interfaces por Modulo");
            return null;
        }
    }

    public async Task<string?> ObterControladoresPorPainelAsync(
        Guid painelId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/automacao/v1/paineis/{painelId}/controladores?status=todos",
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Erro ao obter Controladores por Painel: {StatusCode}",
                    response.StatusCode
                );
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            _armazenamentoAutomacao.Dados = json;

            return json;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Tempo esgotado ao obter Controladores por Painel");
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de rede ao obter Controladores por Painel");
            return null;
        }
    }
}
