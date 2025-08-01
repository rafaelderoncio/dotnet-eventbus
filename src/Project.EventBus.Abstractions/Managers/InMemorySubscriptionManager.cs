using System;
using Project.EventBus.Abstractions.Events;
using Project.EventBus.Abstractions.Handlers;

namespace Project.EventBus.Abstractions.Managers;

/// <summary>
/// Implementação em memória do gerenciador de assinaturas.
/// Armazena os handlers registrados em um dicionário por tipo de evento ou tópico.
/// </summary>
public sealed class InMemorySubscriptionManager : ISubscriptionManager
{
    private readonly Dictionary<string, List<SubscriptionInfo>> _subscriptions = new();

    public bool IsEmpty() => !_subscriptions.Any();

    public void Clear() => _subscriptions.Clear();

    public bool HasSubscription<TEvent>(string? topic) where TEvent : EventBase
    {
        return _subscriptions.ContainsKey(topic ?? typeof(TEvent).Name) &&
               _subscriptions[topic ?? typeof(TEvent).Name].Any();
    }

    public IEnumerable<SubscriptionInfo> GetHandlersForEvent<TEvent>(string? topic) where TEvent : EventBase
    {
        if (!_subscriptions.ContainsKey(topic ?? typeof(TEvent).Name))
            return new List<SubscriptionInfo> { };

        return _subscriptions[topic ?? typeof(TEvent).Name];
    }

    public void AddSubscription<TEvent, THandler>(string? topic) where TEvent : EventBase where THandler : IEventHandler<TEvent>
    {
        if (HasSubscription<TEvent>(topic))
            return;

        SubscriptionInfo subscription = new(typeof(TEvent), typeof(THandler), topic);

        _subscriptions[topic ?? typeof(TEvent).Name] = new() { subscription };
    }

    public void RemoveSubscription<TEvent, THandler>(string? topic) where TEvent : EventBase where THandler : IEventHandler<TEvent>
    {
        if (!_subscriptions.ContainsKey(topic ?? typeof(TEvent).Name))
            return;

        SubscriptionInfo? subscription = _subscriptions[topic ?? typeof(TEvent).Name].FirstOrDefault(s => s.HandlerType == typeof(THandler));

        if (subscription == null)
            return;

        _subscriptions[topic ?? typeof(TEvent).Name].Remove(subscription);
    }
}
