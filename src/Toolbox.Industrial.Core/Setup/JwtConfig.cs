using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.Jwt.Core;
using NetDevPack.Security.JwtExtensions;
using System.IdentityModel.Tokens.Jwt;
using Toolbox.Industrial.Core.Communication.Api;

namespace Toolbox.Industrial.Core.Setup;

internal static class JwtConfig
{
    private static TokenValidationParameters _validationParameters = null!;

    public static void SetJwksOptions(JwtBearerOptions options, JwkOptions jwkOptions)
    {
        HttpClient httpClient = new HttpClient(options.BackchannelHttpHandler ?? new HttpClientHandler())
        {
            Timeout = options.BackchannelTimeout,
            MaxResponseContentBufferSize = 10485760L
        };
        options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(jwkOptions.JwksUri, new JwksRetriever(), new HttpDocumentRetriever(httpClient)
        {
            RequireHttps = options.RequireHttpsMetadata
        });
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.ValidIssuer = jwkOptions.Issuer;
        if (!string.IsNullOrEmpty(jwkOptions.Audience))
        {
            options.TokenValidationParameters.ValidateAudience = true;
            options.TokenValidationParameters.ValidAudience = jwkOptions.Audience;
        }
    }

    public static IServiceCollection AddJwtConfiguration(this IServiceCollection services)
    {
        services
            .AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                //_validationParameters = options.TokenValidationParameters;
                options.SaveToken = true; //cache
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = true;
                SetJwksOptions(options, new JwkOptions(ApiClient.JwtJwksUrl!));
                options.UseSecurityTokenValidators = true;
                options.TokenValidationParameters.ClockSkew = TimeSpan.Zero;
                options.TokenValidationParameters.ValidTypes = ["JWT"];
                options.TokenValidationParameters.ValidIssuer = null;
                options.TokenValidationParameters.ValidateIssuer = true;
                options.TokenValidationParameters.ValidateAudience = false;
                options.TokenValidationParameters.ValidateLifetime = true;
                options.TokenValidationParameters.RequireSignedTokens = true;
                options.TokenValidationParameters.RequireExpirationTime = true;
                options.TokenValidationParameters.IssuerSigningKey = ApiClient.Credentials?.Key;
                options.TokenValidationParameters.ValidateIssuerSigningKey =
                    ApiClient.Credentials?.Key != null;
                options.TokenValidationParameters.ValidIssuers = ApiClient.JwtIssuers?.Split(
                    ';',
                    StringSplitOptions.RemoveEmptyEntries
                );
                if (ApiClient.Credentials?.Key == null)
                {
                    options.TokenValidationParameters.SignatureValidator = delegate(
                        string token,
                        TokenValidationParameters parameters
                    )
                    {
                        return new JwtSecurityToken(token);
                    };
                }
                options.TokenValidationParameters.IssuerSigningKeyResolver =
                    (token, securityToken, kid, parameters) =>
                    {
                        var key = ApiClient.Credentials?.Key;

                        if (key == null)
                            return Enumerable.Empty<SecurityKey>();

                        return new[] { key };
                    };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var options = context
                            .HttpContext.RequestServices.GetRequiredService<IOptions<JwtOptions>>()
                            .Value;

                        var jwt = (JwtSecurityToken)context.SecurityToken;
                        if (
                            !string.Equals(
                                jwt.Header.Alg,
                                options.Jws.Alg,
                                StringComparison.Ordinal
                            )
                        )
                        {
                            context.Fail("Algoritmo inválido.");
                        }

                        if (jwt.Header.Typ != "JWT")
                        {
                            context.Fail("Tipo inválido.");
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
