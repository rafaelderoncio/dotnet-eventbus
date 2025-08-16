using Company.SDK.EventBus.Abstractions.Events;

namespace Company.SDK.EventBus.Abstractions.Handlers;

/// <summary>
/// Define um manipulador para eventos do tipo <typeparamref name="TEvent"/>.
/// </summary>
/// <typeparam name="TEvent">Tipo do evento a ser processado.</typeparam>
public interface IEventHandler<TEvent> where TEvent : Event
{
    /// <summary>
    /// Executa a lógica de processamento de um evento recebido.
    /// </summary>
    /// <param name="event">Instância do evento a ser processado.</param>
    /// <param name="cancellation">Token de cancelamento usado para interromper a execução.</param>
    Task Handle(TEvent @event, CancellationToken cancellation = default);
}
