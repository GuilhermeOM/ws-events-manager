using System.Text.Encodings.Web;
using System.Text.Json;
using Websocket.Client;
using Ws.Events.Manager.Root.Events.Base;
using Ws.Events.Manager.UnitTest.Utils.Exceptions;

namespace Ws.Events.Manager.UnitTest.Utils;
public class WebsocketEventTest : IDisposable
{
    private readonly List<string> _messages = [];
    private readonly WebsocketClient _client;

    private bool _disposedValue;

    public WebsocketEventTest(string serverURL)
    {
        _client = new WebsocketClient(new Uri(serverURL));

        _ = _client.MessageReceived.Subscribe(message =>
        {
            if (!string.IsNullOrEmpty(message.Text))
            {
                lock (_messages)
                {
                    _messages.Add(message.Text);
                }
            }
        });

        StartWebsocketClientAsync();
    }

    private async void StartWebsocketClientAsync()
    {
        await _client.Start();

        if (!_client.IsRunning)
        {
            throw new WebsocketClientIsNotRunningException();
        }
    }

    private void KillWebsocketClient()
    {
        _messages.Clear();

        if (_client != null && _client.IsRunning)
        {
            _client.Dispose();
        }
    }

    public async Task<TM> DispatchEvent<T, TM>(T @event, int maximumAwaitTimeInSeconds = 5)
        where T : BaseEvent
        where TM : class
    {
        var serializedEvent = JsonSerializer.Serialize(@event, SerializerOptions);
        _ = _client.Send(serializedEvent);

        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(maximumAwaitTimeInSeconds))
        {
            foreach (var message in _messages)
            {
                var deserializedMessage = JsonHelper.DeserializeOrDefault<TM>(message, SerializerOptions);

                if (deserializedMessage != null)
                {
                    _messages.Clear();
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
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                KillWebsocketClient();
            }

            _disposedValue = true;
        }
    }
}
