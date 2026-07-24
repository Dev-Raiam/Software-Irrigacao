namespace Toolbox.Modulo.Tekon.Exceptions;

public class TekonException : Exception
{
    public TekonException()
    {
    }

    public TekonException(string message) : base(message)
    {
    }

    public TekonException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
