using LiteDB;
using Microsoft.Extensions.Logging;
using Toolbox.Automacao.Core.Data;
using Toolbox.Automacao.Core.Models;

namespace Toolbox.Automacao.Core.Services.Automacao.Autenticacao
{
    public interface IConfiguracaoAutenticacao
    {
        void AdicionarCredenciais(Credencial credencial);
    }

    internal class ConfiguracaoAutenticacao : IConfiguracaoAutenticacao
    {
        private readonly ICriptografia _criptografia;
        private readonly ILiteDatabase _dataBase;
        private readonly ILogger<ConfiguracaoAutenticacao> _logger;

        public ConfiguracaoAutenticacao(
            ICriptografia criptografia,
            ILiteDatabase database,
            ILogger<ConfiguracaoAutenticacao> logger
        )
        {
            _criptografia = criptografia;
            _dataBase = database;
            _logger = logger;
        }

        public void AdicionarCredenciais(Credencial credencial)
        {
            ValidarCredenciais(credencial);

            var chave = _criptografia.Criptografar(credencial.chave);
            var segredo = _criptografia.Criptografar(credencial.segredo);

            Configuracao[] configuracoes =
            [
                new(ChavesBanco.Integracao.Chave, chave),
                new(ChavesBanco.Integracao.Segredo, segredo),
                new(ChavesBanco.Integracao.ContextoId, credencial.contextoId.ToString()),
            ];

            try
            {
                foreach (var configuracao in configuracoes)
                {
                    _dataBase
                        .GetCollection<Configuracao>(TabelaNome.Configuracoes)
                        .Upsert(configuracao);
                }
            }
            catch (IOException ex)
            {
                _logger.LogError(
                    "O processo não pode acessar o arquivo da banco de dados. {ex}",
                    ex.Message
                );
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro desconhecido. {ex}", ex.Message);
            }
        }

        private void ValidarCredenciais(Credencial credencial)
        {
            if (string.IsNullOrEmpty(credencial.chave))
                throw new ArgumentNullException(
                    nameof(credencial.chave),
                    "A chave de autenticação não pode ser nula ou vazia."
                );
            if (string.IsNullOrEmpty(credencial.segredo))
                throw new ArgumentNullException(
                    nameof(credencial.segredo),
                    "O segredo de autenticação não pode ser nulo ou vazio."
                );
            if (credencial.contextoId == Guid.Empty)
                throw new ArgumentException(
                    "O contextoId não pode ser vazio (Guid.Empty).",
                    nameof(credencial.contextoId)
                );
        }
    }
}
