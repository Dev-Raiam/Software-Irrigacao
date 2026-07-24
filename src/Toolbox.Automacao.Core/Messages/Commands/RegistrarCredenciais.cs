using System.ComponentModel.DataAnnotations;
using Toolbox.Core.Messages;

namespace Toolbox.Automacao.Core.Messages.Commands
{
    internal class RegistrarCredenciais : Command
    {
        [Required(ErrorMessage = "Chave é obrigatória")]
        public string Chave { get; init; } = null!;

        [Required(ErrorMessage = "Segredo é obrigatório")]
        public string Segredo { get; init; } = null!;

        [Required(ErrorMessage = "ContextoId é obrigatório")]
        public Guid ContextoId { get; init; }

        [Required(ErrorMessage = "PainelId é obrigatório")]
        public Guid PainelId { get; init; }
    }

}
