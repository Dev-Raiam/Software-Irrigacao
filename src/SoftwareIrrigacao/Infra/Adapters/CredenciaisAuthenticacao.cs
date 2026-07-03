using Microsoft.EntityFrameworkCore;
using SoftwareIrrigacao.Infra.Data;
using Toolbox.Automacao.Core.Data;
using Toolbox.Automacao.Core.Models;
using Toolbox.Automacao.Core.Services;

namespace SoftwareIrrigacao.Infra.Adapters
{
    public class CredenciaisAuthenticacao : ICredenciaisAutenticacao
    {
        private readonly IrrigacaoDbContext _context;
        private readonly ICriptografia _criptografia;

        public CredenciaisAuthenticacao(
            IrrigacaoDbContext context,
            ICriptografia criptografia
        )
        {
            _context = context;
            _criptografia = criptografia;
        }

        public async Task<Credencial> ObterCredencial(CancellationToken cancellationToken)
        {
            var chave = await _context
                .Set<Toolbox.Automacao.Core.Models.Configuracao>()
                .FirstOrDefaultAsync(
                    x => x.Chave == ChavesBanco.Integracao.Chave,
                    cancellationToken
                );

            var segredo = await _context
                .Set<Toolbox.Automacao.Core.Models.Configuracao>()
                .FirstOrDefaultAsync(
                    x => x.Chave == ChavesBanco.Integracao.Segredo,
                    cancellationToken
                );

            var contextoId = await _context
                .Set<Toolbox.Automacao.Core.Models.Configuracao>()
                .FirstOrDefaultAsync(
                    x => x.Chave == ChavesBanco.Integracao.ContextoId,
                    cancellationToken
                );

            if (chave == null && segredo == null && contextoId == null)
                return new Credencial();

            var credencial = new Credencial
            {
                Chave = _criptografia.Descriptografar(chave!.Valor),
                Segredo = _criptografia.Descriptografar(segredo!.Valor),
                ContextoId = Guid.Parse(contextoId!.Valor),
            };

            return credencial;
        }
    }
}
