using IrrigacaoInteligente.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IrrigacaoInteligente.Configurations;

public static class ContextoConfiguracao
{
    public static void RegistrarContexto(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<IrrigacaoInteligenteContext>(options =>
            options.UseSqlite(configuration.GetSection("PathDatabase:Path").Value)
        );
    }
}
