using Project.EventBus.Abstractions.Events;
using Project.EventBus.Abstractions.Handlers;

namespace Project.EventBus.Abstractions.Managers;

public interface ISubscriptionManager
{
    bool IsEmpty();

    void Clear();

    bool HasSubscription<TEvent>(string? topic)
        where TEvent : EventBase;

    IEnumerable<SubscriptionInfo> GetHandlersForEvent<TEvent>(string? topic)
        where TEvent : EventBase;

    void AddSubscription<TEvent, THandler>(string? topic)
        where TEvent : EventBase
        where THandler : IEventHandler<TEvent>;

    void RemoveSubscription<TEvent, THandler>(string? topic)
        where TEvent : EventBase
        where THandler : IEventHandler<TEvent>;
    
}
