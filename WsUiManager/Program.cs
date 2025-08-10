using System.Reflection;
using Fleck;
using Serilog;
using WsUiManager.Entities;
using WsUiManager.Entities.Feedback;
using WsUiManager.Events;

namespace WsUiManager;
public static class Program
{
    public static void Main(string[] args) => Startup(args).Run();

    public static WebApplication Startup(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var clientEventHandlers = builder.FindAndInjectClientEventHandlers(Assembly.GetExecutingAssembly());

        _ = builder.Logging.ClearProviders();
        _ = builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

        var app = builder.Build();

        var server = new WebSocketServer("ws://0.0.0.0:8181")
        {
            RestartAfterListenError = true,
        };

        server.Start((ws) =>
        {
            ws.OnOpen = () => StateService.AddConnection(ws);

            ws.OnClose = () => StateService.RemoveConnection(ws);

            ws.OnMessage = async message => await app
                .InvokeClientEventHandler(clientEventHandlers, ws, message)
                .ContinueWith((task) => LogEventFaultsAsync(task, ws));
        });

        return app;
    }

    private static async Task LogEventFaultsAsync(Task eventTask, IWebSocketConnection ws)
    {
        var aggregateExceptions = eventTask.Exception?.Flatten();
        if (aggregateExceptions != null)
        {
            foreach (var exception in aggregateExceptions.InnerExceptions)
            {
                Log.Error(exception, "{Id} - Algo aconteceu em um disparo de evento solicitado.",
                    ws.ConnectionInfo.Id);

                await TryNotifyErrorToClientAsync(exception, ws);
            }
        }
    }

    private static async Task TryNotifyErrorToClientAsync(Exception ex, IWebSocketConnection ws)
    {
        try
        {
            Console.WriteLine(ex.StackTrace);

            await ws.Send(new Message<ErrorMessage>
            {
                ConnectionId = ws.ConnectionInfo.Id,
                Name = "ERROR_FEEDBACK",
                Data = new ErrorMessage()
                {
                    Feedback = ex.Message
                }
            }.AsJson());
        }
        catch (ConnectionNotAvailableException connectionException)
        {
            Log.Fatal(connectionException,
                "Conexão com o cliente morreu. IP do cliente: {ClientIpAddress}",
                ws.ConnectionInfo.ClientIpAddress);
        }
    }
}
