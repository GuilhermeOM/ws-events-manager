using System.Text.Encodings.Web;
using System.Text.Json;
using Serilog;
using Ws.Events.Manager.Root.Entities.Feedback;

namespace Ws.Events.Manager.Root.Entities;
public class Message<T> where T : FeedbackMessage
{
    private static readonly JsonSerializerOptions _jsonSerializerForMessage = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid ConnectionId { get; set; }
    public string Name { get; set; } = "UNDEFINED";
    public T? Data { get; set; }

    public string AsJson()
    {
        Log.Debug("{@Id} - {@MessageAsJson}", ConnectionId, this);

        return JsonSerializer.Serialize(this, _jsonSerializerForMessage);
    }
}
