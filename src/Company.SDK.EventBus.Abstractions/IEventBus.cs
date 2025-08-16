using Company.SDK.EventBus.Abstractions.Events;
using Company.SDK.EventBus.Abstractions.Handlers;

namespace Company.SDK.EventBus.Abstractions;

/// <summary>
/// Define o contrato do barramento de eventos.
/// Permite publicar eventos e registrar assinaturas de forma síncrona ou assíncrona.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publica um evento de forma assíncrona para o barramento.
    /// </summary>
    /// <typeparam name="TEvent">Tipo do evento.</typeparam>
    /// <param name="event">Instância do evento a ser publicada.</param>
    /// <param name="topic">Tópico opcional. Caso não informado, será usado o nome do tipo do evento.</param>
    /// <param name="cancellationToken">Token de cancelamento para abortar a operação.</param>
    Task PublishAsync<TEvent>(TEvent @event, string topic = null, CancellationToken cancellationToken = default) where TEvent : Event;

    /// <summary>
    /// Publica um evento de forma síncrona para o barramento.
    /// </summary>
    /// <typeparam name="TEvent">Tipo do evento.</typeparam>
    /// <param name="event">Instância do evento a ser publicada.</param>
    /// <param name="topic">Tópico opcional. Caso não informado, será usado o nome do tipo do evento.</param>
    /// <param name="cancellationToken">Token de cancelamento para abortar a operação.</param>
    void Publish<TEvent>(TEvent @event, string topic = null, CancellationToken cancellationToken = default) where TEvent : Event;

    /// <summary>
    /// Registra uma assinatura assíncrona para receber e processar eventos.
    /// </summary>
    /// <typeparam name="THandler">Tipo do manipulador que processará os eventos.</typeparam>
    /// <typeparam name="TEvent">Tipo do evento a ser manipulado.</typeparam>
    /// <param name="maxMessages">Número máximo de mensagens processadas por vez. Default: 10.</param>
    /// <param name="topic">Tópico opcional. Caso não informado, será usado o nome do tipo do evento.</param>
    /// <param name="cancellationToken">Token de cancelamento para abortar a assinatura.</param>
    Task SubscribeAsync<THandler, TEvent>(int maxMessages = 10, string topic = null, CancellationToken cancellationToken = default)
        where THandler : IEventHandler<TEvent>
        where TEvent : Event;

    /// <summary>
    /// Registra uma assinatura síncrona para receber e processar eventos.
    /// </summary>
    /// <typeparam name="THandler">Tipo do manipulador que processará os eventos.</typeparam>
    /// <typeparam name="TEvent">Tipo do evento a ser manipulado.</typeparam>
    /// <param name="maxMessages">Número máximo de mensagens processadas por vez. Default: 10.</param>
    /// <param name="topic">Tópico opcional. Caso não informado, será usado o nome do tipo do evento.</param>
    /// <param name="cancellationToken">Token de cancelamento para abortar a assinatura.</param>
    void Subscribe<THandler, TEvent>(int maxMessages = 10, string topic = null, CancellationToken cancellationToken = default)
        where THandler : IEventHandler<TEvent>
        where TEvent : Event;
}
