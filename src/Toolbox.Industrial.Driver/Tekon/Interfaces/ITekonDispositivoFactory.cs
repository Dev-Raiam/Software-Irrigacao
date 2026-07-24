namespace Toolbox.Industrial.Driver.Tekon.Interfaces
{
    public interface ITekonDispositivoFactory
    {
        ITekonDispositivoPerfil CriarModelo(string modelo);
    }
}