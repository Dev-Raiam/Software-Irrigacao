using IrrigacaoInteligente.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IrrigacaoInteligente.Configurations;

public static class ContextoConfiguracao
{
    public static void RegistrarContexto(this IServiceCollection services)
    {
        services.AddDbContext<IrrigacaoInteligenteContext>(options =>
            options.UseSqlite("Data Source=worker-service.db")
        );
    }
}
