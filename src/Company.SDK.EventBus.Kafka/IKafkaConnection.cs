using System;
using Company.SDK.EventBus.Abstractions.Events;
using Confluent.Kafka;

namespace Company.SDK.EventBus.Kafka;

public interface IKafkaConnection
{
    public IProducer<string, TEvent> ProducerBuilder<TEvent>(string topic = null)  where TEvent : Event;

    public IConsumer<string, TEvent> ConsumerBuilder<TEvent>(string topic = null)  where TEvent : Event;
}
