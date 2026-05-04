using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace IrrigacaoInteligente.Configurations;

public static class AuthenticacaoConfiguracao
{
    public static void RegistrarAuthenticacao(this IServiceCollection services)
    {
        services
            .AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes("a-string-secret-at-least-256-bits-long")
                    ),
                    ValidAudiences = ["codezone"],
                    ValidIssuers = ["https://codezone.com.br"],
                    ValidateIssuer = true,
                    ValidateAudience = true,
                };
            });
        services.AddAuthorization();
    }
}
