using System.IO.Pipes;
using System.Text;
using System.Text.Json;

namespace Toolbox.Industrial.Core.Communication.RaspIO;

public sealed record IoRequest(Guid Id, string Command, int Pin, object? Value);

public sealed record IoResponse(Guid Id, bool Success, object? Value, string? Error);

public interface IControllerIO
{
    Task<IoResponse> ReadDigitalAsync(int pin, CancellationToken ct = default);

    Task<IoResponse> WriteDigitalAsync(int pin, bool value, CancellationToken ct = default);

    Task<IoResponse> ReadAnalogAsync(int pin, CancellationToken ct = default);

    Task<IoResponse> WriteAnalogAsync(int pin, double value, CancellationToken ct = default);

    //Task<PinSnapshot> ReadAllAsync(CancellationToken ct = default);
}

internal sealed class PythonIoController : IControllerIO
{
    private readonly NamedPipeClientStream _client;

    public PythonIoController()
    {
        _client = new NamedPipeClientStream(
            serverName:".",
            pipeName:"meu_pipe_comunicacao",
            direction: PipeDirection.InOut,
            options: PipeOptions.Asynchronous);

        _client.Connect();
    }

    private async Task<IoResponse> SendAsync<T>(IoRequest request, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(request);

        var bytes = Encoding.UTF8.GetBytes(json + "\n");

        //var write = new StreamWriter();

        await _client.WriteAsync(bytes, ct);

        var buffer = new byte[4096];

        var count = await _client.ReadAsync(buffer, 0, buffer.Length, ct);

        var responseJson = Encoding.UTF8.GetString(buffer, 0, count);
        var response = JsonSerializer.Deserialize<IoResponse>(responseJson);

        if (response == null || !response.Success)
            throw new IOException(response?.Error ?? "Unknown error");

        return response;
    }

    public Task<IoResponse> ReadAnalogAsync(int pin, CancellationToken ct = default)
    {
        var request = new IoRequest(Guid.NewGuid(), "ReadAnalog", pin, null);

        return SendAsync<IoResponse>(request, ct);
    }

    public Task<IoResponse> ReadDigitalAsync(int pin, CancellationToken ct)
    {
        var request = new IoRequest(Guid.NewGuid(), "ReadDigital", pin, null);

        return SendAsync<IoResponse>(request, ct);
    }

    public Task<IoResponse> WriteAnalogAsync(int pin, double value, CancellationToken ct = default)
    {
        var request = new IoRequest(Guid.NewGuid(), "WriteAnalog", pin, value);

        return SendAsync<IoResponse>(request, ct);
    }

    public Task<IoResponse> WriteDigitalAsync(int pin, bool value, CancellationToken ct = default)
    {
        var request = new IoRequest(Guid.NewGuid(), "WriteDigital", pin, value);

        return SendAsync<IoResponse>(request, ct);
    }
}
