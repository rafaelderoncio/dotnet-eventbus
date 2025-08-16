namespace Company.SDK.EventBus.Abstractions.Events;

/// <summary>
/// Representa um evento base que trafega pelo barramento.
/// Cada evento possui um identificador único, data de criação,
/// número de vezes que foi processado e pode armazenar mensagens de erro.
/// </summary>
public class Event : IDisposable
{
    /// <summary>
    /// Identificador único do evento em formato <c>GUID</c> sem hífens.
    /// </summary>
    public string EventId => _eventId;

    /// <summary>
    /// Data de criação do evento em UTC.
    /// </summary>
    public DateTime CreationDate => _creationDate;

    /// <summary>
    /// Número de vezes que o evento foi recebido ou reprocessado.
    /// Esse valor pode ser utilizado para mecanismos de retry.
    /// </summary>
    public int ReceiveCount => _receiveCount;

    /// <summary>
    /// Mensagem de erro associada ao processamento do evento.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Libera recursos do evento e evita que o finalizador seja chamado pelo GC.
    /// </summary>
    public void Dispose() => GC.SuppressFinalize(this);

    ~Event() => Dispose();

    private string _eventId = Guid.NewGuid().ToString("N");
    private DateTime _creationDate = DateTime.UtcNow;
    private int _receiveCount = 0;

    /// <summary>
    /// Define manualmente a data de criação do evento.
    /// </summary>
    /// <param name="creationDate">Data desejada (em UTC).</param>
    /// <returns>O próprio objeto <see cref="Event"/> para encadeamento de chamadas (fluent API).</returns>
    public Event SetCreationDate(DateTime creationDate)
    {
        if (creationDate != default)
            _creationDate = creationDate;

        _creationDate = creationDate;
        return this;
    }

    /// <summary>
    /// Define manualmente o identificador único do evento.
    /// </summary>
    /// <param name="eventId">Identificador no formato string. Se vazio ou nulo, não é alterado.</param>
    /// <returns>O próprio objeto <see cref="Event"/>.</returns>
    public Event SetEventId(string eventId)
    {
        if (!string.IsNullOrWhiteSpace(eventId))
            _eventId = eventId;

        return this;
    }

    /// <summary>
    /// Define o número de vezes que o evento foi recebido.
    /// </summary>
    /// <param name="receiveCount">Número de tentativas. Valores negativos são ignorados.</param>
    /// <returns>O próprio objeto <see cref="Event"/>.</returns>
    public Event SetReceiveCount(int receiveCount)
    {
        if (receiveCount >= 0)
            _receiveCount = receiveCount;

        return this;
    }
}
