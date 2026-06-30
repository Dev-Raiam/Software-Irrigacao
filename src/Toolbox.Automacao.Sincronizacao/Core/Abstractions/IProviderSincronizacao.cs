using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Automacao.Sincronizacao.Provider;

namespace Toolbox.Automacao.Sincronizacao.Core.Abstractions
{
    public interface IProviderSincronizacao
    {
        Task<Controlador> ObterControlador(CancellationToken cancellationToken);
        Task<List<Modulo>> ObterModulos(CancellationToken cancellationToken);
    }
}
