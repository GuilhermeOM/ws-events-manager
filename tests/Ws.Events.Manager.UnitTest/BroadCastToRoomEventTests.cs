using Ws.Events.Manager.Root;
using Ws.Events.Manager.Root.Entities;
using Ws.Events.Manager.Root.Entities.Enums;
using Ws.Events.Manager.Root.Entities.Feedback;
using Ws.Events.Manager.Root.Events;
using Ws.Events.Manager.UnitTest.Utils;

namespace Ws.Events.Manager.UnitTest;

[TestFixture, Description("Testar as respostas do evento BroadCastToRoomEvent"), Category("Event")]
public class BroadCastToRoomEventTests
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
    public async Task ReceivesMessageWhenInRoomBroadcasted()
    {
        var joinRoomEvent = new JoinRoomEvent()
        {
            EventType = nameof(JoinRoomEvent),
            RoomName = Enum.GetName(typeof(Room), ROOM_ONE)!
        };

        var @event = new BroadCastToRoomEvent()
        {
            EventType = nameof(BroadCastToRoomEvent),
            RoomName = Enum.GetName(typeof(Room), ROOM_ONE)!,
            Message = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas vel faucibus risus, vitae scelerisque nibh."
        };

        _ = await _client.DispatchEvent<JoinRoomEvent, Message<JoinRoomMessage>>(joinRoomEvent);
        var eventResponse = await _client.DispatchEvent<BroadCastToRoomEvent, BroadCastToRoomWithUsername>(@event);

        Assert.Multiple(() =>
        {
            Assert.That(eventResponse.Message, Is.Not.Null);
            Assert.That(eventResponse.Message, Is.Not.Empty);
        });
    }

    [Test]
    public void DoesNotReceiveMessageWhenInRoomNotBroadcasted()
    {
        var @event = new BroadCastToRoomEvent()
        {
            EventType = nameof(BroadCastToRoomEvent),
            RoomName = Enum.GetName(typeof(Room), ROOM_ONE)!,
            Message = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas vel faucibus risus, vitae scelerisque nibh."
        };

        _ = Assert.ThrowsAsync<TimeoutException>(async () => await _client.DispatchEvent<BroadCastToRoomEvent, BroadCastToRoomWithUsername>(@event));
    }

    [Test]
    [TestCase("John Doe")]
    public async Task MessageContainsUsernameWhenRegistered(string username)
    {
        var registerEvent = new RegisterEvent()
        {
            EventType = nameof(RegisterEvent),
            Username = username
        };

        var joinRoomEvent = new JoinRoomEvent()
        {
            EventType = nameof(JoinRoomEvent),
            RoomName = Enum.GetName(typeof(Room), ROOM_ONE)!
        };

        var @event = new BroadCastToRoomEvent()
        {
            EventType = nameof(BroadCastToRoomEvent),
            RoomName = Enum.GetName(typeof(Room), ROOM_ONE)!,
            Message = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas vel faucibus risus, vitae scelerisque nibh."
        };

        _ = await _client.DispatchEvent<RegisterEvent, Message<RegisterMessage>>(registerEvent);
        _ = await _client.DispatchEvent<JoinRoomEvent, Message<JoinRoomMessage>>(joinRoomEvent);
        var eventResponse = await _client.DispatchEvent<BroadCastToRoomEvent, BroadCastToRoomWithUsername>(@event);

        Assert.That(eventResponse.From, Is.EqualTo(registerEvent.Username));
    }

    [Test]
    public async Task MessageIsEqualToTheSentOne()
    {
        var joinRoomEvent = new JoinRoomEvent()
        {
            EventType = nameof(JoinRoomEvent),
            RoomName = Enum.GetName(typeof(Room), ROOM_ONE)!
        };

        var @event = new BroadCastToRoomEvent()
        {
            EventType = nameof(BroadCastToRoomEvent),
            RoomName = Enum.GetName(typeof(Room), ROOM_ONE)!,
            Message = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas vel faucibus risus, vitae scelerisque nibh."
        };

        _ = await _client.DispatchEvent<JoinRoomEvent, Message<JoinRoomMessage>>(joinRoomEvent);
        var eventResponse = await _client.DispatchEvent<BroadCastToRoomEvent, BroadCastToRoomWithUsername>(@event);

        Assert.That(eventResponse.Message, Is.EqualTo(@event.Message));
    }
}
