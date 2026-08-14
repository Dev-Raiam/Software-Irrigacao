namespace Irrigacao.Atualizador;

// UpdateRequest
public record AtualizacaoDisponivel(
    Guid ContaId,
    Guid PainelId,
    Guid ControladorId,
    Guid? AtualizacaoId,
    string VersaoAtual,
    DateTime? DataVersaoAtual,
    int Arquitetura
);

// UpdateResponse
public record AtualizacaoResposta(
    Guid Id,
    Version Versao,
    DateTime Lancamento,
    string UrlBase,
    string UrlDownload
);

public record UpdateInstallationConfig(
    string BinaryName,
    string ServiceName,
    string UpdateDirectory,
    string BackupPath
);
