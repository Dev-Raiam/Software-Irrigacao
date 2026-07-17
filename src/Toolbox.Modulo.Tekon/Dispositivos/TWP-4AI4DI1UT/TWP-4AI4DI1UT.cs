using Toolbox.Modulo.Tekon.Interfaces;
using Toolbox.Modulo.Tekon.Models;

namespace Toolbox.Modulo.Tekon.Dispositivos
{
    public class TWP_4AI4DI1UT : ITekonDispositivoDado
    {
        public long NumeroSerie { get; init; }
        public string Modelo { get; init; } = null!;
        public int RSSI { get; init; }
        public int PeriodoComunicacao { get; init; }
        public int TempoDecorrido { get; init; }
        public float TensaoAlimentacao { get; init; }
        public float TemperaturaExterna { get; init; }
        public float ValorEntradaAnalogica_1 { get; init; }
        public float ValorEntradaAnalogica_2 { get; init; }
        public float ValorEntradaAnalogica_3 { get; init; }
        public float ValorEntradaAnalogica_4 { get; init; }
        public int VersaoFirmware { get; init; }
        public int RevisaoVersao { get; init; }
        public int VersaoHardware { get; init; }
        public bool EstadoSaidaRemotaDigital { get; init; }
        public bool EstadoSaidaEnergiaExterna { get; init; }
        public bool EstadoEntradaInterruptor { get; init; }
        public bool EstadoEntradaDigital_1 { get; init; }
        public bool EstadoEntradaDigital_2 { get; init; }
        public bool EstadoEntradaDigital_3 { get; init; }
        public bool EstadoEntradaDigital_4 { get; init; }

        long? ITekonDispositivoDado.NumeroSerie => NumeroSerie;

        public IEnumerable<Metrica> ObterMetricas()
        {
            // Transformar em um Array
            yield return new Metrica
            {
                Porta = "B8/B7",
                Tipo = Metricas.Tipos.TemperaturaExterna,
                Valor = TemperaturaExterna,
                Unidade = Metricas.UnidadeMedidas.Celsius,
            };
            yield return new Metrica
            {
                Tipo = Metricas.Tipos.TensaoAlimentacao,
                Valor = TensaoAlimentacao,
                Unidade = Metricas.UnidadeMedidas.Volts,
            };
            yield return new Metrica
            {
                Tipo = Metricas.Tipos.RSSI,
                Valor = RSSI,
                Unidade = Metricas.UnidadeMedidas.Decibeis,
            };
            yield return new Metrica
            {
                Porta = "Q1",
                Tipo = Metricas.Tipos.EstadoSaidaRemotaDigital,
                Valor = EstadoSaidaRemotaDigital,
                Unidade = Metricas.UnidadeMedidas.Boolean,
            };
            yield return new Metrica
            {
                Porta = "Q2",
                Tipo = Metricas.Tipos.EstadoSaidaEnergiaExterna,
                Valor = EstadoSaidaEnergiaExterna,
                Unidade = Metricas.UnidadeMedidas.Boolean,
            };
            yield return new Metrica
            {
                Porta = "Q3",
                Tipo = Metricas.Tipos.EstadoEntradaInterruptor,
                Valor = EstadoEntradaInterruptor,
                Unidade = Metricas.UnidadeMedidas.Boolean,
            };
            yield return new Metrica
            {
                Porta = "B1",
                Tipo = Metricas.Tipos.EstadoEntradaDigital_1,
                Valor = EstadoEntradaDigital_1,
                Unidade = Metricas.UnidadeMedidas.Boolean,
            };
            yield return new Metrica
            {
                Porta = "B2",
                Tipo = Metricas.Tipos.EstadoEntradaDigital_2,
                Valor = EstadoEntradaDigital_2,
                Unidade = Metricas.UnidadeMedidas.Boolean,
            };
            yield return new Metrica
            {
                Porta = "B3",
                Tipo = Metricas.Tipos.EstadoEntradaDigital_3,
                Valor = EstadoEntradaDigital_3,
                Unidade = Metricas.UnidadeMedidas.Boolean,
            };
            yield return new Metrica
            {
                Porta = "B4",
                Tipo = Metricas.Tipos.EstadoEntradaDigital_4,
                Valor = EstadoEntradaDigital_4,
                Unidade = Metricas.UnidadeMedidas.Boolean,
            };
        }
    }
}
