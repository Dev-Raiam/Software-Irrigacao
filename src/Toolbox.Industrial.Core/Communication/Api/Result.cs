namespace Toolbox.Industrial.Core.Communication.Api;

public class Result<T>
{
    public bool Success { get; private set; }
    public T? Data { get; private set; }
    public string? Error { get; private set; }
    public Exception? Exception { get; private set; }

    private Result(bool success, T? data, string? error, Exception? exception)
    {
        Success = success;
        Data = data;
        Error = error;
        Exception = exception;
    }

    public static Result<T> Ok(T data)
    {
        return new Result<T>(true, data, null, null);
    }

    public static Result<T> Fail(string error, Exception? exception = null)
    {
        return new Result<T>(false, default, error, exception);
    }

    //public static Result<T> Fail(string error, Exception exception)
    //{
    //    return new Result<T>(false, default, error, exception);
    //}
}
