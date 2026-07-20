namespace Toolbox.Modulo.Tekon.Exceptions;

public class TekonLeituraException : TekonException
{
    public TekonLeituraException()
    {
    }

    public TekonLeituraException(string message) : base(message)
    {
    }

    public TekonLeituraException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
