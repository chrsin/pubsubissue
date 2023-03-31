using Api.EventHandling;
using Api.HostedServices;
using Api.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Tests.Shared;

public class TestWebApplicationFactory : WebApplicationFactory<Program> {
	protected override void ConfigureWebHost(IWebHostBuilder builder) {
		builder.ConfigureServices(services => {
			var hostedService = services.Single(x => x.ImplementationType == typeof(PubSubHostedService));
			services.Remove(hostedService);
			
			//Get the existing Pubsub eventhandler so we can wrap it for our 
			var existingEventHandler = services.Single(x => x.ServiceType == typeof(IPubSubEventHandler));
			
			//Remove the registration so we can overwrite it
			services.Remove(existingEventHandler);

			services.AddSingleton<IPubSubEventHandler>(ApplicationFactoryInstantiation.Instance!.PubSubEventHandler);
			//
			// if (existingEventHandler.ImplementationFactory is not null) {
			// 	services.AddSingleton(existingEventHandler.ImplementationFactory);
			// 	services.AddSingleton<IPubSubEventHandler>(x =>
			// 		new TestPubSubEventHandler((IPubSubEventHandler)existingEventHandler.ImplementationFactory(x)));
			// }
			// else if (existingEventHandler.ImplementationInstance is not null) {
			// 	services.AddSingleton(existingEventHandler.ImplementationInstance);
			// 	services.AddSingleton<IPubSubEventHandler>(x =>
			// 		new TestPubSubEventHandler((IPubSubEventHandler)existingEventHandler.ImplementationInstance));
			// }
			// else if (existingEventHandler.ImplementationType is not null) {
			// 	services.AddSingleton(existingEventHandler.ImplementationType);
			// 	services.AddSingleton<IPubSubEventHandler>(x =>
			// 		new TestPubSubEventHandler(
			// 			(IPubSubEventHandler)x.GetRequiredService(existingEventHandler.ImplementationType)));
			// }
			// else {
			// 	throw new Exception("No pubsub event handler was registered");
			// }

			//Register the publisher so we can put stuff on the queue
			services.AddSingleton(x => {
				var publisher = new PubSubPublisher();
				publisher.Init(x.GetRequiredService<IOptions<PubSubOptions>>().Value);
				return publisher;
			});
		});
	}
}