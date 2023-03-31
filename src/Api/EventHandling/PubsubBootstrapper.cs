using Api.Options;
using Google.Api.Gax.Grpc;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Options;

namespace Api.EventHandling;

public class PubsubBootstrapper {
	private readonly PubSubOptions _options;
	private readonly PublisherServiceApiClient _publisherClient;
	private readonly SubscriberServiceApiClient _subscriberClient;

	public PubsubBootstrapper(IOptions<PubSubOptions> options, PublisherServiceApiClient publisherClient,
		SubscriberServiceApiClient subscriberClient) {
		_options = options.Value;
		_publisherClient = publisherClient;
		_subscriberClient = subscriberClient;
	}

	public async Task BootstrapAsync() {
		//Bootstrap the topics and subscriptions
		List<Task> bootStrapTasks = new List<Task>();
		foreach (var topicName in Topics.All) {
			bootStrapTasks.Add(EnsureTopicAndSubscriptionExists(topicName));
		}

		await Task.WhenAll(bootStrapTasks);
	}

	private async Task EnsureTopicAndSubscriptionExists(string topicName) {
		var topicRef = TopicName.FromProjectTopic(_options.ProjectId,
			topicName);

		try {
			var topic = await _publisherClient.CreateTopicAsync(topicRef);
		}
		catch {
			//We dont care for this POC
		}

		//Now we have the topic So we ensure the subscription also exists
		var subscriptionName =
			SubscriptionName.FromProjectSubscription(_options.ProjectId, GetSubscriptionId(topicName));
		var subscriptionRequest = new Subscription {
			SubscriptionName = subscriptionName,
			Topic = topicRef.ToString()
		};

		try {
			await _subscriberClient.CreateSubscriptionAsync(subscriptionRequest);
		}
		catch {
			//We dont care for this POC
		}
	}

	private string GetSubscriptionId(string topicName) {
		return $"sub-{topicName}";
	}
}