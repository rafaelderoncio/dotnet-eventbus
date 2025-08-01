using Project.EventBus.Abstractions.Events;

namespace Project.EventBus.Abstractions.Handlers;

public interface IEventHandler<TEvent> where TEvent : EventBase
{
    Task Handle(TEvent @event);
}
