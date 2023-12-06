using Testcontainers.EventStoreDb;

// ReSharper disable once CheckNamespace
namespace Orleans;

[SetUpFixture]
public class EventStoreDbSetup
{
    public static string ConnectionString { get; private set; } = null!;
    
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var container = new EventStoreDbBuilder().Build();
        await container.StartAsync();

        ConnectionString = container.GetConnectionString();
    }
}