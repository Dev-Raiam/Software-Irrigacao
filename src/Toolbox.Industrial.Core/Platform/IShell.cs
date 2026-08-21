namespace Toolbox.Industrial.Core.Platform
{
    //Interface
    public interface IShell
    {
        Task<(string output, string error, int exitCode)> Run(string command);
        Task<bool> Stop(string serviceName, TimeSpan? timeout = null);
        Task<bool> Start(string serviceName, TimeSpan? timeout = null);
        Task<string?> Status(string serviceName, TimeSpan? timeout = null);
    }
}
