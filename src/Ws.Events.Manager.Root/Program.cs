using System.Globalization;
using System.Reflection;
using Fleck;
using Serilog;
using Ws.Events.Manager.Root.Entities;
using Ws.Events.Manager.Root.Entities.Feedback;
using Ws.Events.Manager.Root.Events;

namespace Ws.Events.Manager.Root;
public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture("en-US");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            Log.Logger = new LoggerConfiguration()
              .Enrich.FromLogContext()
              .CreateBootstrapLogger();

            Startup(args).Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An unhandled exception occurred during bootstrapping");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static WebApplication Startup(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var clientEventHandlers = builder.FindAndInjectClientEventHandlers(Assembly.GetExecutingAssembly());

        _ = builder.Logging.ClearProviders();
        _ = builder.Host.UseSerilog((context, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext());

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

        Log.Information("Server listening on port {Port}", server.Port);

        return app;
    }

    private static async Task LogEventFaultsAsync(Task eventTask, IWebSocketConnection ws)
    {
        var aggregateExceptions = eventTask.Exception?.Flatten();
        if (aggregateExceptions != null)
        {
            foreach (var exception in aggregateExceptions.InnerExceptions)
            {
                Log.Error(exception, "{Id} - Something happened in a requested event dispatch.", ws.ConnectionInfo.Id);

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
                "Client connection lost. IP: {ClientIpAddress}",
                ws.ConnectionInfo.ClientIpAddress);
        }
    }
}
