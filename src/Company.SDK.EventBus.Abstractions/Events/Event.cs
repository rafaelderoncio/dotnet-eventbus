using System.Text.Json.Serialization;

namespace Company.SDK.EventBus.Abstractions.Events;

public class Event : IDisposable
{
    public string EventId => _eventId;

    public DateTime CreationDate => _creationDate;

    public int ReceiveCount => _receiveCount;

    public string ErrorMessage { get; set; } = string.Empty;

    public void Dispose() => GC.SuppressFinalize(this);

    ~Event() => Dispose();

    private string _eventId = Guid.NewGuid().ToString("N");

    private DateTime _creationDate = DateTime.UtcNow;

    private int _receiveCount = 0;

    public Event SetCreationDate(DateTime creationDate)
    {
        if (creationDate != default)
            _creationDate = creationDate;

        _creationDate = creationDate;
        return this;
    }

    public Event SetEventId(string eventId)
    {
        if (!string.IsNullOrWhiteSpace(eventId))
            _eventId = eventId;

        return this;
    }
    
    public Event SetReceiveCount(int receiveCount)
    {
        if (receiveCount >= 0)
            _receiveCount = receiveCount;

        return this;
    }
}
