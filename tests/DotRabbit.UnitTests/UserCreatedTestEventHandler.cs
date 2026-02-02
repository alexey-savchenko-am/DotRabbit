using DotRabbit.Core.Eventing.Abstract;

namespace DotRabbit.UnitTests;

internal class UserCreatedTestEventHandler : IEventHandler<UserCreatedTestEvent>
{
    public Task HandleAsync(IEventContainer<UserCreatedTestEvent> @event)
    {
        Console.WriteLine("Done!");
        return Task.CompletedTask;
    }
}
