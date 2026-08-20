namespace Irrigacao.Atualizador;

public record UpdateRequest(
    Guid ContaId,
    Guid PainelId,
    Guid ControladorId,
    Guid? AtualizacaoId,
    string VersaoAtual,
    DateTime? DataVersaoAtual,
    int Arquitetura
);

public record UpdateResponse(
    Guid Id,
    Version Versao,
    DateTime Lancamento,
    string UrlBase,
    string UrlDownload
);

public record UpdateConfirm(Guid atualizacaoId);
