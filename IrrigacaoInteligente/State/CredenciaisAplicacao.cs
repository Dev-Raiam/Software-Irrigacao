namespace IrrigacaoInteligente.State
{
    public class CredenciaisAplicacao
    {
        public Guid ContaId { get; private set; }
        public Guid PainelId { get; private set; }
        public string? IntegracaoChave { get; private set; }
        public string? IntegracaoSegredo { get; private set; }
        public Guid IntegracaoContextoId { get; private set; }
        public bool Invalida =>
            ContaId == Guid.Empty
            || PainelId == Guid.Empty
            || IntegracaoChave is null
            || IntegracaoSegredo is null
            || IntegracaoContextoId == Guid.Empty;

        public void AdicionarConta(Guid contaId)
        {
            ContaId = contaId;
        }

        public void AdicionarPainel(Guid painelId)
        {
            PainelId = painelId;
        }

        public void AdicionarIntegracao(
            string integracaoChave,
            string integracaoSegredo,
            Guid integracaoContextoId
        )
        {
            IntegracaoChave = integracaoChave;
            IntegracaoSegredo = integracaoSegredo;
            IntegracaoContextoId = integracaoContextoId;
        }

        public void Atualizar(
            Guid contaId,
            Guid painelId,
            string integracaoChave,
            string integracaoSegredo,
            Guid integracaoContextoId
        )
        {
            ContaId = contaId;
            PainelId = painelId;
            IntegracaoChave = integracaoChave;
            IntegracaoSegredo = integracaoSegredo;
            IntegracaoContextoId = integracaoContextoId;
        }

        public void AtualizarConta(Guid contaId)
        {
            ContaId = contaId;
        }

        public void AtualizarPainel(Guid painelId)
        {
            PainelId = painelId;
        }

        public void AtualizarIntegracao(
            string integracaoChave,
            string integracaoSegredo,
            Guid integracaoContextoId
        )
        {
            IntegracaoChave = integracaoChave;
            IntegracaoSegredo = integracaoSegredo;
            IntegracaoContextoId = integracaoContextoId;
        }
    }
}
