namespace Company.SDK.EventBus.Abstractions.Managers;

public sealed class SubscriptionInfo(Type eventType, Type handlerType, string? topic)
{

    public Type EventType { get; set; } = eventType ?? throw new ArgumentNullException(nameof(eventType));

    public Type HandlerType { get; set; } = handlerType ?? throw new ArgumentNullException(nameof(handlerType));

    public string Topic { get; set; } = topic ?? string.Empty;
}
