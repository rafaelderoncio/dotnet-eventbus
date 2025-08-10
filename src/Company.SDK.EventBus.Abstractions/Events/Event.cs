using System.Text.Json.Serialization;

namespace Company.SDK.EventBus.Abstractions.Events;

public class Event : IDisposable
{
    [JsonIgnore]
    public string EventId { get; } = Guid.NewGuid().ToString("N");

    [JsonIgnore]
    public DateTime CreationDate { get; } = DateTime.UtcNow;

    [JsonIgnore]
    public string ErrorMessage { get; set; } = string.Empty;

    [JsonIgnore]
    public int ReceiveCount { get; set; } = 0;

    public void Dispose() => GC.SuppressFinalize(this);

    ~Event() => Dispose();
}
