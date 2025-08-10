using Ws.Events.Manager.Root;
using Ws.Events.Manager.Root.Entities;
using Ws.Events.Manager.Root.Entities.Enums;
using Ws.Events.Manager.Root.Entities.Feedback;
using Ws.Events.Manager.Root.Events;
using Ws.Events.Manager.UnitTest.Utils;

namespace Ws.Events.Manager.UnitTest;

[TestFixture, Description("Testar as respostas do evento JoinRoomEvent"), Category("Event")]
public class JoinRoomEventTests
{
    private WebsocketEventTest _client;
    private const int ROOM_ONE = 1;

    [SetUp]
    public void Setup()
    {
        _ = Program.Startup([]);

        _client = new("ws://localhost:8181");
    }

    [TearDown]
    public void TearDown() => _client.Dispose();

    [Test]
    public async Task CanJoinExistingRoom()
    {
        var @event = new JoinRoomEvent()
        {
            EventType = nameof(JoinRoomEvent),
            RoomName = Enum.GetName(typeof(Room), ROOM_ONE)!
        };

        var eventResponse = await _client.DispatchEvent<JoinRoomEvent, Message<JoinRoomMessage>>(@event);

        Assert.Multiple(() =>
        {
            Assert.That(eventResponse?.Name, Is.EqualTo("JOINROOM_FEEDBACK"));
            Assert.That(eventResponse?.Data, Is.InstanceOf(typeof(JoinRoomMessage)));
            Assert.That(eventResponse?.Data?.RoomName, Is.EqualTo(Enum.GetName(typeof(Room), ROOM_ONE)!));
        });
    }

    [Test]
    public async Task CanNotJoinTheSameRoom()
    {
        var @event = new JoinRoomEvent()
        {
            EventType = nameof(JoinRoomEvent),
            RoomName = Enum.GetName(typeof(Room), ROOM_ONE)!
        };

        _ = await _client.DispatchEvent<JoinRoomEvent, Message<JoinRoomMessage>>(@event);
        var eventResponse = await _client.DispatchEvent<JoinRoomEvent, Message<ErrorMessage>>(@event);

        Assert.Multiple(() =>
        {
            Assert.That(eventResponse?.Name, Is.EqualTo("ERROR_FEEDBACK"));
            Assert.That(eventResponse?.Data, Is.InstanceOf(typeof(ErrorMessage)));
            Assert.That(eventResponse?.Data?.Feedback, Is.EqualTo("Client is already in the requested room"));
        });
    }

    [Test]
    [TestCase("UNEXISTING_ROOM_NAME")]
    public async Task CanNotJoinAnUnexistingRoom(string unexistingRoomName)
    {
        var @event = new JoinRoomEvent()
        {
            EventType = nameof(JoinRoomEvent),
            RoomName = unexistingRoomName
        };

        var eventResponse = await _client.DispatchEvent<JoinRoomEvent, Message<ErrorMessage>>(@event);

        Assert.Multiple(() =>
        {
            Assert.That(eventResponse?.Name, Is.EqualTo("ERROR_FEEDBACK"));
            Assert.That(eventResponse?.Data, Is.InstanceOf(typeof(ErrorMessage)));
            Assert.That(eventResponse?.Data?.Feedback, Is.EqualTo("Room not exists"));
        });
    }
}
