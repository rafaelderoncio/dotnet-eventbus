using Autofac;
using Microsoft.Extensions.Logging;
using Company.SDK.EventBus.Abstractions;
using Company.SDK.EventBus.Abstractions.Events;
using Company.SDK.EventBus.Abstractions.Handlers;
using Company.SDK.EventBus.Abstractions.Managers;
using System.Text.Json;
using Amazon.SQS.Model;

namespace Company.SDK.EventBus.AmazonSimpleQueueService;

public class AmazonSQSEventBus(
    IAmazonSQSConection conection,
    ISubscriptionManager subscriptionManager,
    ILogger<IEventBus> logger,
    ILifetimeScope lifetimeScope,
    string scopeName = "amazon_simple_queue_service_event_bus") : IEventBus
{
    private readonly IAmazonSQSConection _connection = conection
        ?? throw new ArgumentNullException(nameof(conection), "Connection cannot be null.");
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

            await _connection.Client.SendMessageAsync(
                queueUrl: _connection.BaseUrl + eventName,
                messageBody: JsonSerializer.Serialize(@event),
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

            if (!_subscriptionManager.HasSubscription<TEvent>(topic))
                _subscriptionManager.AddSubscription<TEvent, THandler>(topic);

            await Task.Run(
                function: async delegate
                {
                    while (!cancellation.IsCancellationRequested)
                        await ReceiveMessagesAsync<TEvent>(maxMessages, cancellation, topic);
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

    #region private

    private static string GetEventName<TEvent>(string topic) where TEvent : Event
    {
        return topic ?? typeof(TEvent).Name.Replace("Event", "");
    }

    private async Task ReceiveMessagesAsync<TEvent>(int maxMessages, CancellationToken cancellation, string topic = null) where TEvent : Event
    {
        if (cancellation.IsCancellationRequested)
            return;

        string eventName = GetEventName<TEvent>(topic);

        ReceiveMessageRequest receiveMessageRequest = new()
        {
            QueueUrl = _connection.BaseUrl + eventName,
            MaxNumberOfMessages = maxMessages > 10 ? 10 : maxMessages,
            WaitTimeSeconds = 10,
            MessageSystemAttributeNames = ["ApproximateReceiveCount", "SentTimestamp"]
        };

        ReceiveMessageResponse receiveMessageResponse = await _connection.Client.ReceiveMessageAsync(
            request: receiveMessageRequest,
            cancellationToken: cancellation
        );

        List<Message> messages = receiveMessageResponse.Messages ?? new List<Message>();

        foreach (var message in messages)
        {
            try
            {
                _logger.LogInformation("Starting topic consumption {Topic}, MessageId: {MessageId}", eventName, message.MessageId);

                await ProcessMessage<TEvent>(message, eventName, cancellation);

                await DeleteMessage<TEvent>(message, eventName, cancellation);

                _logger.LogInformation("Successfully processed and deleted message {MessageId} from topic {Topic}", message.MessageId, topic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message {MessageId} from topic {Topic}. Will retry later.", message.MessageId, topic);
                throw;
            }
        }
    }

    private async Task DeleteMessage<TEvent>(Message message, string topic, CancellationToken cancellation) where TEvent : Event
    {
        await _connection.Client.DeleteMessageAsync(
            queueUrl: _connection.BaseUrl + topic,
            receiptHandle: message.ReceiptHandle,
            cancellationToken: cancellation
        );

        _logger.LogInformation("Message deleted for topic: {Topic}, MessageId: {MessageId}", topic, message.MessageId);
    }

    private async Task ProcessMessage<TEvent>(Message message, string topic, CancellationToken cancellation) where TEvent : Event
    {
        if (!_subscriptionManager.HasSubscription<TEvent>(topic))
            return;

        using ILifetimeScope scope = _lifetimeScope.BeginLifetimeScope(_scopeName);

        foreach (SubscriptionInfo subscription in _subscriptionManager.GetHandlersForEvent<TEvent>(topic))
        {
            TEvent @event = GetEvent<TEvent>(message);

            IEventHandler<TEvent> handler = GetHandler<TEvent>(scope, subscription);

            if (handler == null) continue;

            await handler.Handle(@event, cancellation);
        }

        scope.Dispose();
    }

    private static TEvent GetEvent<TEvent>(Message message) where TEvent : Event
    {
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        TEvent @event = JsonSerializer.Deserialize<TEvent>(message.Body, options);

        @event.EventId = message.MessageId;

        if (message.Attributes.TryGetValue("ApproximateReceiveCount", out string approximateReceiveCount))
        {
            if (int.TryParse(approximateReceiveCount, out int receiveCount))
                @event.ReceiveCount = receiveCount;
        }

        if (message.Attributes.TryGetValue("SentTimestamp", out var timestampStr))
        {
            var sentUnixMillis = long.Parse(timestampStr);
            @event.CreationDate = DateTimeOffset.FromUnixTimeMilliseconds(sentUnixMillis).UtcDateTime;
        }

        return @event;
    }

    private static IEventHandler<TEvent> GetHandler<TEvent>(ILifetimeScope scope, SubscriptionInfo subscription) where TEvent : Event
    {
        object resolved = ResolutionExtensions.ResolveOptional((IComponentContext)(object)scope, subscription.HandlerType);

        return resolved is not null ? (IEventHandler<TEvent>)scope.Resolve(subscription.HandlerType) : null;
    }

    #endregion
}
