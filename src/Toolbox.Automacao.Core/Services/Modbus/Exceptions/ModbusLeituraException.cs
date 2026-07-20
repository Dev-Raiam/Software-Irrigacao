namespace Toolbox.Automacao.Core.Services.Modbus.Exceptions;

/// <summary>
/// Exceção lançada quando ocorre erro de leitura Modbus
/// </summary>
public sealed class ModbusLeituraException : ModbusException
{
    public ModbusLeituraException(string message) : base(message)
    {
    }

    public ModbusLeituraException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
