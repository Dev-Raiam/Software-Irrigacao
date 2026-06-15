namespace IrrigacaoInteligente.Core;

public class Result<T>
{
    public bool Sucesso { get; private set; }
    public T? Dado { get; private set; }
    public string? Error { get; private set; }
    public Exception? Exception { get; private set; }

    private Result(bool sucesso, T? dado, string? error, Exception? exception)
    {
        Sucesso = sucesso;
        Dado = dado;
        Error = error;
    }

    public static Result<T> Ok(T dado)
    {
        return new Result<T>(true, dado, null, null);
    }

    public static Result<T> Fail(string error)
    {
        return new Result<T>(false, default, error, null);
    }

    public static Result<T> Fail(string error, Exception exception)
    {
        return new Result<T>(false, default, error, exception);
    }
}
