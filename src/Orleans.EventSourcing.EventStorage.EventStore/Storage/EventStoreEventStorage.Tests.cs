using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Orleans.Runtime;
using Orleans.TestingHost;

namespace Orleans.EventSourcing.EventStorage.EventStore;

public class EventStoreEventStorageTests
{
    private TestCluster Cluster { get; set; } = null!;

    private EventStoreEventStorage EventStoreEventStorage
    {
        get
        {
            var silo = Cluster.Primary as InProcessSiloHandle;
            var eventStore = silo!.SiloHost.Services.GetRequiredService<IEventStorage>();
            return (EventStoreEventStorage)eventStore;
        }
    }

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var builder = new TestClusterBuilder();

        builder.AddSiloBuilderConfigurator<TestSiloConfigurator>();

        Cluster = builder.Build();
        await Cluster.DeployAsync();
    }

    [Test]
    public async Task Events_can_be_stored_and_retrieved()
    {
        var grainId = GenerateGrainId();

        var sampleEvent = new SampleEvent(100);
        await EventStoreEventStorage.AppendEventsToStorage(grainId, new[] { sampleEvent }, 0);
        var eventStream = EventStoreEventStorage.ReadEventsFromStorage<SampleEvent>(grainId, 0, 1);
        var eventList = await eventStream.ToListAsync();

        Assert.That(eventList.First().Data, Is.EqualTo(sampleEvent));
    }

    [Test]
    public async Task Retrieved_events_have_same_type_as_stored_events()
    {
        var grainId = GenerateGrainId();

        await EventStoreEventStorage.AppendEventsToStorage(grainId, new[] { new SampleEvent() }, 0);
        var eventStream = EventStoreEventStorage.ReadEventsFromStorage<object>(grainId, 0, 1);
        var eventList = await eventStream.ToListAsync();

        Assert.That(eventList.First().Data, Is.TypeOf<SampleEvent>());
    }

    [Test]
    public async Task Events_are_not_appended_if_the_expected_version_does_not_match()
    {
        var grainId = GenerateGrainId();

        var result = await EventStoreEventStorage.AppendEventsToStorage(grainId, new[] { new SampleEvent() }, 10);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task Trying_to_read_from_a_version_past_the_last_version_returns_an_empty_list()
    {
        var grainId = GenerateGrainId();
        await EventStoreEventStorage.AppendEventsToStorage(grainId, new[] { new SampleEvent() }, 0);

        var eventStream = EventStoreEventStorage.ReadEventsFromStorage<SampleEvent>(grainId, 10);
        var eventList = await eventStream.ToListAsync();

        Assert.That(eventList, Is.Empty);
    }

    [Test]
    public async Task Reading_before_any_events_are_appended_returns_an_empty_list()
    {
        var grainId = GenerateGrainId();

        var eventStream = EventStoreEventStorage.ReadEventsFromStorage<SampleEvent>(grainId);
        var eventList = await eventStream.ToListAsync();

        Assert.That(eventList, Is.Empty);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await Cluster.StopAllSilosAsync();
    }

    private GrainId GenerateGrainId() => GrainId.Create(nameof(SampleGrain), Guid.NewGuid().ToString());

    private class SampleGrain;

    private record SampleEvent(int Value = 0);

    private class TestSiloConfigurator : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.AddEventStoreEventStorage("EventStoreStorage", opts =>
            {
                opts.ClientSettings = EventStoreClientSettings.Create(EventStoreDbSetup.ConnectionString);
            });
            siloBuilder.AddEventStoreEventStorageAsDefault(opts =>
            {
                opts.ClientSettings = EventStoreClientSettings.Create(EventStoreDbSetup.ConnectionString);
            });
        }
    }
}