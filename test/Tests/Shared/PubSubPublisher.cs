using Api.EventHandling;
using Api.Options;
using Google.Api.Gax;
using Google.Cloud.PubSub.V1;

namespace Tests.Shared;

public class PubSubPublisher : IAsyncDisposable {
	private readonly Dictionary<string, PublisherClient> _publishers;

	public PubSubPublisher() {
		_publishers = new Dictionary<string, PublisherClient>();
	}

	public void Init(PubSubOptions pubSubOptions) {
		foreach (var topicName in Topics.All) {
			var publisher = new PublisherClientBuilder {
				TopicName = TopicName.FromProjectTopic(pubSubOptions.ProjectId, topicName),
				EmulatorDetection = EmulatorDetection.EmulatorOnly
			}.Build();
			
			_publishers.Add(topicName, publisher);
		}
	}

	public Task<string> PublishMessageAsync(string topic, PubsubMessage message) {
		if (!_publishers.TryGetValue(topic, out PublisherClient? client)) {
			throw new InvalidOperationException($"No client registered for topic {topic}");
		}

		return client.PublishAsync(message);
	}

	public async ValueTask DisposeAsync() {
		List<Task> shutdownTasks = new List<Task>(_publishers.Count);

		foreach (var client in _publishers.Values) {
			shutdownTasks.Add(client.ShutdownAsync(TimeSpan.FromSeconds(15)));
		}

		await Task.WhenAll(shutdownTasks);
	}
}