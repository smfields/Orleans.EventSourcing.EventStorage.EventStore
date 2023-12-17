using System.Diagnostics;
using EventStore.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.EventSourcing.EventStorage.EventStore.Options;
using Orleans.Runtime;
using Orleans.Storage;

namespace Orleans.EventSourcing.EventStorage.EventStore;

/// <summary>
/// Event storage provider that stores events using an EventStoreDB event stream.
/// </summary>
[DebuggerDisplay("EventStore:{" + nameof(_name) + "}")]
public class EventStoreEventStorage : IEventStorage, ILifecycleParticipant<ISiloLifecycle>
{
    private readonly string _name;
    private readonly string _serviceId;
    private readonly EventStoreOptions _options;
    private readonly ILogger<EventStoreEventStorage> _logger;
    private readonly IGrainStorageSerializer _storageSerializer;
    private EventStoreClient? _eventStoreClient;

    public EventStoreEventStorage(
        string name,
        EventStoreOptions options,
        ILogger<EventStoreEventStorage> logger,
        IGrainStorageSerializer defaultStorageSerializer,
        IOptions<ClusterOptions> clusterOptions
    )
    {
        _name = name;
        _options = options;
        _logger = logger;
        _storageSerializer = _options.GrainStorageSerializer ?? defaultStorageSerializer;
        _serviceId = clusterOptions.Value.ServiceId;
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
        
        var results = _eventStoreClient!.ReadStreamAsync(
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
        
        var eventsToAppend = events.Select(
            x => new EventData(Uuid.NewUuid(), x.GetType().FullName!, _storageSerializer.Serialize(x))
        );
        
        StreamRevision expectedRevision = expectedVersion == 0 ? StreamRevision.None : (ulong)expectedVersion - 1;

        try
        {
            await _eventStoreClient!.AppendToStreamAsync(
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

    public void Participate(ISiloLifecycle lifecycle)
    {
        var name = OptionFormattingUtilities.Name<EventStoreEventStorage>(_name);
        lifecycle.Subscribe(name, _options.InitStage, Init, Close);
    }

    private async Task Init(CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();

        try
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "EventStoreEventStorage {Name} is initializing: ServiceId={ServiceId}",
                    _name,
                    _serviceId
                );
            }

            _eventStoreClient = await _options.CreateClient(_options.ClientSettings);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                timer.Stop();
                _logger.LogDebug(
                    "Init: Name={Name} ServiceId={ServiceId}, initialized in {ElapsedMilliseconds} ms",
                    _name,
                    _serviceId,
                    timer.Elapsed.TotalMilliseconds.ToString("0.00")
                );
            }
        }
        catch (Exception ex)
        {
            timer.Stop();
            _logger.LogError(
                ex,
                "Init: Name={Name} ServiceId={ServiceId}, errored in {ElapsedMilliseconds} ms.",
                _name,
                _serviceId,
                timer.Elapsed.TotalMilliseconds.ToString("0.00")
            );

            throw new EventStoreEventStorageException($"{ex.GetType()}: {ex.Message}");
        }
    }

    private async Task Close(CancellationToken cancellationToken)
    {
        if (_eventStoreClient is null) return;

        await _eventStoreClient.DisposeAsync();
    }
}