namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class SincronizarAutomacao : Command
    {
        internal bool Interno { get; init; } = false;

        /// <summary>
        /// Se informado, somente o controlador especificado realizará a sincronização.
        /// Caso contrário, todos os controladores, Master e Slave, realizarão a sincronização.
        /// Após a sincronização, a aplicação poderá ser reiniciada automaticamente para aplicar a nova configuração.
        /// </summary>
        public Guid? ControladorId { get; init; }
    }
}
