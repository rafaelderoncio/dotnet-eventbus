using Company.SDK.EventBus.Abstractions.Events;
using Company.SDK.EventBus.Abstractions.Handlers;

namespace Company.SDK.EventBus.Abstractions.Managers;

/// <summary>
/// Contrato para gerenciadores de assinaturas de eventos.
/// Controla a relação entre tipos de eventos e manipuladores registrados.
/// </summary>
public interface ISubscriptionManager
{
    /// <summary>
    /// Retorna se não existem assinaturas registradas.
    /// </summary>
    bool IsEmpty();

    /// <summary>
    /// Remove todas as assinaturas registradas.
    /// </summary>
    void Clear();

    /// <summary>
    /// Verifica se há pelo menos um manipulador registrado para um determinado evento.
    /// </summary>
    /// <typeparam name="TEvent">Tipo do evento.</typeparam>
    /// <param name="topic">Tópico ou canal opcional. Se não informado, o nome do tipo do evento é usado.</param>
    bool HasSubscription<TEvent>(string topic = null) where TEvent : Event;

    /// <summary>
    /// Retorna todos os manipuladores associados a um evento.
    /// </summary>
    /// <typeparam name="TEvent">Tipo do evento.</typeparam>
    /// <param name="topic">Tópico ou canal opcional. Se não informado, o nome do tipo do evento é usado.</param>
    IEnumerable<SubscriptionInfo> GetHandlersForEvent<TEvent>(string topic = null) where TEvent : Event;

    /// <summary>
    /// Registra um manipulador para um evento específico.
    /// </summary>
    /// <typeparam name="TEvent">Tipo do evento.</typeparam>
    /// <typeparam name="THandler">Tipo do manipulador.</typeparam>
    /// <param name="topic">Tópico ou canal opcional.</param>
    void AddSubscription<TEvent, THandler>(string topic = null)
        where TEvent : Event
        where THandler : IEventHandler<TEvent>;

    /// <summary>
    /// Remove um manipulador previamente registrado para um evento.
    /// </summary>
    /// <typeparam name="TEvent">Tipo do evento.</typeparam>
    /// <typeparam name="THandler">Tipo do manipulador.</typeparam>
    /// <param name="topic">Tópico ou canal opcional.</param>
    void RemoveSubscription<TEvent, THandler>(string topic = null)
        where TEvent : Event
        where THandler : IEventHandler<TEvent>;
}
