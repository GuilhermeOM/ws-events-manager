namespace WsUiManager.Entities;

using System.Text.Encodings.Web;
using System.Text.Json;
using Serilog;
using WsUiManager.Entities.Feedback;

public class Message<T> where T : FeedbackMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid ConnectionId { get; set; }
    public string Name { get; set; } = "UNDEFINED";
    public T? Data { get; set; }

    public string AsJson()
    {
        Log.Debug("{@Id} - {@MessageAsJson}", this.ConnectionId, this);

        return JsonSerializer.Serialize(this, JsonSerializerForMessage);
    }

    private static readonly JsonSerializerOptions JsonSerializerForMessage = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}
