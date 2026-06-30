using Microsoft.EntityFrameworkCore;
using Toolbox.Automacao.Sincronizacao.Core.Abstractions;
using Toolbox.Automacao.Sincronizacao.Infrastructure.Data;
using Toolbox.Automacao.Sincronizacao.Provider.Extensions;

namespace Toolbox.Automacao.Sincronizacao.Provider
{
    /// <summary>
    /// Pegar dados Da Api ou Cache ou Banco.
    /// </summary>
    internal sealed class ProviderSincronizacao : IProviderSincronizacao
    {
        private readonly SincronizacaoDbContext _context;

        public ProviderSincronizacao(SincronizacaoDbContext context)
        {
            _context = context;
        }

        public async Task<Controlador> ObterControlador(CancellationToken cancellationToken)
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
            var controladores = await _context
                .ControladoresConfiguracao.AsNoTracking()
                .ToListAsync(cancellationToken);

            if (controladores.Count == 0)
                return new();

            var configuracao = controladores.FirstOrDefault(c => c.Controlador.Master == true);

            if (configuracao == null)
                return new();

            return configuracao.Controlador.Mapper();
        }
    }
}
