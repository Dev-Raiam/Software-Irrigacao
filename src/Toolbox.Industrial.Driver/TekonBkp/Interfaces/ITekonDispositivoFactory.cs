namespace Toolbox.Industrial.Driver.TekonBkp.Interfaces
{
    public interface ITekonDispositivoFactory
    {
        ITekonDispositivoPerfil CriarModelo(string modelo);
    }
}