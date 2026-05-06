using System.ComponentModel.DataAnnotations;
using Toolbox.Core.Messages;

namespace IrrigacaoInteligente.Features.Credenciais;

public class AtualizarCredenciais : Command
{
    [Required(ErrorMessage = "ContaId é obrigatório")]
    public Guid ContaId { get; init; }

    [Required(ErrorMessage = "PainelId é obrigatório")]
    public Guid PainelId { get; init; }
}
