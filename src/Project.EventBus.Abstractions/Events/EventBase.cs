using System.Text.Json.Serialization;

namespace Project.EventBus.Abstractions.Events;

/// <summary>
/// Representa a base para todos os eventos publicados no EventBus.
/// Contém propriedades comuns para rastreamento e controle de entrega.
/// </summary>
public class EventBase : IDisposable
{
    /// <summary>
    /// Identificador único do evento.
    /// </summary>
    [JsonIgnore]
    public string EventId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Data de criação do evento.
    /// </summary>
    [JsonIgnore]
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Mensagem de erro associada ao processamento do evento.
    /// </summary>
    [JsonIgnore]
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Contador de quantas vezes o evento foi recebido.
    /// </summary>
    [JsonIgnore]
    public int ReceiveCount { get; set; } = 0;

    /// <summary>
    /// Libera recursos usados pelo evento.
    /// </summary>
    public void Dispose() => GC.SuppressFinalize(this);

    /// <summary>
    /// Finalizador para o EventBase.
    /// </summary>
    ~EventBase() => Dispose();
}
