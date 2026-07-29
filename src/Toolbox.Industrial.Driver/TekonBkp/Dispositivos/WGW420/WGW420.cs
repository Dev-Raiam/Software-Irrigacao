using Toolbox.Industrial.Driver.TekonBkp.Interfaces;
using Toolbox.Industrial.Driver.TekonBkp.Models;

namespace Toolbox.Industrial.Driver.TekonBkp.Dispositivos
{
    public class WGW420 : ITekonDispositivoDado
    {
        public Analogica Analogica_1 { get; init; } = null!;
        public Analogica Analogica_2 { get; init; } = null!;
        public Analogica Analogica_3 { get; init; } = null!;
        public Analogica Analogica_4 { get; init; } = null!;
        public Analogica Analogica_5 { get; init; } = null!;
        public Analogica Analogica_6 { get; init; } = null!;
        public Analogica Analogica_7 { get; init; } = null!;
        public Analogica Analogica_8 { get; init; } = null!;

        public string Modelo => Modelos.WGW420;
        public long? NumeroSerie => null;

        public class Analogica
        {
            public float ValorMinimo { get; init; }
            public float ValorMaximo { get; init; }
            public int DesvioSaida { get; init; }
            public int NumeroTentativas { get; init; }
            public int LinkEnderecoModbus { get; init; }
            public float ValorCorrenteAtual { get; init; }

            public Analogica(
                float valorMinimo,
                float valorMaximo,
                int desvioSaida,
                int numeroTentativas,
                int linkEnderecoModbus,
                float valorCorrenteAtual
            )
            {
                ValorMinimo = valorMinimo;
                ValorMaximo = valorMaximo;
                DesvioSaida = desvioSaida;
                NumeroTentativas = numeroTentativas;
                LinkEnderecoModbus = linkEnderecoModbus;
                ValorCorrenteAtual = valorCorrenteAtual;
            }
        }

        public IEnumerable<Metrica> ObterMetricas()
        {
            yield return new Metrica
            {
                Porta = "A1",
                Tipo = Metricas.Tipos.Analogica_1,
                Valor = Analogica_1.ValorCorrenteAtual,
                Unidade = Metricas.UnidadeMedidas.MiliAmper,
            };

            yield return new Metrica
            {
                Porta = "A2",
                Tipo = Metricas.Tipos.Analogica_2,
                Valor = Analogica_2.ValorCorrenteAtual,
                Unidade = Metricas.UnidadeMedidas.MiliAmper,
            };

            yield return new Metrica
            {
                Porta = "A3",
                Tipo = Metricas.Tipos.Analogica_3,
                Valor = Analogica_3.ValorCorrenteAtual,
                Unidade = Metricas.UnidadeMedidas.MiliAmper,
            };

            yield return new Metrica
            {
                Porta = "A4",
                Tipo = Metricas.Tipos.Analogica_4,
                Valor = Analogica_4.ValorCorrenteAtual,
                Unidade = Metricas.UnidadeMedidas.MiliAmper,
            };

            yield return new Metrica
            {
                Porta = "A5",
                Tipo = Metricas.Tipos.Analogica_5,
                Valor = Analogica_5.ValorCorrenteAtual,
                Unidade = Metricas.UnidadeMedidas.MiliAmper,
            };

            yield return new Metrica
            {
                Porta = "A6",
                Tipo = Metricas.Tipos.Analogica_6,
                Valor = Analogica_6.ValorCorrenteAtual,
                Unidade = Metricas.UnidadeMedidas.MiliAmper,
            };

            yield return new Metrica
            {
                Porta = "A7",
                Tipo = Metricas.Tipos.Analogica_7,
                Valor = Analogica_7.ValorCorrenteAtual,
                Unidade = Metricas.UnidadeMedidas.MiliAmper,
            };

            yield return new Metrica
            {
                Porta = "A8",
                Tipo = Metricas.Tipos.Analogica_8,
                Valor = Analogica_8.ValorCorrenteAtual,
                Unidade = Metricas.UnidadeMedidas.MiliAmper,
            };
        }
    }
}
