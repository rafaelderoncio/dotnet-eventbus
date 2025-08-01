using Project.EventBus.Abstractions.Events;
using Project.EventBus.Abstractions.Handlers;

namespace Project.EventBus.Abstractions;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, string? topico) where TEvent : EventBase;

    void Publish<TEvent>(TEvent @event, string? topico) where TEvent : EventBase;

    Task SubscribeAsync<THandler, TEvent>(int maxMessages = 10, string? topico = default, CancellationToken ct = default)
        where THandler : IEventHandler<TEvent>
        where TEvent : EventBase;

    void Subscribe<THandler, TEvent>(int maxMessages = 10, string? topico = default, CancellationToken ct = default)
        where THandler : IEventHandler<TEvent>
        where TEvent : EventBase;
}