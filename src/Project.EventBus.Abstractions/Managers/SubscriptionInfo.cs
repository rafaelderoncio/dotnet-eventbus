namespace Project.EventBus.Abstractions.Managers;

/// <summary>
/// Representa as informações de uma assinatura de evento,
/// incluindo o tipo do evento, o tipo do handler e o tópico.
/// </summary>
/// <param name="eventType">Tipo do evento associado à assinatura.</param>
/// <param name="handlerType">Tipo do manipulador associado.</param>
/// <param name="topic">Tópico opcional.</param>
public sealed class SubscriptionInfo(Type eventType, Type handlerType, string? topic)
{
    /// <summary>
    /// Tipo do evento.
    /// </summary>
    public Type EventType { get; set; } = eventType ?? throw new ArgumentNullException(nameof(eventType));

    /// <summary>
    /// Tipo do handler (manipulador) do evento.
    /// </summary>
    public Type HandlerType { get; set; } = handlerType ?? throw new ArgumentNullException(nameof(handlerType));

    /// <summary>
    /// Nome do tópico associado ao evento.
    /// </summary>
    public string Topic { get; set; } = topic ?? string.Empty;
}
