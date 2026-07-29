using Toolbox.Industrial.Driver.TekonBkp.Interfaces;
using Toolbox.Industrial.Driver.TekonBkp.Models;

namespace Toolbox.Industrial.Driver.TekonBkp.Dispositivos
{
    public class TWP_2UT : ITekonDispositivoDado
    {
        public long NumeroSerie { get; init; }
        public string Modelo { get; init; } = null!;
        public int RSSI { get; init; }
        public int PeriodoComunicacao { get; init; }
        public int TempoDecorrido { get; init; }
        public float TensaoAlimentacao { get; init; }
        public float TemperaturaInterna { get; init; }
        public float TemperaturaExterna_1 { get; init; }
        public float TemperaturaExterna_2 { get; init; }
        public int VersaoFirmware { get; init; }
        public int RevisaoVersao { get; init; }
        public int VersaoHardware { get; init; }

        long? ITekonDispositivoDado.NumeroSerie => NumeroSerie;

        public IEnumerable<Metrica> ObterMetricas()
        {
            yield return new Metrica
            {
                Tipo = Metricas.Tipos.TemperaturaInterna,
                Valor = TemperaturaInterna,
                Unidade = Metricas.UnidadeMedidas.Celsius,
            };

            yield return new Metrica
            {
                Tipo = Metricas.Tipos.TemperaturaExterna,
                Valor = TemperaturaExterna_1,
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
        }
    }
}
