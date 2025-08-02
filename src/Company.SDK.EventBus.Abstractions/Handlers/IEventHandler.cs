using Company.SDK.EventBus.Abstractions.Events;

namespace Company.SDK.EventBus.Abstractions.Handlers;

public interface IEventHandler<TEvent> where TEvent : Event
{
    Task Handle(TEvent @event, CancellationToken cancellation = default);
}
