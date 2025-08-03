using Company.SDK.EventBus.Abstractions.Events;
using Company.SDK.EventBus.Abstractions.Handlers;

namespace Company.SDK.EventBus.Abstractions.Managers;

public interface ISubscriptionManager
{

    bool IsEmpty();

    void Clear();

    bool HasSubscription<TEvent>(string topic = null) where TEvent : Event;

    IEnumerable<SubscriptionInfo> GetHandlersForEvent<TEvent>(string topic = null) where TEvent : Event;

    void AddSubscription<TEvent, THandler>(string topic = null)
        where TEvent : Event
        where THandler : IEventHandler<TEvent>;

    void RemoveSubscription<TEvent, THandler>(string topic = null)
        where TEvent : Event
        where THandler : IEventHandler<TEvent>;
}
