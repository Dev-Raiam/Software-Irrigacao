using Toolbox.Modulo.Tekon.Interfaces;
using Toolbox.Modulo.Tekon.Models;

namespace Toolbox.Modulo.Tekon.Dispositivos
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

        public string Modelo => TekonConstants.Modelos.WGW420;
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
                Tipo = TekonConstants.Metricas.Tipos.Analogica_1,
                Valor = Analogica_1.ValorCorrenteAtual,
                Unidade = TekonConstants.Metricas.UnidadeMedidas.MiliAmper,
            };

            yield return new Metrica
            {
                Porta = "A2",
                Tipo = TekonConstants.Metricas.Tipos.Analogica_2,
                Valor = Analogica_2.ValorCorrenteAtual,
                Unidade = TekonConstants.Metricas.UnidadeMedidas.MiliAmper,
            };

            yield return new Metrica
            {
                Porta = "A3",
                Tipo = TekonConstants.Metricas.Tipos.Analogica_3,
                Valor = Analogica_3.ValorCorrenteAtual,
                Unidade = TekonConstants.Metricas.UnidadeMedidas.MiliAmper,
            };

            yield return new Metrica
            {
                Porta = "A4",
                Tipo = TekonConstants.Metricas.Tipos.Analogica_4,
                Valor = Analogica_4.ValorCorrenteAtual,
                Unidade = TekonConstants.Metricas.UnidadeMedidas.MiliAmper,
            };

            yield return new Metrica
            {
                Porta = "A5",
                Tipo = TekonConstants.Metricas.Tipos.Analogica_5,
                Valor = Analogica_5.ValorCorrenteAtual,
                Unidade = TekonConstants.Metricas.UnidadeMedidas.MiliAmper,
            };

            yield return new Metrica
            {
                Porta = "A6",
                Tipo = TekonConstants.Metricas.Tipos.Analogica_6,
                Valor = Analogica_6.ValorCorrenteAtual,
                Unidade = TekonConstants.Metricas.UnidadeMedidas.MiliAmper,
            };

            yield return new Metrica
            {
                Porta = "A7",
                Tipo = TekonConstants.Metricas.Tipos.Analogica_7,
                Valor = Analogica_7.ValorCorrenteAtual,
                Unidade = TekonConstants.Metricas.UnidadeMedidas.MiliAmper,
            };

            yield return new Metrica
            {
                Porta = "A8",
                Tipo = TekonConstants.Metricas.Tipos.Analogica_8,
                Valor = Analogica_8.ValorCorrenteAtual,
                Unidade = TekonConstants.Metricas.UnidadeMedidas.MiliAmper,
            };
        }
    }
}
