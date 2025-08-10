using WsUiManager;
using WsUiManager.Entities;
using WsUiManager.Entities.Enums;
using WsUiManager.Entities.Feedback;
using WsUiManager.Events;
using WsUiManagerTest.Utils;

namespace WsUiManagerTest;

[TestFixture, Description("Testar as respostas do evento LeaveRoomEvent"), Category("Event")]
public class LeaveRoomEventTests
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
    public async Task CanLeaveExistingRoom()
    {
        var joinRoomEvent = new JoinRoomEvent()
        {
            EventType = nameof(JoinRoomEvent),
            RoomName = Enum.GetName(typeof(Room), ROOM_ONE)!
        };

        var @event = new LeaveRoomEvent()
        {
            EventType = nameof(LeaveRoomEvent),
            RoomName = Enum.GetName(typeof(Room), ROOM_ONE)!
        };

        _ = await _client.DispatchEvent<JoinRoomEvent, Message<JoinRoomMessage>>(joinRoomEvent);
        var eventResponse = await _client.DispatchEvent<LeaveRoomEvent, Message<LeaveRoomMessage>>(@event);

        Assert.Multiple(() =>
        {
            Assert.That(eventResponse?.Name, Is.EqualTo("LEAVEROOM_FEEDBACK"));
            Assert.That(eventResponse?.Data, Is.InstanceOf(typeof(LeaveRoomMessage)));
            Assert.That(eventResponse?.Data?.RoomName, Is.EqualTo(Enum.GetName(typeof(Room), ROOM_ONE)!));
        });
    }

    [Test]
    public async Task CanNotLeaveRoomWhenNotInTheSpecified()
    {
        var @event = new LeaveRoomEvent()
        {
            EventType = nameof(LeaveRoomEvent),
            RoomName = Enum.GetName(typeof(Room), ROOM_ONE)!
        };

        var eventResponse = await _client.DispatchEvent<LeaveRoomEvent, Message<ErrorMessage>>(@event);

        Assert.Multiple(() =>
        {
            Assert.That(eventResponse?.Name, Is.EqualTo("ERROR_FEEDBACK"));
            Assert.That(eventResponse?.Data, Is.InstanceOf(typeof(ErrorMessage)));
            Assert.That(eventResponse?.Data?.Feedback, Is.EqualTo("Cliente não está na sala solicitada."));
        });
    }

    [Test]
    [TestCase("UNEXISTING_ROOM_NAME")]
    public async Task CanNotLeaveAnUnexistingRoom(string unexistingRoomName)
    {
        var @event = new LeaveRoomEvent()
        {
            EventType = nameof(LeaveRoomEvent),
            RoomName = unexistingRoomName
        };

        var eventResponse = await _client.DispatchEvent<LeaveRoomEvent, Message<ErrorMessage>>(@event);

        Assert.Multiple(() =>
        {
            Assert.That(eventResponse?.Name, Is.EqualTo("ERROR_FEEDBACK"));
            Assert.That(eventResponse?.Data, Is.InstanceOf(typeof(ErrorMessage)));
            Assert.That(eventResponse?.Data?.Feedback, Is.EqualTo("Sala não existe."));
        });
    }
}
