using Confluent.Kafka;

namespace Company.SDK.EventBus.Kafka;

public sealed class KafkaConfiguration
{
    public string BootstrapServers { get; set; }
    public string GroupId { get; set; }
    public Acks Acks { get; set; } = Acks.Leader;
    public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.Plaintext;
    public SaslMechanism SaslMechanism { get; set; } = SaslMechanism.Plain;
    public AutoOffsetReset AutoOffsetReset { get; set; } = AutoOffsetReset.Earliest;
    public string SaslUsername { get; set; } = string.Empty;
    public string SaslPassword { get; set; } = string.Empty;
    public bool EnableAutoCommit { get; set; } = true;
}
