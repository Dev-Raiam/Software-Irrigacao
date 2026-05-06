using System.ComponentModel.DataAnnotations;
using Toolbox.Core.Messages;

namespace IrrigacaoInteligente.Features.Credenciais;

public class AdicionarCredenciais : Command
{
    [Required(ErrorMessage = "ContaId é obrigatório")]
    public Guid ContaId { get; init; }

    [Required(ErrorMessage = "PainelId é obrigatório")]
    public Guid PainelId { get; init; }

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
