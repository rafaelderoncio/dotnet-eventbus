using Company.SDK.EventBus.Abstractions.Events;
using Confluent.Kafka;

namespace Company.SDK.EventBus.Kafka;

public sealed class KafkaConnection : IKafkaConnection, IDisposable
{
    private readonly KafkaConfiguration _kafkaConfiguration;
    private readonly ProducerConfig _producerConfiguration;
    private readonly ConsumerConfig _consumerConfiguration;

    private Dictionary<string, object> _producerBuilderDict = new();
    private Dictionary<string, object> _consumerBuilderDict = new();

    public KafkaConnection(KafkaConfiguration configuration)
    {
        _kafkaConfiguration = configuration ??
             throw new ArgumentNullException(nameof(configuration), "KafkaConfiguration cannot be null.");

        // Producer configuration
        _producerConfiguration = new ()
        {
            BootstrapServers = _kafkaConfiguration.BootstrapServers,
            Acks = _kafkaConfiguration.Acks,
            SecurityProtocol = _kafkaConfiguration.SecurityProtocol,
            SaslMechanism = _kafkaConfiguration.SaslMechanism,
            SaslUsername = _kafkaConfiguration.SaslUsername,
            SaslPassword = _kafkaConfiguration.SaslPassword
        };

        // Consumer configuration
        _consumerConfiguration = new ()
        {
            BootstrapServers = _kafkaConfiguration.BootstrapServers,
            GroupId = _kafkaConfiguration.GroupId,
            AutoOffsetReset = _kafkaConfiguration.AutoOffsetReset,
            EnableAutoCommit = _kafkaConfiguration.EnableAutoCommit,
            SecurityProtocol = _kafkaConfiguration.SecurityProtocol,
            SaslMechanism = _kafkaConfiguration.SaslMechanism,
            SaslUsername = _kafkaConfiguration.SaslUsername,
            SaslPassword = _kafkaConfiguration.SaslPassword
        };
    }

    public IConsumer<string, TEvent> ConsumerBuilder<TEvent>(string topic = null) where TEvent : Event
    {
        string name = typeof(TEvent).Name;

        if (!_consumerBuilderDict.ContainsKey(name))
        {
            var consumer = new ConsumerBuilder<string, TEvent>(_consumerConfiguration)
                .SetValueDeserializer(new KafkaSerializer<TEvent>())
                .Build();

            _consumerBuilderDict.Add(name, consumer);
        }

        return (IConsumer<string, TEvent>)_consumerBuilderDict[name];
    }

    public IProducer<string, TEvent> ProducerBuilder<TEvent>(string topic = null) where TEvent : Event
    {
        if (_producerConfiguration == null)
            throw new InvalidOperationException("Producer configuration is not set.");

        string eventName = topic ?? typeof(TEvent).Name;

        if (!_producerBuilderDict.ContainsKey(eventName))
        {
            var producer = new ProducerBuilder<string, TEvent>(_producerConfiguration)
                .SetValueSerializer(new KafkaSerializer<TEvent>())
                .Build();

            _producerBuilderDict.Add(eventName, producer);
        }

        return (IProducer<string, TEvent>)_producerBuilderDict[eventName];
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private bool disposedValue;

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _producerBuilderDict.Clear();
                _producerBuilderDict = null;
            }
            disposedValue = true;
        }
    }

    ~KafkaConnection()
    {
        Dispose(disposing: false);
    }
}
