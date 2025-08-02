using Amazon.SQS;
using Company.SDK.EventBus.AmazonSimpleQueueService;

namespace Company.SDK.EventBus.AmazonSimpleQueueService;

public interface IAmazonSQSConection
{
    public AmazonSQSConfiguration Configuration { get; }
    public IAmazonSQS Client { get; }
    public string Topic { get; }
    public string BaseUrl { get; }
}
