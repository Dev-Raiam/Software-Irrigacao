using Microsoft.EntityFrameworkCore;
using Toolbox.Automacao.Core.Data;
using Toolbox.Automacao.Core.Models;

namespace Toolbox.Automacao.Core.Services;

/// <summary>
/// Pegar dados Da Api ou Cache ou Banco.
/// </summary>
internal sealed class ProvedorDataSincronizacao : IProvedorDataSincronizacao
{
    private readonly SincronizacaoDbContext _context;

    public ProvedorDataSincronizacao(SincronizacaoDbContext context)
    {
        _context = context;
    }

    public async Task<Controlador?> ObterControlador(CancellationToken cancellationToken)
    {
        var controlador = await ObterControladorMaster(cancellationToken);
        return controlador;
    }

    public async Task<List<Dispositivo>> ObterDispositivos(CancellationToken cancellationToken)
    {
        var controlador = await ObterControladorMaster(cancellationToken);

        List<Dispositivo> dispositivos = new List<Dispositivo>();

        foreach (var dispositivo in controlador.Dispositivos)
        {
            dispositivos.Add(dispositivo);
        }

        return dispositivos;
    }

    public async Task<List<Modulo>> ObterModulos(CancellationToken cancellationToken)
    {
        var controlador = await ObterControladorMaster(cancellationToken);

        List<Modulo> modulos = new List<Modulo>();

        foreach (var modulo in controlador.Modulos)
        {
            modulos.Add(modulo);
        }

        return modulos;
    }

    private async Task<Controlador> ObterControladorMaster(CancellationToken cancellationToken)
    {
        var controladores = await _context.Set<ControladorConfiguracao>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (controladores.Count == 0)
            return new();

        var configuracao = controladores.FirstOrDefault(c => c.Controlador.Master == true);

        if (configuracao == null)
            return new();

        return configuracao.Controlador;
    }
}
