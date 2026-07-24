namespace Toolbox.Industrial.Driver.Tekon.Exceptions;

public class TekonPortaInvalidaException : TekonException
{
    public TekonPortaInvalidaException()
    {
    }

    public TekonPortaInvalidaException(string message) : base(message)
    {
    }

    public TekonPortaInvalidaException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
