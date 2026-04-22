using System.ComponentModel.DataAnnotations;

namespace WorkerService.Presentation.DTOs
{
    public class Configuracao
    {
        [Required(ErrorMessage = "ContaId é obrigatório")]
        public Guid ContaId { get; init; }

        [Required(ErrorMessage = "TopicoConfiguracao é obrigatório")]
        public string TopicoConfiguracao { get; init; } = null!;

        [Required(ErrorMessage = "Integracao é obrigatório")]
        public IntegracaoConfiguracao Integracao { get; init; } = null!;

        public class IntegracaoConfiguracao
        {
            [Required(ErrorMessage = "Chave é obrigatório")]
            public string Chave { get; init; } = null!;

            [Required(ErrorMessage = "Segredo é obrigatório")]
            public string Segredo { get; init; } = null!;

            [Required(ErrorMessage = "ContextoId é obrigatório")]
            public Guid ContextoId { get; init; }
        };
    }
}
