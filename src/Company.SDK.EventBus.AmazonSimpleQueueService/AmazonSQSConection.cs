using Amazon.SQS;
using Company.SDK.EventBus.AmazonSimpleQueueService;

namespace Company.SDK.EventBus.AmazonSimpleQueueService;

public sealed class AmazonSQSConection(AmazonSQSConfiguration configuration, IAmazonSQS client, string topic = null) : IAmazonSQSConection
{

    public AmazonSQSConfiguration Configuration { get; } = configuration
        ?? throw new ArgumentNullException(nameof(configuration), "");

    public IAmazonSQS Client { get; } = client
        ?? throw new ArgumentNullException(nameof(client), "");

    public string Topic { get; } = topic;

    public string BaseUrl { get; } = $"https://sqs.{configuration?.Region}.amazonaws.com/{configuration?.Account}/{topic ?? string.Empty}";
}
