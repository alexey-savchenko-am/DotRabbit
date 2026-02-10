using AutoFixture;
using DotRabbit.Core.Settings.Entities;
using DotRabbit.IntegrationTests.EventsAndHandlers;
using DotRabbit.IntegrationTests.Fixtures;
using DotRabbit.IntegrationTests.Tools;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Tracing;
using Xunit;
using Xunit.Abstractions;

namespace DotRabbit.IntegrationTests;

public class RmqPublishAndProcessEventsIntegrationTests : IAsyncLifetime
{
    private readonly RabbitMqFixture _rabbit = new();
    private readonly TestServer _server;
    private readonly ITestOutputHelper _output;

    public RmqPublishAndProcessEventsIntegrationTests(ITestOutputHelper output)
    {
        _server = new TestServer(output);
        _output = output;
    }

    public async Task InitializeAsync()
    {
        await _rabbit.StartAsync();
        await _server.StartAsync(_rabbit.ConnectionString);
    }

    public async Task DisposeAsync()
    {
        await _rabbit.DisposeAsync();
        await _server.DisposeAsync();
    }

    [Fact]
    public async Task PublishEventToRmq_ProcessSuccesfully_WhenEventIsCorrect()
    {
        var userCreatedEvent = _server.Fixture.Create<UserCreatedTestEvent>();

        await _server.EventPublisher.PublishAsync(new DomainDefinition("users"), userCreatedEvent);

        var @event = await _server.UserCreatedEventSignal.WaitAsync(TimeSpan.FromSeconds(5));

        @event.Event.Name.Should().Be(userCreatedEvent.Name);
        @event.Event.UserId.Should().Be(userCreatedEvent.UserId);
        @event.Event.CreatedOnUtc.Should().Be(userCreatedEvent.CreatedOnUtc);
    }

    [Fact]
    public async Task PublishFailedEventToRmq_TriesToBeProcessed5Times_WhenEventHandlerThrowsException()
    {
        const int retryCount = 5;

        var counter = _server.Services.GetRequiredService<EventProcessingCounter>();
        counter.Reset(retryCount);

        var generation = counter.Generation;

        await _server.EventPublisher.PublishAsync(
            new DomainDefinition("users"),
            _server.Fixture.Create<UserUpdatedFailedEvent>()
        );

        var elapsedMs = await counter.WaitAsync(TimeSpan.FromSeconds(60));

        counter.Remaining.Should().Be(0);

        _output.WriteLine(
            "Message processing repeated {0} times in {1}ms",
            retryCount,
            elapsedMs
        );
    }

    [Fact]
    public async Task PublishMultipleEventsToRmq_ProcessSuccesfully_WhenEventsAreCorrect()
    {
        var eventCount = 10_000;
        var counter = _server.Services.GetRequiredService<EventProcessingCounter>();
        counter.Reset(eventCount);


        var tasks = Enumerable.Range(0, eventCount)
            .Select(_ =>
            {
                var evt = _server.Fixture.Create<UserCreatedTestEvent>();
                return _server.EventPublisher.PublishAsync(new DomainDefinition("users"), evt);
            });

        await Task.WhenAll(tasks);

        var elapsedMs = await counter.WaitAsync(TimeSpan.FromSeconds(20));

        _output.WriteLine(
            "{0} events have been processed in {1}ms",
            eventCount,
            elapsedMs
        );
    }

}
