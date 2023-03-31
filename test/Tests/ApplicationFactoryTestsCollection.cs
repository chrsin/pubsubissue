using Api.EventHandling;
using Api.HostedServices;
using Api.Options;
using Microsoft.Extensions.Options;
using Tests.Shared;

namespace Tests;

[CollectionDefinition(CollectionName)]
public class ApplicationFactoryTestsCollection : ICollectionFixture<ApplicationFactoryInstantiation> {
	public const string CollectionName = "ApplicationFactoryCollection";
}

public class ApplicationFactoryInstantiation : IAsyncDisposable {
	public static ApplicationFactoryInstantiation? Instance { get; private set; }

	public IPubSubEventHandler PubSubEventHandler;
	public PubSubPublisher Publisher;
	private PubSubHostedService _hostedService;

	public ApplicationFactoryInstantiation() {
		Instance = this;

		PubSubEventHandler = new TestPubSubEventHandler(new SimplePubSubEventHandler());
		var options = TestConfigurationHelper.GetPubSubOptions();
		
		if (!string.IsNullOrEmpty(options.EmulatorUrl))
			Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", options.EmulatorUrl);

		_hostedService = new PubSubHostedService(new OptionsWrapper<PubSubOptions>(options), PubSubEventHandler);
		_hostedService.StartAsync(CancellationToken.None);

		Publisher = new PubSubPublisher();
		Publisher.Init(options);
	}

	public async ValueTask DisposeAsync() {
		await _hostedService.StopAsync(CancellationToken.None);
		await Publisher.DisposeAsync();
	}
}