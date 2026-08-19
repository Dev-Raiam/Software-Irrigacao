namespace Toolbox.Industrial.Core.Platform
{
    public interface IShell
    {
        Task<(string output, string error, int exitCode)> Run(string command);
    }
}
