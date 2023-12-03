using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;

namespace Orleans.EventSourcing.EventStorage.EventStore;

/// <summary>
/// Factory for <see cref="EventStoreEventStorage"/>
/// </summary>
public static class EventStoreStorageFactory
{
    public static IEventStorage Create(IServiceProvider services, object? key)
    {
        var name = (string)key!;

        return ActivatorUtilities.CreateInstance<EventStoreEventStorage>(
            services,
            services.GetRequiredService<IOptionsMonitor<EventStoreOptions>>().Get(name),
            name
        );
    }
}