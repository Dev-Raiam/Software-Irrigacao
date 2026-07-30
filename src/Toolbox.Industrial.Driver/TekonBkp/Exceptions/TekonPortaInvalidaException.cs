namespace Toolbox.Industrial.Driver.TekonBkp.Exceptions;

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
