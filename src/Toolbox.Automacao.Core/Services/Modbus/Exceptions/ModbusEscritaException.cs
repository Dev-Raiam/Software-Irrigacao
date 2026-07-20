namespace Toolbox.Automacao.Core.Services.Modbus.Exceptions;

/// <summary>
/// Exceção lançada quando ocorre erro de escrita Modbus
/// </summary>
public sealed class ModbusEscritaException : ModbusException
{
    public ModbusEscritaException(string message) : base(message)
    {
    }

    public ModbusEscritaException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
