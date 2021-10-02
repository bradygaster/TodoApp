using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using TodoApp;

namespace TodoApp
{
    public class ApplicationMapNodeNameInitializer : ITelemetryInitializer
    {
        public ApplicationMapNodeNameInitializer(IConfiguration configuration)
        {
            Name = configuration["ApplicationMapNodeName"];
        }

        public string Name { get; set; }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Cloud.RoleName = Name;
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplicationMapName(this IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();
            services.AddSingleton<ITelemetryInitializer, ApplicationMapNodeNameInitializer>();
        }
    }
}
