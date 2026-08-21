namespace Toolbox.Industrial.Core.Platform
{
    public enum ServiceStatus
    {
        Running,
        Stopped,
        Starting,
        Stopping,
        Failed,
        Unknown,
    }

    public interface IShell
    {
        Task<bool> StopService(string serviceName, CancellationToken cancellationToken);
        Task<bool> StartService(string serviceName, CancellationToken cancellationToken);
        Task<ServiceStatus> StatusService(string serviceName, CancellationToken cancellationToken);
    }

    public static class ShellExtensions
    {
        public static async Task<bool> WaitForStatus(
            this IShell shell,
            string serviceName,
            ServiceStatus expected,
            TimeSpan timeout,
            CancellationToken cancellationToken
        )
        {
            var maxTimer = DateTime.Now + timeout;

            while (DateTime.Now < maxTimer)
            {
                var status = await shell.StatusService(serviceName, cancellationToken);

                if (status == expected)
                    return true;

                await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
            }

            return false;
        }
    }
}
