//using LiteDB;
//using Microsoft.Extensions.Logging;
//using Toolbox.Automacao.Core.Data;
//using Toolbox.Automacao.Core.Models;

//namespace Toolbox.Automacao.Core.Services.Automacao.Autenticacao
//{
//    public interface IConfiguracaoAutenticacao
//    {
//        void AdicionarCredenciais(Credencial credencial);
//    }

//    internal class ConfiguracaoAutenticacao : IConfiguracaoAutenticacao
//    {
//        private readonly ICriptografia _criptografia;
//        private readonly ILiteDatabase _dataBase;

//        public ConfiguracaoAutenticacao(ICriptografia criptografia, ILiteDatabase database)
//        {
//            _criptografia = criptografia;
//            _dataBase = database;
//        }

//        public void AdicionarCredenciais(Credencial credencial)
//        {
//            if (string.IsNullOrEmpty(credencial.chave))
//                throw new ArgumentNullException(
//                    nameof(credencial.chave),
//                    "A chave de autenticação não pode ser nula ou vazia."
//                );
//            if (string.IsNullOrEmpty(credencial.segredo))
//                throw new ArgumentNullException(
//                    nameof(credencial.segredo),
//                    "O segredo de autenticação não pode ser nulo ou vazio."
//                );
//            if (credencial.contextoId == Guid.Empty)
//                throw new ArgumentException(
//                    "O contextoId não pode ser vazio (Guid.Empty).",
//                    nameof(credencial.contextoId)
//                );

//            var chaveCriptografada = _criptografia.Criptografar(credencial.chave);
//            var segredoCriptografado = _criptografia.Criptografar(credencial.segredo);

//            Configuracao[] configuracoes =
//            [
//                new(ChavesBanco.Integracao.Chave, chaveCriptografada),
//                new(ChavesBanco.Integracao.Segredo, segredoCriptografado),
//                new(ChavesBanco.Integracao.ContextoId, credencial.contextoId.ToString()),
//            ];

//            foreach (var configuracao in configuracoes)
//            {
//                _dataBase.GetCollection<Configuracao>(Tabela.Configuracoes).Upsert(configuracao);
//            }
//        }
//    }
//}
