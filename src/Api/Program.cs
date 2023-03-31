using Api.EventHandling;
using Api.HostedServices;
using Api.Options;
using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var pubsubSection = builder.Configuration.GetSection("PubSub");

string? emulatorHost = pubsubSection.GetValue<string>("EmulatorUrl");
if (!string.IsNullOrEmpty(emulatorHost))
	Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", emulatorHost);

builder.Services.Configure<PubSubOptions>(pubsubSection);

//Register pubsub api clients
builder.Services.AddSingleton(new PublisherServiceApiClientBuilder {
	EmulatorDetection = EmulatorDetection.EmulatorOnly
}.Build());

builder.Services.AddSingleton(new SubscriberServiceApiClientBuilder {
	EmulatorDetection = EmulatorDetection.EmulatorOnly
}.Build());

builder.Services.AddSingleton<IPubSubEventHandler, SimplePubSubEventHandler>();

builder.Services.AddHostedService<PubSubHostedService>();

var app = builder.Build();

var pubsubOptions = app.Services.GetRequiredService<IOptions<PubSubOptions>>();
var bootStrapper = new PubsubBootstrapper(pubsubOptions, app.Services.GetRequiredService<PublisherServiceApiClient>(),
	app.Services.GetRequiredService<SubscriberServiceApiClient>());

await bootStrapper.BootstrapAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program {
}