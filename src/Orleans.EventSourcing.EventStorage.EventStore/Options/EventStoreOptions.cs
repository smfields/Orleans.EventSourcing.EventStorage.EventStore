using EventStore.Client;
using Orleans.Storage;

// ReSharper disable once CheckNamespace
namespace Orleans.Configuration;

public class EventStoreOptions : IStorageProviderSerializerOptions
{
    public EventStoreClientSettings ClientSettings { get; set; } = null!;
    
    /// <inheritdoc/>
    public IGrainStorageSerializer? GrainStorageSerializer { get; set; }
}