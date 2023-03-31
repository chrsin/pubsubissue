using System.Reflection;
using Api.Options;
using Microsoft.Extensions.Configuration;

namespace Tests.Shared; 

public static class TestConfigurationHelper {
    public static IConfigurationRoot GetIConfigurationRoot()
    {
        var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().Location);
        var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
        var dirPath = Path.GetDirectoryName(codeBasePath);

        return new ConfigurationBuilder()
            .SetBasePath(dirPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    public static PubSubOptions GetPubSubOptions() {
        var options = new PubSubOptions();
        GetIConfigurationRoot().Bind("PubSub", options);
        return options;
    }
}