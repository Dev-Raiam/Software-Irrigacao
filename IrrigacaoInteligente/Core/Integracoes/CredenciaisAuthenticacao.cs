using Autenticacao.Interfaces;
using Autenticacao.Models;
using IrrigacaoInteligente.Core.Criptografia;
using IrrigacaoInteligente.Core.DataBase;
using IrrigacaoInteligente.Core.Shared.Utils;
using Microsoft.EntityFrameworkCore;

namespace IrrigacaoInteligente.Features.Configuracao
{
    public class CredenciaisAuthenticacao : ICredenciaisAutenticacao
    {
        private readonly IrrigacaoInteligenteContext _context;
        private readonly ICriptografia _criptografia;

        public CredenciaisAuthenticacao(
            IrrigacaoInteligenteContext context,
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

            if (chave != null && segredo != null && contextoId != null)
            {
                var credencial = new Credencial
                {
                    Chave = _criptografia.Descriptografar(chave.Valor),
                    Segredo = _criptografia.Descriptografar(segredo.Valor),
                    ContextoId = Guid.Parse(contextoId.Valor),
                };

                return credencial;
            }

            return new Credencial();
        }
    }
}
