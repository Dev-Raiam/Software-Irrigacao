namespace Toolbox.Automacao.Core.Services.Modbus.Exceptions;

/// <summary>
/// Exceção lançada quando ocorre erro de comunicação Modbus
/// </summary>
public sealed class ModbusComunicacaoException : ModbusException
{
    public ModbusComunicacaoException(string message) : base(message)
    {
    }

    public ModbusComunicacaoException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
