namespace Company.SDK.EventBus.Abstractions.Managers;

/// <summary>
/// Representa as informações de uma assinatura entre um evento e seu manipulador.
/// </summary>
public sealed class SubscriptionInfo(Type eventType, Type handlerType, string topic = null)
{
    /// <summary>
    /// Tipo do evento associado.
    /// </summary>
    public Type EventType { get; set; } = eventType ?? throw new ArgumentNullException(nameof(eventType));

    /// <summary>
    /// Tipo do manipulador associado ao evento.
    /// </summary>
    public Type HandlerType { get; set; } = handlerType ?? throw new ArgumentNullException(nameof(handlerType));

    /// <summary>
    /// Nome do tópico/canal usado na assinatura. Pode ser vazio se não for usado.
    /// </summary>
    public string Topic { get; set; } = topic ?? string.Empty;
}
