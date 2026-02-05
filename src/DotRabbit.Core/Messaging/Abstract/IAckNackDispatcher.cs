namespace DotRabbit.Core.Messaging.Abstract;

/// <summary>
/// Represents a queue-based consumer used to ACK/NACK incoming messages.
/// Instances are created by <see cref="Eventing.RmqEventConsumer"/> per <see cref="RmqMessageConsumer"/>
/// and should not be registered in DI.
/// </summary>
public interface IAckNackDispatcher
{
    void Start();
    Task StopAsync();
}
