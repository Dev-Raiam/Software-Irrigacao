using LiteDB;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using Toolbox.Automacao.Core.Data;
using Toolbox.Automacao.Core.Models;

namespace Toolbox.Automacao.Core.Services;

public class AutenticacaoHandler : DelegatingHandler
{
    private readonly IServicoAutenticacao _autenticacao;
    private readonly ILiteDatabase _database;
    private readonly ICriptografia _criptografia;
    private readonly ILogger<AutenticacaoHandler> _logger;
    private readonly Token _token;

    public AutenticacaoHandler(
        IServicoAutenticacao autenticacao,
        ILiteDatabase database,
        ICriptografia criptografia,
        ILogger<AutenticacaoHandler> logger,
        Token token
    )
    {
        _autenticacao = autenticacao;
        _database = database;
        _criptografia = criptografia;
        _logger = logger;
        _token = token;
    }

    private Credencial ObterCredencial()
    {
        var colecao = _database.GetCollection<Configuracao>(Tabela.Configuracoes);

        var chaveConfig = colecao.FindOne(c => c.Chave == ChavesBanco.Integracao.Chave);
        var segredoConfig = colecao.FindOne(c => c.Chave == ChavesBanco.Integracao.Segredo);
        var contextoIdConfig = colecao.FindOne(c => c.Chave == ChavesBanco.Integracao.ContextoId);

        if (chaveConfig == null || segredoConfig == null || contextoIdConfig == null)
        {
            throw new InvalidOperationException("Credenciais não encontradas no banco.");
        }

        var chave = _criptografia.Descriptografar(chaveConfig.Valor);
        var segredo = _criptografia.Descriptografar(segredoConfig.Valor);
        var contextoId = Guid.Parse(contextoIdConfig.Valor);

        return new Credencial(chave, segredo, contextoId);
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (_token.Expirado)
        {
            var credencial = ObterCredencial();

            var response = await _autenticacao.Autenticar(credencial, cancellationToken);

            if (!response.Sucesso || response.Dado == null)
            {
                _logger.LogError("Falha ao autenticar: {Error}", response.Error);
                return await base.SendAsync(request, cancellationToken);
            }

            _token.Atualizar(
                response.Dado.TokenAcesso!,
                response.Dado.TokenAtualizacao!,
                response.Dado.Emitido,
                response.Dado.Expira
            );
        }

        if (!string.IsNullOrEmpty(_token.TokenAcesso))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _token.TokenAcesso
            );
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
