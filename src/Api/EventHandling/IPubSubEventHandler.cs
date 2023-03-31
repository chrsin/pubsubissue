using Google.Cloud.PubSub.V1;

namespace Api.EventHandling;

public interface IPubSubEventHandler {
	Task<SubscriberClient.Reply>
		HandleMessage(string topic, PubsubMessage message, CancellationToken cancellationToken);
}