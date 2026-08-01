using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using Toolbox.Industrial.Core.Data;
using Grupo = Toolbox.Industrial.Core.Data.Configuracao.grupo;
using Tipo = Toolbox.Industrial.Core.Data.Configuracao.tipo;

namespace Toolbox.Industrial.Core.Setup;

internal static class JwtConfig
{
    public static IServiceCollection AddJwtConfiguration(this IServiceCollection services)
    {
        services.AddHttpClient<JwtService>();
        services
            .AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = false;
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = true;
                options.UseSecurityTokenValidators = true;
                options.TokenValidationParameters.ClockSkew = TimeSpan.Zero;
                options.TokenValidationParameters.ValidTypes = ["JWT"];
                options.TokenValidationParameters.ValidIssuer = null;
                options.TokenValidationParameters.ValidIssuers = null;
                options.TokenValidationParameters.ValidateIssuer = true;
                options.TokenValidationParameters.ValidateAudience = false;
                options.TokenValidationParameters.ValidateLifetime = true;
                options.TokenValidationParameters.RequireSignedTokens = true;
                options.TokenValidationParameters.RequireExpirationTime = true;
                options.TokenValidationParameters.ValidateIssuerSigningKey = true;
                options.TokenValidationParameters.IssuerValidator = (
                    issuer,
                    securityToken,
                    validationParameters
                ) =>
                {
                    if (
                        JwtService.Config.ValidIssuers.Contains(
                            issuer,
                            StringComparer.OrdinalIgnoreCase
                        )
                    )
                    {
                        return issuer;
                    }

                    throw new SecurityTokenInvalidIssuerException(
                        $"Issuer '{issuer}' não permitido."
                    );
                };

                options.TokenValidationParameters.IssuerSigningKeyResolver = (
                    token,
                    securityToken,
                    kid,
                    validationParameters
                ) => JwtService.Config.KeyStore.SigningKeys;

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var jwt = (JwtSecurityToken)context.SecurityToken;
                        if (
                            !JwtService.Config.KeyStore.JsonWebKeys.TryGetValue(
                                jwt.Header.Kid,
                                out var key
                            )
                        )
                        {
                            context.Fail("Token inválido.");
                            return Task.CompletedTask;
                        }

                        if (!string.Equals(jwt.Header.Alg, key!.Alg, StringComparison.Ordinal))
                        {
                            context.Fail("Algoritmo inválido.");
                            return Task.CompletedTask;
                        }

                        if (
                            !string.Equals(
                                key.Use,
                                JsonWebKeyUseNames.Sig,
                                StringComparison.Ordinal
                            )
                        )
                        {
                            context.Fail("Uso da chave inválido.");
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<
                            ILogger<JwtBearerEvents>
                        >();
                        logger.LogError(context.Exception, "Authentication failed");
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Append("token-expired", "true");
                        }
                        return Task.CompletedTask;
                    },
                };
            });

        services.AddAuthorization();

        return services;
    }
}

internal sealed class JwtInMemoryConfig
{
    public string JwksUrl { get; set; } = null!;
    public string[] ValidIssuers { get; set; } = [];
    public JwtKeyStore KeyStore { get; set; } =
        new()
        {
            KeySet = new(),
            SigningKeys = [],
            JsonWebKeys = new Dictionary<string, JsonWebKey>(),
        };
}

internal sealed class JwtKeyStore
{
    public required JsonWebKeySet KeySet { get; init; }
    public required IReadOnlyDictionary<string, JsonWebKey> JsonWebKeys { get; init; }

    public required IReadOnlyCollection<SecurityKey> SigningKeys { get; init; }
}

internal class JwtService
{
    internal static JwtInMemoryConfig Config = new();

    private readonly IEntityStore _store;
    private readonly HttpClient _httpClient;

    //private readonly ILogger<JwtService> _logger;

    public JwtService(
        IEntityStore store /*, ILogger<JwtService> logger*/
        ,
        HttpClient httpClient
    )
    {
        _store = store;
        //_logger = logger;
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
        _httpClient.MaxResponseContentBufferSize = 10485760L;
    }

    public Task LoadJwksAsync(string json)
    {
        var keySet = new JsonWebKeySet(json);
        Config.KeyStore = new JwtKeyStore
        {
            KeySet = keySet,
            SigningKeys = keySet.GetSigningKeys().ToImmutableList(),
            JsonWebKeys = keySet
                .Keys.Where(k => !string.IsNullOrWhiteSpace(k.KeyId))
                .ToImmutableDictionary(k => k.KeyId, k => k, StringComparer.Ordinal),
        };
        return Task.CompletedTask;
    }

    public async Task LoadJwksAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IDocumentRetriever retriever = new HttpDocumentRetriever(_httpClient)
            {
                RequireHttps = true,
            };
            string json = await retriever.GetDocumentAsync(Config.JwksUrl, cancellationToken);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var config = new Configuracao(
                    id: Entity.Keys.Api.Jwt.SecKeys,
                    configuracao: json,
                    grupo: Grupo.Auth,
                    tipo: Tipo.Seguranca
                );
                await _store.UpsertAsync(config);
            }
            await LoadJwksAsync(json);
        }
        catch (Exception)
        {
            //_logger.LogError(ex, "Falha ao carregar chaves");
        }
    }
}
