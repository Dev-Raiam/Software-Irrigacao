using Microsoft.EntityFrameworkCore;
using WorkerService.Configurations;
using WorkerService.Features.Configuracao.Credenciais.Interfaces;
using WorkerService.Features.Shared.Abstractions;
using WorkerService.Infrastructure.Data;
using WorkerService.State;

namespace WorkerService.Features.Configuracao.GerenciamentoCredenciais
{
    public sealed class GerenciadorCredenciais(
        WorkerServiceContext _context,
        ICriptografia _servicoCriptografia,
        CredenciaisAplicacao _credenciaisAplicacao
    )
    {
        #region Consultas
        public async Task<bool> ObterContaId(CancellationToken cancellationToken)
        {
            if (!await VerificarConta(cancellationToken))
                return false;

            var contaId = await _context.ConfiguracoesSistema.FirstOrDefaultAsync(
                x => x.Chave == ChavesBanco.Padrao.ContaId,
                cancellationToken
            );

            _credenciaisAplicacao.AdicionarConta(Guid.Parse(contaId!.Valor));

            return true;
        }

        public async Task<bool> ObterPainelId(CancellationToken cancellationToken)
        {
            if (!await VerificarPainel(cancellationToken))
                return false;

            var painelId = await _context.ConfiguracoesSistema.FirstOrDefaultAsync(
                x => x.Chave == ChavesBanco.Padrao.PainelId,
                cancellationToken
            );

            _credenciaisAplicacao.AdicionarPainel(Guid.Parse(painelId!.Valor));

            return true;
        }

        public async Task<bool> ObterCredencialIntegracao(CancellationToken cancellationToken)
        {
            if (!await VerificarIntegracao(cancellationToken))
                return false;

            var chave = await _context.ConfiguracoesSistema.FirstAsync(
                x => x.Chave == ChavesBanco.Integracao.Chave,
                cancellationToken
            );
            var segredo = await _context.ConfiguracoesSistema.FirstAsync(
                x => x.Chave == ChavesBanco.Integracao.Segredo,
                cancellationToken
            );
            var contextoId = await _context.ConfiguracoesSistema.FirstAsync(
                x => x.Chave == ChavesBanco.Integracao.ContextoId,
                cancellationToken
            );

            chave!.Atualizar(_servicoCriptografia.Descriptografar(chave.Valor));
            segredo!.Atualizar(_servicoCriptografia.Descriptografar(segredo.Valor));

            _credenciaisAplicacao.AdicionarIntegracao(
                chave.Valor,
                segredo.Valor,
                Guid.Parse(contextoId.Valor)
            );

            return true;
        }
        #endregion

        #region Adicionar
        public async Task<bool> AdicionarContaId(Guid contaId, CancellationToken cancellationToken)
        {
            if (await VerificarConta(cancellationToken))
                return false;

            var conta = new Domain.Entities.ConfiguracaoSistema(
                ChavesBanco.Padrao.ContaId,
                contaId.ToString()
            );

            await _context.ConfiguracoesSistema.AddAsync(conta, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> AdicionarPainelId(
            Guid painelId,
            CancellationToken cancellationToken
        )
        {
            if (await VerificarPainel(cancellationToken))
                return false;

            var painel = new Domain.Entities.ConfiguracaoSistema(
                ChavesBanco.Padrao.PainelId,
                painelId.ToString()
            );

            await _context.ConfiguracoesSistema.AddAsync(painel, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> AdicionarIntegracao(
            string chave,
            string segredo,
            Guid contextoId,
            CancellationToken cancellationToken
        )
        {
            if (await VerificarIntegracao(cancellationToken))
                return false;

            var integracao = new List<Domain.Entities.ConfiguracaoSistema>
            {
                new(ChavesBanco.Integracao.Chave, _servicoCriptografia.Criptografar(chave)),
                new(ChavesBanco.Integracao.Segredo, _servicoCriptografia.Criptografar(segredo)),
                new(ChavesBanco.Integracao.ContextoId, contextoId.ToString()),
            };

            await _context.ConfiguracoesSistema.AddRangeAsync(integracao, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
        #endregion

        #region  Atualizações
        public async Task<bool> AtualizarConta(Guid contaId, CancellationToken cancellationToken)
        {
            var conta = await _context.ConfiguracoesSistema.FirstOrDefaultAsync(
                x => x.Chave == ChavesBanco.Padrao.ContaId,
                cancellationToken
            );
            if (conta is null)
                return false;

            conta.Atualizar(contaId.ToString());
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> AtualizarPainel(Guid painelId, CancellationToken cancellationToken)
        {
            var painel = await _context.ConfiguracoesSistema.FirstOrDefaultAsync(
                x => x.Chave == ChavesBanco.Padrao.PainelId,
                cancellationToken
            );

            if (painel is null)
                return false;

            painel.Atualizar(painelId.ToString());
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
        #endregion
        #region  Verificaçõens
        public async Task<bool> VerificarCredenciaisExistentes(CancellationToken cancellationToken)
        {
            var existeConta = await VerificarConta(cancellationToken);
            var existeIntegracao = await VerificarIntegracao(cancellationToken);
            var existePainel = await VerificarPainel(cancellationToken);

            return existeConta && existeIntegracao && existePainel;
        }

        private async Task<bool> VerificarConta(CancellationToken cancellationToken)
        {
            var existeContaId = await _context.ConfiguracoesSistema.AnyAsync(
                x => x.Chave == ChavesBanco.Padrao.ContaId,
                cancellationToken
            );

            if (!existeContaId)
                return false;

            return true;
        }

        private async Task<bool> VerificarPainel(CancellationToken cancellationToken)
        {
            var existePainelId = await _context.ConfiguracoesSistema.AnyAsync(
                x => x.Chave == ChavesBanco.Padrao.PainelId,
                cancellationToken
            );

            if (!existePainelId)
                return false;

            return true;
        }

        private async Task<bool> VerificarIntegracao(CancellationToken cancellationToken)
        {
            var existeChave = await _context.ConfiguracoesSistema.AnyAsync(
                x => x.Chave == ChavesBanco.Integracao.Chave,
                cancellationToken
            );
            var existeSegredo = await _context.ConfiguracoesSistema.AnyAsync(
                x => x.Chave == ChavesBanco.Integracao.Segredo,
                cancellationToken
            );
            var existeContextoId = await _context.ConfiguracoesSistema.AnyAsync(
                x => x.Chave == ChavesBanco.Integracao.ContextoId,
                cancellationToken
            );

            if (!existeChave && !existeSegredo && !existeContextoId)
                return false;

            return true;
        }
        #endregion
    }
}
