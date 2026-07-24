using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SoftwareIrrigacao.Setup;

public static class JwtConfig
{
    public static void AddJwtConfiguration(this IServiceCollection services)
    {
        //TODO preciso dos dados de configuração do JWT (issuer, audience, secret key) da Toolbox
        // services
        //     .AddAuthentication()
        //     .AddJwtBearer(options =>
        //     {
        //         options.TokenValidationParameters = new TokenValidationParameters
        //         {
        //             ValidateIssuerSigningKey = true,
        //             IssuerSigningKey = new SymmetricSecurityKey(
        //                 Encoding.UTF8.GetBytes("a-string-secret-at-least-256-bits-long")
        //             ),
        //             ValidAudiences = ["codezone"],
        //             ValidIssuers = ["https://codezone.com.br"],
        //             ValidateIssuer = true,
        //             ValidateAudience = true,
        //         };
        //     });
        // services.AddAuthorization();
    }
}
