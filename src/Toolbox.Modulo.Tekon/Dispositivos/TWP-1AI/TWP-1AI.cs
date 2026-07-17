using Toolbox.Modulo.Tekon.Interfaces;
using Toolbox.Modulo.Tekon.Models;

namespace Toolbox.Modulo.Tekon.Dispositivos
{
    public class TWP_1AI : ITekonDispositivoDado
    {
        public long NumeroSerie { get; init; }
        public string Modelo { get; init; } = null!;
        public int RSSI { get; init; }
        public int PeriodoComunicacao { get; init; }
        public int TempoDecorrido { get; init; }
        public float TensaoAlimentacao { get; init; }
        public float TemperaturaInterna { get; init; }
        public float ValorEntradaAnalogica_1 { get; init; }
        public int VersaoFirmware { get; init; }
        public int RevisaoVersao { get; init; }
        public int VersaoHardware { get; init; }

        long? ITekonDispositivoDado.NumeroSerie => NumeroSerie;

        public IEnumerable<Metrica> ObterMetricas()
        {
            yield return new Metrica
            {
                Tipo = TekonConstants.Metricas.Tipos.TemperaturaInterna,
                Valor = TemperaturaInterna,
                Unidade = TekonConstants.Metricas.UnidadeMedidas.Celsius
            };
            
            yield return new Metrica
            {
                Tipo = TekonConstants.Metricas.Tipos.TensaoAlimentacao,
                Valor = TensaoAlimentacao,
                Unidade = TekonConstants.Metricas.UnidadeMedidas.Volts,
            };

            yield return new Metrica
            {
                Tipo = TekonConstants.Metricas.Tipos.RSSI,
                Valor = RSSI,
                Unidade = TekonConstants.Metricas.UnidadeMedidas.Decibeis,
            };
        }
    }
}
