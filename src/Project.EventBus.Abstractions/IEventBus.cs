namespace Project.EventBus.Abstractions;

using Project.EventBus.Abstractions.Events;
using Project.EventBus.Abstractions.Handlers;

/// <summary>
/// Interface principal do barramento de eventos.
/// Define métodos para publicação e assinatura de eventos.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publica um evento de forma assíncrona.
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event, string? topico) where TEvent : EventBase;

    /// <summary>
    /// Publica um evento de forma síncrona.
    /// </summary>
    void Publish<TEvent>(TEvent @event, string? topico) where TEvent : EventBase;

    /// <summary>
    /// Realiza a inscrição assíncrona de um handler para um tipo de evento.
    /// </summary>
    Task SubscribeAsync<THandler, TEvent>(int maxMessages = 10, string? topico = default, CancellationToken ct = default)
        where THandler : IEventHandler<TEvent>
        where TEvent : EventBase;

    /// <summary>
    /// Realiza a inscrição síncrona de um handler para um tipo de evento.
    /// </summary>
    void Subscribe<THandler, TEvent>(int maxMessages = 10, string? topico = default, CancellationToken ct = default)
        where THandler : IEventHandler<TEvent>
        where TEvent : EventBase;
}
