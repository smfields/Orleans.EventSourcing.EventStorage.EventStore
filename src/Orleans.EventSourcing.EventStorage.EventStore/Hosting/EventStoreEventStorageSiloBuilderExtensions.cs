using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.EventSourcing.EventStorage;
using Orleans.EventSourcing.EventStorage.EventStore;

// ReSharper disable once CheckNamespace
namespace Orleans.Hosting;

public static class EventStoreEventStorageSiloBuilderExtensions
{
    /// <summary>
    /// Configure silo to use event store as the default event storage.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="configureOptions">The configuration delegate.</param>
    /// <returns>The silo builder.</returns>
    public static ISiloBuilder AddEventStoreEventStorageAsDefault(
        this ISiloBuilder builder,
        Action<EventStoreOptions> configureOptions
    )
    {
        return builder.AddEventStoreEventStorageAsDefault(ob => ob.Configure(configureOptions));
    }

    /// <summary>
    /// Configure silo to use event store as the default event storage.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="configureOptions">The configuration delegate.</param>
    /// <returns>The silo builder.</returns>
    public static ISiloBuilder AddEventStoreEventStorageAsDefault(
        this ISiloBuilder builder,
        Action<OptionsBuilder<EventStoreOptions>>? configureOptions = null
    )
    {
        return builder.AddEventStoreEventStorage(
            EventStorageConstants.DEFAULT_EVENT_STORAGE_PROVIDER_NAME,
            configureOptions
        );
    }

    /// <summary>
    /// Configure silo to use event store as the default event storage.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="name">The name of the event storage provider. This must match the <c>ProviderName</c> property specified when injecting state into a grain.</param>
    /// <param name="configureOptions">The configuration delegate.</param>
    /// <returns>The silo builder.</returns>
    public static ISiloBuilder AddEventStoreEventStorage(
        this ISiloBuilder builder,
        string name,
        Action<EventStoreOptions> configureOptions
    )
    {
        return builder.AddEventStoreEventStorage(
            name,
            ob => ob.Configure(configureOptions)
        );
    }

    /// <summary>
    /// Configure the silo to use event store for event storage
    /// </summary>
    /// <param name="builder">The silo builder</param>
    /// <param name="name">The name of the event storage provider. This must match the <c>ProviderName</c> property specified when injecting state into a grain.</param>
    /// <param name="configureOptions">The configuration delegate</param>
    /// <returns>The silo builder</returns>
    public static ISiloBuilder AddEventStoreEventStorage(
        this ISiloBuilder builder,
        string name,
        Action<OptionsBuilder<EventStoreOptions>>? configureOptions = null
    )
    {
        return builder.ConfigureServices(services =>
        {
            configureOptions?.Invoke(services.AddOptions<EventStoreOptions>(name));
            services.ConfigureNamedOptionForLogging<EventStoreOptions>(name);

            const string defaultProviderName = EventStorageConstants.DEFAULT_EVENT_STORAGE_PROVIDER_NAME;
            if (string.Equals(name, defaultProviderName, StringComparison.Ordinal))
            {
                services.TryAddSingleton(
                    sp => sp.GetRequiredKeyedService<IEventStorage>(defaultProviderName)
                );
            }

            services.AddKeyedSingleton(name, EventStoreStorageFactory.Create);
        });
    }
}