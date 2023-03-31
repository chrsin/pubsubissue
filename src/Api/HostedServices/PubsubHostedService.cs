using Api.EventHandling;
using Api.Options;
using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Options;

namespace Api.HostedServices;

public class PubSubHostedService : BackgroundService {
	private readonly IPubSubEventHandler _eventHandler;
	private readonly PubSubOptions _pubSubOptions;

	private List<(Task Task, SubscriberClient Client)> _subscriptions = new();

	public PubSubHostedService(IOptions<PubSubOptions> pubsubOptions, IPubSubEventHandler eventHandler) {
		_eventHandler = eventHandler;
		_pubSubOptions = pubsubOptions.Value;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
		//Lets actually setup the subscriptions
		foreach (var topicName in Topics.All) {
			var subscriberClient = await new SubscriberClientBuilder {
				EmulatorDetection = EmulatorDetection.EmulatorOnly,
				SubscriptionName =
					SubscriptionName.FromProjectSubscription(_pubSubOptions.ProjectId, GetSubscriptionId(topicName))
			}.BuildAsync(stoppingToken);

			_subscriptions.Add((
				subscriberClient.StartAsync(async (message, cancellationToken) =>
					await _eventHandler.HandleMessage(topicName, message, cancellationToken)), subscriberClient));
		}

		await await Task.WhenAny(_subscriptions.Select(_ => _.Task));
	}


	private string GetSubscriptionId(string topicName) {
		return $"sub-{topicName}";
	}
}