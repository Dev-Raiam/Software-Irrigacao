namespace Toolbox.Industrial.Driver.TekonBkp.Exceptions;

public class TekonOperacaoNaoSuportadaException : TekonException
{
    public TekonOperacaoNaoSuportadaException()
    {
    }

    public TekonOperacaoNaoSuportadaException(string message) : base(message)
    {
    }

    public TekonOperacaoNaoSuportadaException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
