using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Runtime;

namespace Orleans.EventSourcing.EventStorage.EventStore;

/// <summary>
/// Event storage provider that stores events using an EventStoreDB event stream.
/// </summary>
[DebuggerDisplay("EventStore:{" + nameof(_name) + "}")]
public class EventStoreEventStorage : IEventStorage
{
    private readonly string _name;
    private readonly EventStoreOptions _options;
    private readonly ILogger<EventStoreEventStorage> _logger;

    public EventStoreEventStorage(
        string name,
        EventStoreOptions options,
        ILogger<EventStoreEventStorage> logger
    )
    {
        _name = name;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<EventRecord<TEvent>> ReadEventsFromStorage<TEvent>(
        GrainId grainId, 
        int version = 0, 
        int maxCount = 2147483647
    ) where TEvent : class
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<bool> AppendEventsToStorage<TEvent>(
        GrainId grainId, 
        IEnumerable<TEvent> events, 
        int expectedVersion
    ) where TEvent : class
    {
        throw new NotImplementedException();
    }
}