namespace WsUiManagerTest;

using WsUiManager;
using WsUiManager.Entities;
using WsUiManager.Entities.Feedback;
using WsUiManager.Events;
using WsUiManagerTest.Utils;

[TestFixture, Description("Testar as respostas do evento RegisterEvent"), Category("Event")]
public class RegisterEventTests
{
    private WebsocketEventTest client;

    [SetUp]
    public void Setup()
    {
        _ = Program.Startup([]);

        this.client = new("ws://localhost:8181");
    }

    [TearDown]
    public void TearDown() => this.client.Dispose();

    [Test]
    [TestCase("John Doe")]
    public async Task CanRegisterAsNewUser(string username)
    {
        var @event = new RegisterEvent()
        {
            EventType = nameof(RegisterEvent),
            Username = username,
        };

        var eventResponse = await this.client.DispatchEvent<RegisterEvent, Message<RegisterMessage>>(@event);

        Assert.Multiple(() =>
        {
            Assert.That(eventResponse?.Name, Is.EqualTo("REGISTER_FEEDBACK"));
            Assert.That(eventResponse?.Data, Is.InstanceOf(typeof(RegisterMessage)));
            Assert.That(eventResponse?.Data?.Username, Is.EqualTo(@event.Username));
        });
    }

    [Test]
    [TestCase("John Doe")]
    public async Task CanNotUseExistingUsername(string username)
    {
        var @event = new RegisterEvent()
        {
            EventType = nameof(RegisterEvent),
            Username = username,
        };

        _ = await this.client.DispatchEvent<RegisterEvent, Message<RegisterMessage>>(@event);
        var eventResponse = await this.client.DispatchEvent<RegisterEvent, Message<ErrorMessage>>(@event);

        Assert.Multiple(() =>
        {
            Assert.That(eventResponse?.Name, Is.EqualTo("ERROR_FEEDBACK"));
            Assert.That(eventResponse?.Data, Is.InstanceOf(typeof(ErrorMessage)));
            Assert.That(eventResponse?.Data?.Feedback, Is.EqualTo("Nome de usuário já está em uso."));
        });
    }

    [Test]
    public async Task CanNotRegisterAnonymousAsUsername()
    {
        var @event = new RegisterEvent()
        {
            EventType = nameof(RegisterEvent),
            Username = "Anonymous",
        };

        var eventResponse = await this.client.DispatchEvent<RegisterEvent, Message<ErrorMessage>>(@event);

        Assert.Multiple(() =>
        {
            Assert.That(eventResponse?.Name, Is.EqualTo("ERROR_FEEDBACK"));
            Assert.That(eventResponse?.Data, Is.InstanceOf(typeof(ErrorMessage)));
            Assert.That(eventResponse?.Data?.Feedback, Is.EqualTo("Nome solicitado se trata de um nome reservado."));
        });
    }
}
