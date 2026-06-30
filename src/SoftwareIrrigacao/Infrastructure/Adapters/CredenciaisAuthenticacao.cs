using Microsoft.EntityFrameworkCore;
using SoftwareIrrigacao.Data;
using SoftwareIrrigacao.Shared.Constants;
using SoftwareIrrigacao.Shared.Contracts;
using Toolbox.Automacao.Autenticacao;
using Toolbox.Automacao.Autenticacao.Dtos;

namespace SoftwareIrrigacao.Infrastructure.Adapters
{
    public class CredenciaisAuthenticacao : ICredenciaisAutenticacao
    {
        private readonly SoftwareIrrigacaoContext _context;
        private readonly ICriptografia _criptografia;

        public CredenciaisAuthenticacao(
            SoftwareIrrigacaoContext context,
            ICriptografia criptografia
        )
        {
            _context = context;
            _criptografia = criptografia;
        }

        public async Task<Credencial> ObterCredencial(CancellationToken cancellationToken)
        {
            var chave = await _context.Configuracoes.FirstOrDefaultAsync(
                x => x.Chave == ChavesBanco.Integracao.Chave,
                cancellationToken
            );

            var segredo = await _context.Configuracoes.FirstOrDefaultAsync(
                x => x.Chave == ChavesBanco.Integracao.Segredo,
                cancellationToken
            );

            var contextoId = await _context.Configuracoes.FirstOrDefaultAsync(
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
