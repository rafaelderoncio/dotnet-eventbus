namespace Project.EventBus.Abstractions.Handlers;

using Project.EventBus.Abstractions.Events;

/// <summary>
/// Interface para manipuladores de eventos.
/// Cada tipo de evento deve ter um manipulador que implemente essa interface.
/// </summary>
/// <typeparam name="TEvent">Tipo do evento que será manipulado.</typeparam>
public interface IEventHandler<TEvent> where TEvent : EventBase
{
    /// <summary>
    /// Manipula um evento recebido.
    /// </summary>
    /// <param name="event">Instância do evento.</param>
    Task Handle(TEvent @event);
}
