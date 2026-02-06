
using AutoFixture;
using DotRabbit.Core.Configuration.Extensions;
using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Settings.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace DotRabbit.IntegrationTests;

public sealed class TestServerHarness : IAsyncDisposable
{
    private IHost? _host;
    public IServiceProvider Services =>
        _host?.Services ?? throw new InvalidOperationException();
    public Fixture Fixture { get; } = new Fixture();

    public IEventPublisher EventPublisher { get; private set; } = null!;
    public EventProcessingSignal<IEventContainer<UserCreatedTestEvent>> UserCreatedEventSignal { get; private set; } = null!;
    private readonly ITestOutputHelper _output;

    public TestServerHarness(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task StartAsync(string rmqConnectionString)
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();

                logging.AddProvider(
                    new XunitLoggerProvider(_output, LogLevel.Warning)
                );

                logging.SetMinimumLevel(LogLevel.Warning);
            })
            .ConfigureServices(services =>
            {
                services.AddLogging(builder => builder.AddConsole());

                services.AddSingleton<EventProcessingSignal<IEventContainer<UserCreatedTestEvent>>>();
                services.AddSingleton<EventProcessingCounter>();

                services.AddScoped<UserCreatedTestEventHandler>();

                services.AddRmqTransport(
                    serviceName: "TestService", 
                    config => config.FromConnectionString(rmqConnectionString)
                );

                services.AddEventSubscriber(
                    domain: new DomainDefinition("users"),
                    processor =>
                        processor.SubscribeOn<UserCreatedTestEvent, UserCreatedTestEventHandler>()
                );

            })
            .Build();

        
        await _host.StartAsync();

        EventPublisher = _host.Services.GetRequiredService<IEventPublisher>();
        UserCreatedEventSignal = _host.Services.GetRequiredService<EventProcessingSignal<IEventContainer<UserCreatedTestEvent>>>();
    }

    public async Task StopAsync()
    {
        if (_host is null)
            return;

        await _host.StopAsync();
        _host.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}


public sealed class XunitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _output;
    private readonly LogLevel _minLevel;

    public XunitLoggerProvider(
        ITestOutputHelper output,
        LogLevel minLevel = LogLevel.Warning)
    {
        _output = output;
        _minLevel = minLevel;
    }

    public ILogger CreateLogger(string categoryName)
        => new XunitLogger(_output, categoryName, _minLevel);

    public void Dispose() { }
}

public sealed class XunitLogger : ILogger
{
    private readonly ITestOutputHelper _output;
    private readonly string _category;
    private readonly LogLevel _minLevel;

    public XunitLogger(
        ITestOutputHelper output,
        string category,
        LogLevel minLevel)
    {
        _output = output;
        _category = category;
        _minLevel = minLevel;
    }

    public IDisposable BeginScope<TState>(TState state)
        => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel)
        => logLevel >= _minLevel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        _output.WriteLine(
            $"[{logLevel}] {_category}: {formatter(state, exception)}");

        if (exception != null)
        {
            _output.WriteLine(exception.ToString());
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}
