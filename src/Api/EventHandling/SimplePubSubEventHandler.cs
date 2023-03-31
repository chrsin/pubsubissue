using Google.Cloud.PubSub.V1;

namespace Api.EventHandling;

public class SimplePubSubEventHandler : IPubSubEventHandler {
	public Task<SubscriberClient.Reply> HandleMessage(string topic, PubsubMessage message,
		CancellationToken cancellationToken) {
		return Task.FromResult(SubscriberClient.Reply.Ack);
	}
}