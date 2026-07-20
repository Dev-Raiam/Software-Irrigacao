namespace Toolbox.Modulo.Tekon.Exceptions;

public class TekonEscritaException : TekonException
{
    public TekonEscritaException()
    {
    }

    public TekonEscritaException(string message) : base(message)
    {
    }

    public TekonEscritaException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
