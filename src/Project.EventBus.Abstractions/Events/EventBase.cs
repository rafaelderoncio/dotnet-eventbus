using System.Text.Json.Serialization;

namespace Project.EventBus.Abstractions.Events;

public class EventBase : IDisposable
{
    [JsonIgnore]
    public string EventId { get; set; } = Guid.NewGuid().ToString();

    [JsonIgnore]
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public string ErrorMessage { get; set; } = string.Empty;

    [JsonIgnore]
    public int ReceiveCount { get; set; } = 0;

    public void Dispose() => GC.SuppressFinalize(this);

    ~EventBase() => Dispose();
}
