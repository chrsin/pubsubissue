using System.Collections.Concurrent;
using System.Threading.Channels;
using Api.EventHandling;
using Google.Cloud.PubSub.V1;

namespace Tests.Shared;

public class TestPubSubEventHandler : IPubSubEventHandler {
	private readonly IPubSubEventHandler _eventHandler;

	private readonly ConcurrentDictionary<string, Channel<(PubsubMessage Message, SubscriberClient.Reply Reply)>>
		_channels = new();

	public TestPubSubEventHandler(IPubSubEventHandler eventHandler) {
		_eventHandler = eventHandler;
		foreach (var topicName in Topics.All) {
			_channels.TryAdd(topicName,
				Channel.CreateUnbounded<(PubsubMessage, SubscriberClient.Reply)>());
		}
	}

	public async Task<SubscriberClient.Reply> HandleMessage(string topic, PubsubMessage message,
		CancellationToken cancellationToken) {
		var response = await _eventHandler.HandleMessage(topic, message, cancellationToken);
		await Enqueue(topic, message, response, cancellationToken);
		return response;
	}

	public async Task<(PubsubMessage Message, SubscriberClient.Reply Reply)> ReadMessage(string topic,
		Func<PubsubMessage, SubscriberClient.Reply, bool> evaluator, CancellationToken cancellationToken) {
		var channel = GetChannel(topic);
		while (true) {
			var message = await channel.Reader.ReadAsync(cancellationToken);
			if (evaluator(message.Message, message.Reply))
				return message;
		}
	}

	private async Task Enqueue(string topicName, PubsubMessage message, SubscriberClient.Reply reply,
		CancellationToken cancellationToken) {
		var channel = GetChannel(topicName);
		await channel.Writer.WriteAsync((message, reply), cancellationToken);
	}

	private Channel<(PubsubMessage Message, SubscriberClient.Reply Reply)> GetChannel(string topicName) {
		if (_channels.TryGetValue(topicName, out Channel<(PubsubMessage, SubscriberClient.Reply)>? channel))
			return channel;

		throw new InvalidOperationException($"No topic with name {topicName}");
	}
}