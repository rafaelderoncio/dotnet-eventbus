using Autofac;
using Company.SDK.EventBus.Abstractions;
using Company.SDK.EventBus.Abstractions.Events;
using Company.SDK.EventBus.Abstractions.Handlers;
using Company.SDK.EventBus.Abstractions.Managers;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Company.SDK.EventBus.Kafka;

public sealed class KafkaEventBus(
    IKafkaConnection connection,
    ISubscriptionManager subscriptionManager,
    ILogger<IEventBus> logger,
    ILifetimeScope lifetimeScope,
    string scopeName = "kafka_event_bus"
) : IEventBus
{
    private readonly IKafkaConnection _connection = connection
        ?? throw new ArgumentNullException(nameof(connection), "Connection cannot be null.");
    private readonly ISubscriptionManager _subscriptionManager = subscriptionManager
        ?? throw new ArgumentNullException(nameof(subscriptionManager), "Subscription manager cannot be null.");
    private readonly ILogger<IEventBus> _logger = logger
        ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
    private readonly ILifetimeScope _lifetimeScope = lifetimeScope
        ?? throw new ArgumentNullException(nameof(lifetimeScope), "Lifetime scope cannot be null.");
    private readonly string _scopeName = scopeName
        ?? throw new ArgumentNullException(nameof(scopeName), "Scope name cannot be null.");


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

            Message<string, TEvent> message = new()
            {
                Key = @event.EventId.ToString(),
                Value = @event
            };

            await _connection.ProducerBuilder<TEvent>().ProduceAsync(
                topic: eventName,
                message: message,
                cancellationToken: cancellation
            );
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

            IConsumer<string, TEvent> consumer = _connection.ConsumerBuilder<TEvent>();

            if (!_subscriptionManager.HasSubscription<TEvent>(eventName))
            {
                _subscriptionManager.AddSubscription<TEvent, THandler>(eventName);
                consumer.Subscribe(eventName);
            }

            await Task.Run(
                function: async delegate
                {
                    while (!cancellation.IsCancellationRequested)
                        await ReceiveMessagesAsync<TEvent>(consumer, maxMessages, cancellation, eventName);
                },
                cancellationToken: cancellation
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

    private async Task ReceiveMessagesAsync<TEvent>(IConsumer<string, TEvent> consumer, int maxMessages, CancellationToken cancellation, string topic) where TEvent : Event
    {
        if (cancellation.IsCancellationRequested)
            return;

        string eventName = GetEventName<TEvent>(topic);

        ConsumeResult<string, TEvent> result = consumer.Consume(cancellation);

        try
        {
            _logger.LogInformation("Starting topic consumption {Topic}, MessageId: {MessageId}", eventName, result.Message.Key);

            await ProcessMessage<TEvent>(result.Message, eventName, cancellation);

            consumer.Commit(result);

            _logger.LogInformation("Successfully processed and deleted message {MessageId} from topic {Topic}", result.Message.Key, topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message {MessageId} from topic {Topic}. Will retry later.", result.Message.Key, topic);
            throw;
        }
    }

    private static string GetEventName<TEvent>(string topic) where TEvent : Event
    {
        return topic ?? typeof(TEvent).Name.Replace("Event", "");
    }

    private async Task ProcessMessage<TEvent>(Message<string, TEvent> message, string topic, CancellationToken cancellation) where TEvent : Event
    {
        if (!_subscriptionManager.HasSubscription<TEvent>(topic))
            return;

        using ILifetimeScope scope = _lifetimeScope.BeginLifetimeScope(_scopeName);

        foreach (SubscriptionInfo subscription in _subscriptionManager.GetHandlersForEvent<TEvent>(topic))
        {
            TEvent @event = message.Value;

            IEventHandler<TEvent> handler = GetHandler<TEvent>(scope, subscription);

            if (handler == null) continue;

            await handler.Handle(@event, cancellation);
        }

        scope.Dispose();
    }

    private static IEventHandler<TEvent> GetHandler<TEvent>(ILifetimeScope scope, SubscriptionInfo subscription) where TEvent : Event
    {
        object resolved = ResolutionExtensions.ResolveOptional((IComponentContext)(object)scope, subscription.HandlerType);

        return resolved is not null ? (IEventHandler<TEvent>)scope.Resolve(subscription.HandlerType) : null;
    }
}
