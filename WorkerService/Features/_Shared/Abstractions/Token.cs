namespace WorkerService.Features.Shared.Abstractions;

public sealed record Token(
    string TokenAcesso,
    string TokenAtualizacao,
    DateTime Emitido,
    DateTime Expira
);
