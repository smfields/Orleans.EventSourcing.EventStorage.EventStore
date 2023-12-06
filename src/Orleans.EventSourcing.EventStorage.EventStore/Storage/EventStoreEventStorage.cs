using System.Diagnostics;
using EventStore.Client;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;

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
    private readonly IGrainStorageSerializer _storageSerializer;

    public EventStoreEventStorage(
        string name,
        EventStoreOptions options,
        ILogger<EventStoreEventStorage> logger,
        IGrainStorageSerializer defaultStorageSerializer
    )
    {
        _name = name;
        _options = options;
        _logger = logger;
        _storageSerializer = _options.GrainStorageSerializer ?? defaultStorageSerializer;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<EventRecord<TEvent>> ReadEventsFromStorage<TEvent>(
        GrainId grainId, 
        int version = 0, 
        int maxCount = 2147483647
    ) where TEvent : class
    {
        if (version < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(version), "Version cannot be less than 0");
        }

        if (maxCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCount), "Max Count cannot be less than 0");
        }
        
        var client = new EventStoreClient(_options.ClientSettings);

        var results = client.ReadStreamAsync(
            Direction.Forwards,
            grainId.ToString(),
            revision: (ulong) version,
            maxCount: maxCount
        );

        if (await results.ReadState == ReadState.StreamNotFound)
        {
            yield break;
        }
        
        await foreach (var entry in results)
        {
            yield return new EventRecord<TEvent>(_storageSerializer.Deserialize<TEvent>(entry.Event.Data), (int)entry.Event.EventNumber.ToInt64());
        }
    }

    /// <inheritdoc />
    public async Task<bool> AppendEventsToStorage<TEvent>(
        GrainId grainId, 
        IEnumerable<TEvent> events, 
        int expectedVersion
    ) where TEvent : class
    {
        if (expectedVersion < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(expectedVersion), "Expected version cannot be less than 0");
        }
        
        var client = new EventStoreClient(_options.ClientSettings);

        var eventsToAppend = events.Select(
            x => new EventData(Uuid.NewUuid(), x.GetType().FullName!, _storageSerializer.Serialize(x))
        );
        
        StreamRevision expectedRevision = expectedVersion == 0 ? StreamRevision.None : (ulong)expectedVersion - 1;

        try
        {
            await client.AppendToStreamAsync(
                grainId.ToString(),
                expectedRevision: expectedRevision,
                eventsToAppend
            );
        }
        catch (WrongExpectedVersionException)
        {
            return false;
        }
        
        return true;
    }
}