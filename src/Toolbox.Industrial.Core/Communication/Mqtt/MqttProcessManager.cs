using Newtonsoft.Json;
using System.Collections.Concurrent;
using Toolbox.Industrial.Core.Messages.Integration.Events;

namespace Toolbox.Industrial.Core.Communication.Mqtt;

public sealed class MqttProcessManager
{
    private readonly ConcurrentDictionary<string, IPendingProcess> _pendings = new();
    public IReadOnlyDictionary<string, IPendingProcess> Pendings => _pendings;

    public bool Add(IPendingProcess process)
    {
        return _pendings.TryAdd(process.Id, process);
    }

    public bool Completed(string processId, ResponseRequest response)
    {
        Console.WriteLine(
            $"Processo completado [{processId}] =>"
        );
        var result = _pendings.TryRemove(processId, out var process);
        if (result)
        {
            Console.WriteLine($"{JsonConvert.SerializeObject(response, Formatting.Indented)}");
            process!.Completed(response);
        }
        return result;
    }
}

public interface IPendingProcess
{
    string Id { get; init; }
    void Completed(ResponseRequest response);
}

public sealed class PendingProcess<TContent> : IPendingProcess
{
    public required string Id { get; init; }
    public required string Topic { get; init; }
    public required string BrokerKey { get; init; }
    public required TContent Content { get; init; }
    public required TaskCompletionSource<ResponseRequest> Completion { get; init; }

    public void Completed(ResponseRequest response)
    {
        Completion.TrySetResult(response);
    }
}
