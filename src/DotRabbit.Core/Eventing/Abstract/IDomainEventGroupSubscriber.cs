namespace DotRabbit.Core.Eventing.Abstract;

internal interface IDomainEventGroupSubscriber
{
    Task<bool> SubscribeAsync(CancellationToken ct = default);
    Task UnsubscribeAsync(CancellationToken ct = default);
    Task HealthCheckAsync(CancellationToken ct = default);
}
