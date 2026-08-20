using System.Runtime.InteropServices;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Extensions;

namespace Irrigacao.Atualizador.Extensions
{
    // Mudar para o Toolbox.Industrial.Core
    public static class StoreExtensions
    {
        public static async Task<UpdateRequest> ObterCredenciais(this IEntityStore store)
        {
            var contaId = await store.ObterConfiguracao<Guid>(Entity.Keys.ContaId);
            var painelId = await store.ObterConfiguracao<Guid>(Entity.Keys.PainelId);
            var controladorId = await store.ObterConfiguracao<Guid>(Entity.Keys.ControladorId);
            var versaoAtual = await store.ObterConfiguracao<string>(Entity.Keys.VersaoAtual);

            return new UpdateRequest(
                contaId,
                painelId,
                controladorId,
                null,
                versaoAtual ?? "",
                null,
                (int)RuntimeInformation.OSArchitecture
            );
        }
    }
}
