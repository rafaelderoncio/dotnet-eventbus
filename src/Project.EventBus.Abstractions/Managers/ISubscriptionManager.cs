namespace Project.EventBus.Abstractions.Managers;

using Project.EventBus.Abstractions.Events;
using Project.EventBus.Abstractions.Handlers;

/// <summary>
/// Interface para gerenciamento de assinaturas de eventos.
/// Permite adicionar, remover e consultar handlers registrados.
/// </summary>
public interface ISubscriptionManager
{
    /// <summary>
    /// Verifica se não há assinaturas registradas.
    /// </summary>
    bool IsEmpty();

    /// <summary>
    /// Remove todas as assinaturas registradas.
    /// </summary>
    void Clear();

    /// <summary>
    /// Verifica se há assinaturas para um evento específico.
    /// </summary>
    bool HasSubscription<TEvent>(string? topic) where TEvent : EventBase;

    /// <summary>
    /// Retorna todos os handlers associados a um tipo de evento.
    /// </summary>
    IEnumerable<SubscriptionInfo> GetHandlersForEvent<TEvent>(string? topic) where TEvent : EventBase;

    /// <summary>
    /// Adiciona uma nova assinatura de handler para um tipo de evento.
    /// </summary>
    void AddSubscription<TEvent, THandler>(string? topic)
        where TEvent : EventBase
        where THandler : IEventHandler<TEvent>;

    /// <summary>
    /// Remove uma assinatura específica.
    /// </summary>
    void RemoveSubscription<TEvent, THandler>(string? topic)
        where TEvent : EventBase
        where THandler : IEventHandler<TEvent>;
}
