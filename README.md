# DotRabbit

<div align="center">
  <a href="https://github.com/alexey-savchenko-am/DotRabbit">
    <img src="docs/assets/logo-lg.png" width="250" />
  </a>
</div>

**DotRabbit** is a fast, event-driven RabbitMQ client for .NET. 
It is designed to be simple to use, easy to configure, and efficient under load. 
DotRabbit focuses on a clean developer experience while keeping message publishing and consumption lightweight and predictable.

## Table of contents
- [Installation](#installation)
- [Getting Started](#getting-started)
    - [Event Definition](#event-definition)
    - [Event Handler Definition](#event-handler-definition)
    - [Transport Registration](#transport-registration)
    - [Event Subscriber Registration](#event-subscriber-registration)
    - [Event Publishing](#event-publishing)
- [Architecture](#architecture)

## Installation
Install NuGet package via NuGet.org

```sh
$ dotnet add package DotRabbit
```
```powershell
PS> Install-Package DotRabbit
```

## Getting Started
### Event Definition

Create an event by implementing the `IEvent` interface.
Set a specific event name using the EventName property.
`DotRabbit` uses the event name as a RMQ queue name under the hood.

```csharp
using DotRabbit.Core.Eventing.Abstract;

[EventName("user-created")]
public class UserCreatedEvent : IEvent
{
    public string Name { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
```

### Event Handler Definition

Create an event handler to process `UserCreatedEvent`.
Implement the `IEventHandler` interface closed with the specific event type.
The `HandleAsync` method is invoked when the event is received.
The method receives the event instance as a parameter.
**Do not forget to register the event handler in the dependency injection container with a scoped lifetime.**

```csharp
using DotRabbit.Core.Eventing.Abstract;

internal class UserCreatedEventHandler : IEventHandler<UserCreatedTestEvent>
{
    public Task HandleAsync(IEventContainer<UserCreatedEvent> @event)
    {
        Console.WriteLine("We've got an event!");
        return Task.CompletedTask;
    }
}

// In Startup
services.AddScoped<UserCreatedEventHandler>();
```

### Transport Registration

Register the RMQ transport within the `Startup` class to configure the consumer and producer for the bus. 
Use the configuration builder to set up the connection to the RMQ endpoint. 
You can use `RmqConfigurationBuilder` for easier configuration.
Alternatively, you can manually configure the `RmqConnectionConfiguration` object via the `configBuilder` parameter.

```csharp
using DotRabbit.Core.Configuration.Extensions;

var connectionString = "amqp://guest:guest@localhost:{port}";

services.AddRmqTransport(
  serviceName: "UserService", 
  config => config.FromConnectionString(connectionString)
);
```

 ### Event Subscriber Registration

Use the event subscriber registration to bind event handlers to a specific domain.
In this configuration, the **domain acts as an RMQ exchange**. 
All events published within the same domain will be routed through this exchange and processed by the subscribed handlers.

```csharp
using DotRabbit.Core.Configuration.Extensions;

services.AddEventSubscriber(
    domain: new DomainDefinition("users"),
    processor =>
        processor
            .SubscribeOn<UserCreatedEvent, UserCreatedEventHandler>()
            .SubscribeOn<UserUpdatedEvent, UserUpdatedEventHandler>()
);
```

## Architecture

<div align="center">
  <img src="docs/assets/architecture.jpg#gh-light-mode-only" alt="Architecture diagram"  width="60%">
  <img src="docs/assets/architecture-dark.png#gh-dark-mode-only" alt="Architecture diagram"  width="60%">
</div>

