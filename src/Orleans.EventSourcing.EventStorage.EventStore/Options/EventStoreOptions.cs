using EventStore.Client;
using Orleans.Storage;

// ReSharper disable once CheckNamespace
namespace Orleans.Configuration;

public class EventStoreOptions : IStorageProviderSerializerOptions
{
    /// <summary>
    /// Settings used to create the <see cref="EventStoreClient"/>
    /// </summary>
    public EventStoreClientSettings ClientSettings { get; set; } = null!;
    
    /// <summary>
    /// Stage of silo lifecycle where storage should be initialized.  Storage must be initialized prior to use.
    /// </summary>
    public int InitStage { get; set; } = ServiceLifecycleStage.ApplicationServices;
    
    /// <inheritdoc/>
    public IGrainStorageSerializer? GrainStorageSerializer { get; set; }

    /// <summary>
    /// The delegate used to create the <see cref="EventStoreClient"/>
    /// </summary>
    public Func<EventStoreClientSettings, Task<EventStoreClient>> CreateClient { get; set; } = DefaultCreateClient;

    /// <summary>
    /// Default delegate used to create the <see cref="EventStoreClient"/>
    /// </summary>
    /// <param name="settings">EventStoreClientSettings</param>
    /// <returns>The <see cref="EventStoreClient"/></returns>
    public static Task<EventStoreClient> DefaultCreateClient(EventStoreClientSettings settings) =>
        Task.FromResult(new EventStoreClient(settings));
}