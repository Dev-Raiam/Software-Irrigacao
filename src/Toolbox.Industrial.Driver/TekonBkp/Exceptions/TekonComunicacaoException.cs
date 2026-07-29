namespace Toolbox.Industrial.Driver.TekonBkp.Exceptions;

public class TekonComunicacaoException : TekonException
{
    public TekonComunicacaoException()
    {
    }

    public TekonComunicacaoException(string message) : base(message)
    {
    }

    public TekonComunicacaoException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
