using LiteDB;
using Toolbox.Automacao.Core.Data;
using Toolbox.Automacao.Core.Models;

namespace Toolbox.Automacao.Core.Services;

public record Credencial
{
    public string Chave { get; private set; } = null!;
    public string Segredo { get; private set; } = null!;
    public Guid ContextoId { get; private set; }
    public Guid PainelId { get; private set; }

    public Credencial(string chave, string segredo, Guid contextoId, Guid painelId)
    {
        if (string.IsNullOrWhiteSpace(chave))
            throw new ArgumentNullException();
        if (string.IsNullOrWhiteSpace(segredo))
            throw new ArgumentNullException();
        if (contextoId == Guid.Empty)
            throw new ArgumentNullException();
        if (painelId == Guid.Empty)
            throw new ArgumentNullException();

        Chave = chave;
        Segredo = segredo;
        ContextoId = contextoId;
        PainelId = painelId;
    }
};

public record Integracao(string chave, string segredo, Guid contextoId);

public interface IGerenciadorConfiguracao
{
    void AdicionarCredenciais(Credencial credenciais);
    Integracao? ObterCredenciaisIntegracao();
    Guid ObterCredencialPainel();
    bool ExisteCredenciaisIntegracao();
    bool ExisteCredencialPainel();
}

internal class GerenciadorConfiguracao : IGerenciadorConfiguracao
{
    private readonly ILiteDatabase _database;
    private readonly ICriptografia _criptografia;

    public GerenciadorConfiguracao(ILiteDatabase database, ICriptografia criptografia)
    {
        _database = database;
        _criptografia = criptografia;
    }

    public void AdicionarCredenciais(Credencial credenciais)
    {
        var chaveCriptografada = _criptografia.Criptografar(credenciais.Chave);
        var segredoCriptografado = _criptografia.Criptografar(credenciais.Segredo);

        Configuracao[] configuracoes =
        [
            new(ChaveConfiguracao.Integracao.Chave, chaveCriptografada!),
            new(ChaveConfiguracao.Integracao.Segredo, segredoCriptografado!),
            new(ChaveConfiguracao.Integracao.ContextoId, credenciais.ContextoId.ToString()),
            new(ChaveConfiguracao.Padrao.PainelId, credenciais.PainelId.ToString()),
        ];

        foreach (var configuracao in configuracoes)
        {
            _database.GetCollection<Configuracao>(Tabela.Configuracoes).Upsert(configuracao);
        }
    }

    public bool ExisteCredenciaisIntegracao()
    {
        var colecao = _database.GetCollection<Configuracao>(Tabela.Configuracoes);

        var chave = colecao.FindOne(x => x.Chave == ChaveConfiguracao.Integracao.Chave);
        var segredo = colecao.FindOne(x => x.Chave == ChaveConfiguracao.Integracao.Segredo);
        var contextoId = colecao.FindOne(x => x.Chave == ChaveConfiguracao.Integracao.ContextoId);

        return chave != null && segredo != null && contextoId != null;
    }

    public bool ExisteCredencialPainel()
    {
        var colecao = _database.GetCollection<Configuracao>(Tabela.Configuracoes);
        return colecao.Exists(x => x.Chave == ChaveConfiguracao.Padrao.PainelId);
    }

    public Integracao? ObterCredenciaisIntegracao()
    {
        var colecao = _database.GetCollection<Configuracao>(Tabela.Configuracoes);

        var chave = colecao.FindOne(x => x.Chave == ChaveConfiguracao.Integracao.Chave);
        var segredo = colecao.FindOne(x => x.Chave == ChaveConfiguracao.Integracao.Segredo);
        var contextoId = colecao.FindOne(x => x.Chave == ChaveConfiguracao.Integracao.ContextoId);

        if (chave == null || segredo == null || contextoId == null)
            return null;

        return new Integracao(
            _criptografia.Descriptografar(chave.Valor),
            _criptografia.Descriptografar(segredo.Valor),
            Guid.Parse(contextoId.Valor)
        );
    }

    public Guid ObterCredencialPainel()
    {
        var colecao = _database.GetCollection<Configuracao>(Tabela.Configuracoes);

        var configuracao = colecao.FindOne(x => x.Chave == ChaveConfiguracao.Padrao.PainelId);

        var painelId = configuracao != null ? Guid.Parse(configuracao.Valor) : Guid.Empty;

        return painelId;
    }
}
