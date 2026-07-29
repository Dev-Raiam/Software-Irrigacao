using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.Jwt.Core;
using NetDevPack.Security.JwtExtensions;
using Toolbox.Industrial.Core.Communication.Api;

namespace Toolbox.Industrial.Core.Setup;

internal static class JwtConfig
{
    private static TokenValidationParameters _validationParameters = null!;

    public static IServiceCollection AddJwtConfiguration(
        this IServiceCollection services
    )
    {
        services
            .AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                _validationParameters = options.TokenValidationParameters;
                options.SaveToken = true; //cache
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = true;
                options.SetJwksOptions(new JwkOptions(ApiClient.JwtJwksUrl!));
                options.UseSecurityTokenValidators = true;
                options.TokenValidationParameters.ClockSkew = TimeSpan.Zero;
                options.TokenValidationParameters.ValidateIssuer = true;
                options.TokenValidationParameters.ValidateAudience = false;
                options.TokenValidationParameters.ValidateLifetime = true;
                options.TokenValidationParameters.RequireSignedTokens = true;
                options.TokenValidationParameters.RequireExpirationTime = true;
                options.TokenValidationParameters.ValidateIssuerSigningKey = false; //tem que pegar a chave de segurança.
                options.TokenValidationParameters.ValidTypes = ["JWT"];
                options.TokenValidationParameters.ValidIssuer = null;
                options.TokenValidationParameters.ValidIssuers = ApiClient.JwtIssuers?.Split(
                    ';',
                    StringSplitOptions.RemoveEmptyEntries
                );
                //Remover isso para deixar a validação padrao.
                options.TokenValidationParameters.SignatureValidator = delegate(
                    string token,
                    TokenValidationParameters parameters
                )
                {
                    return new JwtSecurityToken(token);
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<
                            ILogger<JwtBearerEvents>
                        >();
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
                        return Task.CompletedTask;
                        //var jwksOptions = context
                        //    .HttpContext.RequestServices.GetRequiredService<IOptions<JwtOptions>>()
                        //    .Value;
                        //var token = ((JwtSecurityToken)context.SecurityToken).RawData;
                        //var tokenHandler = new JwtSecurityTokenHandler();
                        //tokenHandler.ValidateToken(
                        //    token,
                        //    _validationParameters,
                        //    out SecurityToken securityToken
                        //);
                        //if (
                        //    securityToken is not JwtSecurityToken jwtSecurityToken
                        //    || !jwtSecurityToken.Header.Alg.Equals(
                        //        jwksOptions?.Jws.Alg,
                        //        StringComparison.InvariantCultureIgnoreCase
                        //    )
                        //)
                        //{
                        //    throw new SecurityTokenInvalidSignatureException(
                        //        "Assinatura do token inválida"
                        //    );
                        //}
                        //return Task.CompletedTask;
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
