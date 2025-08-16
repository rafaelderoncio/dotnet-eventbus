using Company.SDK.EventBus.Abstractions.Events;
using RabbitMQ.Client;

namespace Company.SDK.EventBus.RabbitMQ;

public interface IRabbitMQConnection
{
    Task<IConnection> OpenConnection<TEvent>(string name = null, string topic = null, CancellationToken cancellationToken = default) where TEvent : Event;

    Task CloseConnection<TEvent>(string name = null, string topic = null, CancellationToken cancellationToken = default) where TEvent : Event;
}
