using Microsoft.EntityFrameworkCore;
using WorkerService.Configurations;
using WorkerService.Features.Configuracao.GerenciamentoCredenciais.Interfaces;
using WorkerService.Features.Shared.Abstractions;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Features.Configuracao.GerenciamentoCredenciais
{
    public sealed class GerenciadorCredenciais(
        WorkerServiceContext _context,
        IntegracaoConfiguracao _integracaoConfig,
        TopicoConfiguracao _topicoConfig,
        ICriptografia _servicoCriptografia,
        ContaConfiguracao _contaConfiguracao,
        ILogger<GerenciadorCredenciais> _logger
    )
    {
        public async Task<bool> ObterContaId(CancellationToken cancellationToken)
        {
            if (!await VerificarExistenciaCredenciaisContaId(cancellationToken))
            {
                return false;
            }
            var contaId = await _context.ConfiguracoesSistema.FirstOrDefaultAsync(
                x => x.Chave == ChavesBanco.Padrao.ContaId,
                cancellationToken
            );
            _contaConfiguracao.ContaId = Guid.Parse(contaId!.Valor);
            return true;
        }

        public async Task<bool> ObterCredencialIntegracao(CancellationToken cancellationToken)
        {
            if (!await VerificarExistenciaCredenciaisIntegracao(cancellationToken))
            {
                return false;
            }

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

            _integracaoConfig.Chave = chave.Valor;
            _integracaoConfig.Segredo = segredo.Valor;
            _integracaoConfig.ContextoId = Guid.Parse(contextoId.Valor);
            return true;
        }

        public async Task<bool> ObterCredencialTopicoConfiguracao(
            CancellationToken cancellationToken
        )
        {
            var existeTopicoConfiguracao = await _context.ConfiguracoesSistema.AnyAsync(
                x => x.Chave == ChavesBanco.Padrao.TopicoConfiguracao,
                cancellationToken
            );
            if (!existeTopicoConfiguracao)
                return false;

            var topico = await _context.ConfiguracoesSistema.FirstAsync(
                x => x.Chave == ChavesBanco.Padrao.TopicoConfiguracao,
                cancellationToken
            );
            _topicoConfig.Topico = topico.Valor;
            return true;
        }

        public async Task<bool> DefinirContaId(Guid contaId, CancellationToken cancellationToken)
        {
            if (await VerificarExistenciaCredenciaisContaId(cancellationToken))
            {
                return true;
            }
            var configuracao = new Domain.Entities.ConfiguracaoSistema(
                ChavesBanco.Padrao.ContaId,
                contaId.ToString()
            );
            await _context.ConfiguracoesSistema.AddAsync(configuracao, cancellationToken);
            var salvo = await _context.SaveChangesAsync(cancellationToken);
            if (salvo == 0)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> AtualizarContaId(Guid contaId, CancellationToken cancellationToken)
        {
            var configuracao = await _context.ConfiguracoesSistema.FirstOrDefaultAsync(
                x => x.Chave == ChavesBanco.Padrao.ContaId,
                cancellationToken
            );
            if (configuracao == null)
            {
                return false;
            }
            configuracao.Atualizar(contaId.ToString());
            var salvo = await _context.SaveChangesAsync(cancellationToken);
            if (salvo == 0)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> DefinirCredencialIntegracao(
            string chave,
            string segredo,
            Guid contextoId,
            CancellationToken cancellationToken
        )
        {
            if (await VerificarExistenciaCredenciaisIntegracao(cancellationToken))
            {
                return true;
            }
            var configuracoes = new List<Domain.Entities.ConfiguracaoSistema>
            {
                new(ChavesBanco.Integracao.Chave, _servicoCriptografia.Criptografar(chave)),
                new(ChavesBanco.Integracao.Segredo, _servicoCriptografia.Criptografar(segredo)),
                new(ChavesBanco.Integracao.ContextoId, contextoId.ToString()),
            };

            await _context.ConfiguracoesSistema.AddRangeAsync(configuracoes, cancellationToken);
            var salvo = await _context.SaveChangesAsync(cancellationToken);
            if (salvo == 0)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> DefinirTopicoConfiguracao(
            string topico,
            CancellationToken cancellationToken
        )
        {
            var existeTopicoConfiguracao = await _context.ConfiguracoesSistema.AnyAsync(
                x => x.Chave == ChavesBanco.Padrao.TopicoConfiguracao,
                cancellationToken
            );
            if (existeTopicoConfiguracao)
            {
                _logger.LogWarning("Topico Configuracao já existe no banco de dados");
                return true;
            }
            var configuracao = new Domain.Entities.ConfiguracaoSistema(
                ChavesBanco.Padrao.TopicoConfiguracao,
                topico
            );
            await _context.ConfiguracoesSistema.AddAsync(configuracao, cancellationToken);
            var salvo = await _context.SaveChangesAsync(cancellationToken);
            if (salvo == 0)
            {
                return false;
            }
            return true;
        }

        // public async Task DefinirCredencialIntegracao()
        // {
        //     var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        //     var path = Path.GetFullPath(
        //         "D:\\Desenvolvimento\\Backend\\WorkerService\\WorkerService\\credenciais.json"
        //     );

        //     if (!File.Exists(path))
        //     {
        //         return;
        //     }

        //     var file = await File.ReadAllTextAsync(path);

        //     var integrationCredencial = JsonSerializer.Deserialize<IntegrationCredential>(
        //         file,
        //         jsonOptions
        //     );

        //     var configuracoens = new List<ConfiguracaoSistema>
        //     {
        //         new(
        //             Config.Integracao.Chave,
        //             _encryptionService.Criptografar(integrationCredencial!.Chave!)
        //         ),
        //         new(
        //             Config.Integracao.Segredo,
        //             _encryptionService.Criptografar(integrationCredencial.Segredo!)
        //         ),
        //         new(Config.Integracao.ContextoId, integrationCredencial.ContextoId.ToString()),
        //     };

        //     await _context.ConfiguracoesSistema.AddRangeAsync(configuracoens);
        //     await _context.SaveChangesAsync();

        //     File.Delete(path);
        // }

        private async Task<bool> VerificarExistenciaCredenciaisContaId(
            CancellationToken cancellationToken
        )
        {
            var existeContaId = await _context.ConfiguracoesSistema.AnyAsync(
                x => x.Chave == ChavesBanco.Padrao.ContaId,
                cancellationToken
            );
            if (!existeContaId)
                return false;

            return true;
        }

        private async Task<bool> VerificarExistenciaCredenciaisIntegracao(
            CancellationToken cancellationToken
        )
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
    }
}
