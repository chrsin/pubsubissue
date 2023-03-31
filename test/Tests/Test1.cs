using Api.EventHandling;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Tests.Shared;

namespace Tests;

[Trait("Category", "Integration")]
[Collection(ApplicationFactoryTestsCollection.CollectionName)]
public class Test1 : IClassFixture<TestWebApplicationFactory> {
	private readonly TestPubSubEventHandler _eventHandler;
	private readonly PubSubPublisher _publisher;

	public Test1(TestWebApplicationFactory factory) {
		_eventHandler = factory.Services.GetRequiredService<IPubSubEventHandler>() as TestPubSubEventHandler ??
		                throw new InvalidOperationException("TestPubSubEventHandler not registered");

		_publisher = factory.Services.GetRequiredService<PubSubPublisher>();
	}

	[Theory]
	[InlineData(Topics.Topic1)]
	[InlineData(Topics.Topic2)]
	[InlineData(Topics.Topic3)]
	[InlineData(Topics.Topic4)]
	[InlineData(Topics.Topic5)]
	[InlineData(Topics.Topic6)]
	[InlineData(Topics.Topic7)]
	[InlineData(Topics.Topic8)]
	[InlineData(Topics.Topic9)]
	[InlineData(Topics.Topic10)]
	[InlineData(Topics.Topic11)]
	public async Task CanSendMessage(string topicName) {
		//Arrange
		var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

		//Act
		var messageId = await _publisher.PublishMessageAsync(topicName, new PubsubMessage {
			Data = ByteString.CopyFromUtf8("Hello!")
		});

		//Assert
		var message =
			await _eventHandler.ReadMessage(topicName, (x, y) => x.MessageId == messageId, cts.Token);
		Assert.Equal(SubscriberClient.Reply.Ack, message.Reply);
	}

	[Fact]
	public async Task METHOD() {
		//Arrange
		foreach (var topicName in Topics.All) {
			//Act
			var messageId = await _publisher.PublishMessageAsync(topicName, new PubsubMessage {
				Data = ByteString.CopyFromUtf8("Hello!")
			});

			//Assert
			var message =
				await _eventHandler.ReadMessage(topicName, (x, y) => x.MessageId == messageId, CancellationToken.None);
			Assert.Equal(SubscriberClient.Reply.Ack, message.Reply);
		}
	}
}