namespace Company.SDK.EventBus.RabbitMQ;

public class RabbitMQConfiguration
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Hostname { get; set; }
    public string VirtualHost { get; set; } = "/";
    public string Schema => IsSSL ? "amqps" : "amqp";
    public int Port { get; set; } = 5672;
    public int TLSPort { get; set; } = 5671;
    public bool IsSSL { get; set; } = false;
    public bool AutomaticRecoveryEnabled { get; set; } = true;
    public int NetworkRecoveryIntervalSeconds { get; set; } = 5;
    public string Uri { get; set; } = string.Empty;

    public bool IsCloudAMQP { get; set; } = false;
    public string UriCloudAMQP => IsCloudAMQP ? $"{Schema}://{Username}:{Password}@{VirtualHost}/{Username}" : string.Empty;
}
