namespace WsUiManagerTest.Utils;

using System.Text.Encodings.Web;
using System.Text.Json;
using Websocket.Client;
using WsUiManager.Events.Base;
using WsUiManagerTest.Utils.Exceptions;

public class WebsocketEventTest : IDisposable
{
    private readonly List<string> messages = [];
    private readonly WebsocketClient client;

    private bool disposedValue;

    public WebsocketEventTest(string serverURL)
    {
        this.client = new WebsocketClient(new Uri(serverURL));

        _ = this.client.MessageReceived.Subscribe(message =>
        {
            if (!string.IsNullOrEmpty(message.Text))
            {
                lock (this.messages)
                {
                    this.messages.Add(message.Text);
                }
            }
        });

        this.StartWebsocketClientAsync();
    }

    private async void StartWebsocketClientAsync()
    {
        await this.client.Start();

        if (!this.client.IsRunning)
        {
            throw new WebsocketClientIsNotRunningException();
        }
    }

    private void KillWebsocketClient()
    {
        this.messages.Clear();

        if (this.client != null && this.client.IsRunning)
        {
            this.client.Dispose();
        }
    }

    public async Task<TM> DispatchEvent<T, TM>(T @event, int maximumAwaitTimeInSeconds = 5)
        where T : BaseEvent
        where TM : class
    {
        var serializedEvent = JsonSerializer.Serialize(@event, SerializerOptions);
        _ = this.client.Send(serializedEvent);

        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(maximumAwaitTimeInSeconds))
        {
            foreach (var message in this.messages)
            {
                var deserializedMessage = JsonHelper.DeserializeOrDefault<TM>(message, SerializerOptions);

                if (deserializedMessage != null)
                {
                    this.messages.Clear();
                    return deserializedMessage;
                }
            }

            await Task.Delay(100);
        }

        throw new TimeoutException("Tempo limite de aguardo de resposta foi excedido.");
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.KillWebsocketClient();
            }

            this.disposedValue = true;
        }
    }
}
