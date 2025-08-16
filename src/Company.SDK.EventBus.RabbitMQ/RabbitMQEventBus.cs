using System.Text;
using System.Text.Json;
using Autofac;
using Company.SDK.EventBus.Abstractions;
using Company.SDK.EventBus.Abstractions.Events;
using Company.SDK.EventBus.Abstractions.Handlers;
using Company.SDK.EventBus.Abstractions.Managers;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Company.SDK.EventBus.RabbitMQ;

public sealed class RabbitMQEventBus(
    IRabbitMQConnection connection,
    ISubscriptionManager subscriptionManager,
    ILogger<IEventBus> logger,
    ILifetimeScope lifetimeScope,
    string scopeName = "rabbitmq_event_bus"
) : IEventBus
{
    private readonly IRabbitMQConnection _connection = connection
        ?? throw new ArgumentNullException(nameof(connection), "Connection cannot be null.");
    private readonly ISubscriptionManager _subscriptionManager = subscriptionManager
        ?? throw new ArgumentNullException(nameof(subscriptionManager), "Subscription manager cannot be null.");
    private readonly ILogger<IEventBus> _logger = logger
        ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
    private readonly ILifetimeScope _lifetimeScope = lifetimeScope
        ?? throw new ArgumentNullException(nameof(lifetimeScope), "Lifetime scope cannot be null.");
    private readonly string _scopeName = scopeName
        ?? throw new ArgumentNullException(nameof(scopeName), "Scope name cannot be null.");
    private readonly Dictionary<string, (IConnection Connection, IChannel Channel)> _activeConsumers = new();

    public void Publish<TEvent>(TEvent @event, string topic = null, CancellationToken cancellation = default) where TEvent : Event
    {
        throw new NotImplementedException("Use PublishAsync instead for asynchronous operations.");
    }

    public async Task PublishAsync<TEvent>(TEvent @event, string topic = null, CancellationToken cancellation = default) where TEvent : Event
    {
        try
        {
            _logger.LogInformation("Starting async publishing of event {Event}", typeof(TEvent).Name);

            string eventName = GetEventName<TEvent>(topic);
            var eventTag = $"{eventName}_{RabbitMQConnectionType.Publisher}".ToLower();

            using IConnection connection = await _connection.OpenConnection<TEvent>(name: eventTag, topic: eventName, cancellationToken: cancellation);
            using IChannel channel = await connection.CreateChannelAsync(cancellationToken: cancellation);

            await QueueDeclare(channel, eventName);

            await BasicPublish(channel, eventName, @event);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error async publishing event {Event}", typeof(TEvent).Name);
            _logger.LogError(ex, ex.Message + "\n" + ex.StackTrace);

            throw;
        }
        finally
        {
            _logger.LogInformation("Finalizing async publication of event {Event}", typeof(TEvent).Name);
        }
    }

    public void Subscribe<THandler, TEvent>(int maxMessages = 10, string topic = null, CancellationToken cancellation = default)
        where THandler : IEventHandler<TEvent>
        where TEvent : Event
    {
        throw new NotImplementedException("Use SubscribeAsync instead for asynchronous operations.");
    }

    public async Task SubscribeAsync<THandler, TEvent>(int maxMessages = 10, string topic = null, CancellationToken cancellation = default)
        where THandler : IEventHandler<TEvent>
        where TEvent : Event
    {
        try
        {
            _logger.LogInformation("Starting async subscribe of event {Event}", typeof(TEvent).Name);

            var eventName = GetEventName<TEvent>(topic);
            var eventTag = $"{eventName}_{RabbitMQConnectionType.Consumer}".ToLower();

            if (!_subscriptionManager.HasSubscription<TEvent>(eventName))
                _subscriptionManager.AddSubscription<TEvent, THandler>(eventName);

            var connection = await _connection.OpenConnection<TEvent>(name: eventTag, topic: eventName, cancellationToken: cancellation);
            var channel = await connection.CreateChannelAsync(cancellationToken: cancellation);

            await channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: (ushort)maxMessages, 
                global: false,
                cancellationToken: cancellation
            );

            await QueueDeclare(channel, eventName);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (obj, args) => await ReceiveMessagesAsync<THandler, TEvent>(args, channel, cancellation);

            await channel.BasicConsumeAsync(
                queue: eventName,
                autoAck: false,
                consumerTag: eventTag,
                consumer, cancellation
            );
        }
        catch (Exception ex)
        {
            _logger.LogError("Error async subscribe event {Event}", typeof(TEvent).Name);
            _logger.LogError(ex, ex.Message + "\n" + ex.StackTrace);

            throw;
        }
        finally
        {
            _logger.LogInformation("Finalizing async subscribe of event {Event}", typeof(TEvent).Name);
        }
    }

    private async Task ReceiveMessagesAsync<THandler, TEvent>(BasicDeliverEventArgs args, IChannel channel, CancellationToken cancellationToken) where TEvent : Event
    {
        var eventName = args.RoutingKey;
        var messageId = args.BasicProperties.MessageId;

        try
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            _logger.LogInformation("Starting topic consumption {Topic}, MessageId: {MessageId}", eventName, messageId);

            string message = Encoding.UTF8.GetString(args.Body.ToArray());

            await ProcessMessage<TEvent>(message, eventName, cancellationToken);

            await channel.BasicAckAsync(
                deliveryTag: args.DeliveryTag,
                multiple: false,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("Successfully processed and deleted message {MessageId} from topic {Topic}", messageId, eventName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message {MessageId} from topic {Topic}. Will retry later.", messageId, eventName);
            throw;
        }
    }

    private async Task ProcessMessage<TEvent>(string message, string eventName, CancellationToken cancellationToken) where TEvent : Event
    {
        if (!_subscriptionManager.HasSubscription<TEvent>(eventName))
            return;

        using ILifetimeScope scope = _lifetimeScope.BeginLifetimeScope(_scopeName);

        foreach (SubscriptionInfo subscription in _subscriptionManager.GetHandlersForEvent<TEvent>(eventName))
        {
            TEvent @event = JsonSerializer.Deserialize<TEvent>(message);

            IEventHandler<TEvent> handler = GetHandler<TEvent>(scope, subscription);

            if (handler == null) continue;

            await handler.Handle(@event, cancellationToken);
        }

        scope.Dispose();
    }

    private static IEventHandler<TEvent> GetHandler<TEvent>(ILifetimeScope scope, SubscriptionInfo subscription) where TEvent : Event
    {
        object resolved = ResolutionExtensions.ResolveOptional((IComponentContext)(object)scope, subscription.HandlerType);

        return resolved is not null ? (IEventHandler<TEvent>)scope.Resolve(subscription.HandlerType) : null;
    }

    private static string GetEventName<TEvent>(string topic) where TEvent : Event
    {
        return topic ?? typeof(TEvent).Name.Replace("Event", "");
    }

    private static async Task BasicPublish<TEvent>(IChannel channel, string eventName, TEvent @event) where TEvent : Event
    {
        byte[] body = Encoding.UTF8.GetBytes(s: JsonSerializer.Serialize(@event));
        BasicProperties basicProperties = new()
        {
            Persistent = true,
            MessageId = @event.EventId,
            ContentType = "application/json"
        };

        await channel.BasicPublishAsync(
            exchange: string.Empty,             // Usa a exchange padrão (direct)
            mandatory: true,                    // Roteia somente para uma fila
            routingKey: eventName,              // Nome da fila (routing key)
            basicProperties: basicProperties,   // Propriedades adicionais (ex: persistência)
            body: body                          // Corpo da mensagem (bytes)
        );
    }

    private static async Task QueueDeclare(IChannel channel, string eventName)
    {
        await channel.QueueDeclareAsync(
            queue: eventName,    // Nome da fila
            durable: false,      // Se true, a fila persiste após reiniciar o servidor
            exclusive: false,    // Se true, só esta conexão pode usá-la
            autoDelete: false,   // Se true, a fila é deletada quando não há consumidores
            arguments: null      // Argumentos adicionais
        );
    }
}
