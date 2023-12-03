using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Orleans.EventSourcing.EventStorage;
using Orleans.EventSourcing.EventStorage.EventStore;
using Orleans.TestingHost;

// ReSharper disable once CheckNamespace
namespace Orleans.Hosting;

public class EventStoreEventStorageSiloBuilderExtensionsTests
{
    private TestCluster Cluster { get; set; } = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var builder = new TestClusterBuilder();

        builder.AddSiloBuilderConfigurator<TestSiloConfigurator>();

        Cluster = builder.Build();
        await Cluster.DeployAsync();
    }

    [Test]
    public void EventStore_storage_can_be_registered_as_default()
    {
        var silo = Cluster.Primary as InProcessSiloHandle;
        var eventStorage = silo!.SiloHost.Services.GetRequiredService<IEventStorage>();
        Assert.That(eventStorage, Is.TypeOf<EventStoreEventStorage>());
    }

    [Test]
    public void EventStore_storage_can_be_registered_by_name()
    {
        var silo = Cluster.Primary as InProcessSiloHandle;
        var eventStorage = silo!.SiloHost.Services.GetRequiredKeyedService<IEventStorage>("EventStoreEventStorage");
        Assert.That(eventStorage, Is.TypeOf<EventStoreEventStorage>());
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await Cluster.StopAllSilosAsync();
    }

    private class TestSiloConfigurator : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.AddEventStoreEventStorage("EventStoreEventStorage");
            siloBuilder.AddEventStoreEventStorageAsDefault();
        }
    }
}