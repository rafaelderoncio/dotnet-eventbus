using Company.SDK.EventBus.Abstractions.Events;
using Company.SDK.EventBus.Abstractions.Handlers;

namespace Company.SDK.EventBus.Abstractions.Managers;

/// <summary>
/// Gerenciador de assinaturas em memória.
/// Armazena relacionamentos entre eventos e seus respectivos manipuladores.
/// </summary>
public sealed class InMemorySubscriptionManager : ISubscriptionManager
{
    private readonly Dictionary<string, List<SubscriptionInfo>> _subscriptions = new();

    /// <inheritdoc/>
    public bool IsEmpty() => !_subscriptions.Any();

    /// <inheritdoc/>
    public void Clear() => _subscriptions.Clear();

    /// <inheritdoc/>
    public bool HasSubscription<TEvent>(string topic = null) where TEvent : Event
    {
        return _subscriptions.ContainsKey(topic ?? typeof(TEvent).Name) &&
               _subscriptions[topic ?? typeof(TEvent).Name].Any();
    }

    /// <inheritdoc/>
    public IEnumerable<SubscriptionInfo> GetHandlersForEvent<TEvent>(string topic = null) where TEvent : Event
    {
        if (!_subscriptions.ContainsKey(topic ?? typeof(TEvent).Name))
            return new List<SubscriptionInfo>();

        return _subscriptions[topic ?? typeof(TEvent).Name];
    }

    /// <inheritdoc/>
    public void AddSubscription<TEvent, THandler>(string topic = null) 
        where TEvent : Event 
        where THandler : IEventHandler<TEvent>
    {
        if (HasSubscription<TEvent>(topic))
            return;

        SubscriptionInfo subscription = new(typeof(TEvent), typeof(THandler), topic);
        _subscriptions[topic ?? typeof(TEvent).Name] = new() { subscription };
    }

    /// <inheritdoc/>
    public void RemoveSubscription<TEvent, THandler>(string topic = null) 
        where TEvent : Event 
        where THandler : IEventHandler<TEvent>
    {
        if (!_subscriptions.ContainsKey(topic ?? typeof(TEvent).Name))
            return;

        SubscriptionInfo subscription = _subscriptions[topic ?? typeof(TEvent).Name]
            .FirstOrDefault(s => s.HandlerType == typeof(THandler));

        if (subscription == null)
            return;

        _subscriptions[topic ?? typeof(TEvent).Name].Remove(subscription);
    }
}
