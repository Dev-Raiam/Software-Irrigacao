namespace WorkerService.Infrastructure.Services;

public sealed record Token(
    string TokenAcesso,
    string TokenAtualizacao,
    DateTime Emitido,
    DateTime Expira
);
