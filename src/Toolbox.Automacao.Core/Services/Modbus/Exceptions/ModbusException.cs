namespace Toolbox.Automacao.Core.Services.Modbus.Exceptions;

/// <summary>
/// Exceção base para erros relacionados ao Modbus
/// </summary>
public abstract class ModbusException : Exception
{
    protected ModbusException(string message) : base(message)
    {
    }

    protected ModbusException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
