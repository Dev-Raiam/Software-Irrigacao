using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.Jwt.Core;
using NetDevPack.Security.JwtExtensions;

namespace SoftwareIrrigacao.Setup;

public static class JwtConfig
{
    private static TokenValidationParameters _validationParameters = null!;

    public static IServiceCollection AddJwtConfiguration(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                _validationParameters = x.TokenValidationParameters;
                x.RequireHttpsMetadata = true;
                x.SaveToken = true; //cache
                x.SetJwksOptions(new JwkOptions(configuration["Authentication:Jwt:JwksUrl"]!));
                x.TokenValidationParameters.ClockSkew = TimeSpan.Zero;
                x.TokenValidationParameters.ValidateIssuer = true;
                x.TokenValidationParameters.ValidateAudience = false;
                x.TokenValidationParameters.ValidateLifetime = true;
                x.TokenValidationParameters.ValidateIssuerSigningKey = true;
                x.UseSecurityTokenValidators = true;
                x.TokenValidationParameters.RequireSignedTokens = true;
                x.TokenValidationParameters.SignatureValidator = delegate(
                    string token,
                    TokenValidationParameters parameters
                )
                {
                    return new JwtSecurityToken(token);
                };

                var issuers = configuration["Authentication:Jwt:Issuers"];
                if (string.IsNullOrWhiteSpace(issuers) == false)
                {
                    x.TokenValidationParameters.ValidIssuer = null;
                    x.TokenValidationParameters.ValidIssuers = issuers.Split(
                        ';',
                        StringSplitOptions.RemoveEmptyEntries
                    );
                }
                x.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<
                            ILogger<JwtBearerEvents>
                        >();
                        var jwksOptions = context
                            .HttpContext.RequestServices.GetRequiredService<IOptions<JwtOptions>>()
                            .Value;
                        var token = ((JwtSecurityToken)context.SecurityToken).RawData;
                        var tokenHandler = new JwtSecurityTokenHandler();
                        tokenHandler.ValidateToken(
                            token,
                            _validationParameters,
                            out SecurityToken securityToken
                        );
                        if (
                            securityToken is not JwtSecurityToken jwtSecurityToken
                            || !jwtSecurityToken.Header.Alg.Equals(
                                jwksOptions?.Jws.Alg,
                                StringComparison.InvariantCultureIgnoreCase
                            )
                        )
                        {
                            throw new SecurityTokenInvalidSignatureException(
                                "Assinatura do token inválida"
                            );
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
