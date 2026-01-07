using System.Collections.Concurrent;

namespace TdA26_CyganStudios.Services;

public sealed class SseConnectionManager
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<long, SseConnection>> _courseConnections = new();

    private long _nextId;

    public long AddCourseConnection(Guid courseId, HttpResponse response, CancellationToken connectionCancellationToken)
    {
        var connections = _courseConnections.GetOrAdd(courseId, static key => new ConcurrentDictionary<long, SseConnection>());

        var id = Interlocked.Increment(ref _nextId);
        connections[id] = new SseConnection(response, connectionCancellationToken);
        return id;
    }

    public void RemoveCourseConnection(Guid courseId, long id)
    {
        if (_courseConnections.TryGetValue(courseId, out var connections))
        {
            connections.TryRemove(id, out _);
        }
    }

    public async Task SendPingAsync(Guid courseId, long connectionId)
    {
        if (!_courseConnections.TryGetValue(courseId, out var connections))
        {
            return; // no connected clients
        }

        if (connections.TryGetValue(connectionId, out var connection))
        {
            await connection.SendEventAsync("ping", $"{{ date: \"{DateTime.UtcNow}\" }}");
        }
    }

    public async Task BroadcastCourseAsync(Guid courseId, string eventName, string data, CancellationToken cancellationToken = default)
    {
        if (!_courseConnections.TryGetValue(courseId, out var connections) || connections.IsEmpty)
        {
            return; // no connected clients
        }

        foreach (var kvp in connections)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await kvp.Value.SendEventAsync(eventName, data);
            }
            catch
            {
                RemoveCourseConnection(courseId, kvp.Key); // remove dead/disconnected clients
            }
        }
    }

    private readonly struct SseConnection
    {
        public readonly HttpResponse Response { get; }
        public readonly CancellationToken CancellationToken { get; }

        public SseConnection(HttpResponse response, CancellationToken cancellationToken)
        {
            Response = response;
            CancellationToken = cancellationToken;
        }

        public readonly async Task SendEventAsync(string eventName, string data)
        {
            CancellationToken.ThrowIfCancellationRequested();

            var sseMessage = $"event: {eventName}\n" +
                             $"data: {data}\n\n";

            await Response.WriteAsync(sseMessage);
            await Response.Body.FlushAsync();
        }
    }
}