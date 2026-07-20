using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Toolbox.Core.Mediator;

namespace SoftwareIrrigacao.Infrastructure.Handlers.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var (status, mensagem) = exception switch
        {
            IOException => (
                HttpStatusCode.ServiceUnavailable,
                "Não foi possível salvar ou ler os dados. Tente novamente."
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.Forbidden,
                "Você não tem permissão para realizar essa ação."
            ),
            ArgumentException => (
                HttpStatusCode.BadRequest,
                "Algumas informações enviadas são inválidas."
            ),
            InvalidOperationException => (
                HttpStatusCode.BadRequest,
                "Não foi possível concluir a operação neste momento."
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "Ocorreu um erro inesperado. Tente novamente mais tarde."
            ),
        };

        _logger.LogError(
            exception,
            "Falha ao processar {Path} - {Method}",
            httpContext.Request.Path,
            httpContext.Request.Method
        );

        var resposta = ResponseResult.Result(status).AddError(mensagem);

        httpContext.Response.StatusCode = (int)status;
        await httpContext.Response.WriteAsJsonAsync(resposta, cancellationToken);

        return true;
    }
}
