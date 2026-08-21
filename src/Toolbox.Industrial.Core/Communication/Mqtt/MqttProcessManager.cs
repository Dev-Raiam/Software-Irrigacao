using System.Collections.Concurrent;
using Newtonsoft.Json;
using Toolbox.Industrial.Core.Messages.Integration.Events;

namespace Toolbox.Industrial.Core.Communication.Mqtt;

public sealed class MqttProcessManager
{
    private readonly ConcurrentDictionary<Guid, IPendingProcess> _pendings = new();
    public IReadOnlyDictionary<Guid, IPendingProcess> Pendings => _pendings;

    public bool Add(IPendingProcess process)
    {
        return _pendings.TryAdd(process.Id, process);
    }

    public bool Completed(Guid processId, ResponseRequest response)
    {
        Console.WriteLine($"Processo completado [{processId}] =>");
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
    Guid Id { get; init; }
    void Completed(ResponseRequest response);
}

public sealed class PendingProcess<TContent> : IPendingProcess
{
    public required Guid Id { get; init; }
    public required string Topic { get; init; }
    public required string BrokerKey { get; init; }
    public required TContent Content { get; init; }
    public required TaskCompletionSource<ResponseRequest> Completion { get; init; }

    public void Completed(ResponseRequest response)
    {
        Completion.TrySetResult(response);
    }
}
