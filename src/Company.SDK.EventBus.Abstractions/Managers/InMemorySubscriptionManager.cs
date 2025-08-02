using Company.SDK.EventBus.Abstractions.Events;
using Company.SDK.EventBus.Abstractions.Handlers;

namespace Company.SDK.EventBus.Abstractions.Managers;

public sealed class InMemorySubscriptionManager : ISubscriptionManager
{
    private readonly Dictionary<string, List<SubscriptionInfo>> _subscriptions = new();

    public bool IsEmpty() => !_subscriptions.Any();

    public void Clear() => _subscriptions.Clear();

    public bool HasSubscription<TEvent>(string? topic) where TEvent : Event
    {
        return _subscriptions.ContainsKey(topic ?? typeof(TEvent).Name) &&
               _subscriptions[topic ?? typeof(TEvent).Name].Any();
    }

    public IEnumerable<SubscriptionInfo> GetHandlersForEvent<TEvent>(string? topic) where TEvent : Event
    {
        if (!_subscriptions.ContainsKey(topic ?? typeof(TEvent).Name))
            return new List<SubscriptionInfo> { };

        return _subscriptions[topic ?? typeof(TEvent).Name];
    }

    public void AddSubscription<TEvent, THandler>(string? topic) where TEvent : Event where THandler : IEventHandler<TEvent>
    {
        if (HasSubscription<TEvent>(topic))
            return;

        SubscriptionInfo subscription = new(typeof(TEvent), typeof(THandler), topic);

        _subscriptions[topic ?? typeof(TEvent).Name] = new() { subscription };
    }

    public void RemoveSubscription<TEvent, THandler>(string? topic) where TEvent : Event where THandler : IEventHandler<TEvent>
    {
        if (!_subscriptions.ContainsKey(topic ?? typeof(TEvent).Name))
            return;

        SubscriptionInfo? subscription = _subscriptions[topic ?? typeof(TEvent).Name].FirstOrDefault(s => s.HandlerType == typeof(THandler));

        if (subscription == null)
            return;

        _subscriptions[topic ?? typeof(TEvent).Name].Remove(subscription);
    }
}
