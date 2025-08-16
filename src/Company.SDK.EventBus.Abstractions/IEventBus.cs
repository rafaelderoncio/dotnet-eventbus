using Company.SDK.EventBus.Abstractions.Events;
using Company.SDK.EventBus.Abstractions.Handlers;

namespace Company.SDK.EventBus.Abstractions;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, string topic = null, CancellationToken cancellationToken = default) where TEvent : Event;

    void Publish<TEvent>(TEvent @event, string topic = null, CancellationToken cancellationToken = default) where TEvent : Event;

    Task SubscribeAsync<THandler, TEvent>(int maxMessages = 10, string topic = null, CancellationToken cancellationToken = default)
        where THandler : IEventHandler<TEvent>
        where TEvent : Event;

    void Subscribe<THandler, TEvent>(int maxMessages = 10, string topic = null, CancellationToken cancellationToken = default)
        where THandler : IEventHandler<TEvent>
        where TEvent : Event;
}
