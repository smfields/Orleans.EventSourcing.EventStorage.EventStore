using Orleans.Runtime;

// ReSharper disable once CheckNamespace
namespace Orleans.Configuration;

public class EventStoreOptionsValidator : IConfigurationValidator
{
    private readonly EventStoreOptions _options;
    private readonly string _name;

    public EventStoreOptionsValidator(EventStoreOptions options, string name)
    {
        _options = options;
        _name = name;
    }

    /// <inheritdoc />
    public void ValidateConfiguration()
    {
        if (_options.ClientSettings is null)
        {
            throw new OrleansConfigurationException(
                $"Configuration for EventStore event storage provider {_name} is invalid. {nameof(_options.ClientSettings)} must be configured."
            );
        }
    }
}