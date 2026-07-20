namespace Toolbox.Automacao.Core.Services.Modbus.Exceptions;

/// <summary>
/// Exceção lançada quando ocorre erro de conexão Modbus
/// </summary>
public sealed class ModbusConexaoException : ModbusException
{
    public ModbusConexaoException(string message) : base(message)
    {
    }

    public ModbusConexaoException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
