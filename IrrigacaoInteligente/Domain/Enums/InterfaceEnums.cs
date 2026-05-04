using System.Runtime.Serialization;

namespace IrrigacaoInteligente.Domain.Enums;

public enum InterfaceTipo : int
{
    [EnumMember(Value = "Bluetooth")]
    Bluetooth = 1,

    [EnumMember(Value = "Ethernet")]
    Ethernet = 2,

    [EnumMember(Value = "Wi-Fi")]
    WiFi = 3,

    [EnumMember(Value = "Serial")]
    Serial = 4,

    [EnumMember(Value = "RS485")]
    RS485 = 5,

    [EnumMember(Value = "USB")]
    USB = 6,

    [EnumMember(Value = "PWM")]
    PWM = 7,

    [EnumMember(Value = "I2C")]
    I2C = 8,

    [EnumMember(Value = "SPI")]
    SPI = 9,
}
