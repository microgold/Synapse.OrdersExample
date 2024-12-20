using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Synapse.OrdersExample
{
    /// <summary>
    /// Main program class to process orders.
    /// </summary>
    public class Program
    {
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

          
            // if its in production use the actual implementation
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                services.AddScoped<IOrderServiceClient, OrderServiceClient>();
            }
            else
            {
                services.AddScoped<IOrderServiceClient, MockOrderServiceClient>();
            }

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddLog4Net("log4net.config");
            });
            services.AddScoped<IProcess, Process>();
            return services.BuildServiceProvider();
        }

        public static async Task Main(string[] args)
        {
            var serviceProvider = ConfigureServices();
            var process = serviceProvider.GetRequiredService<IProcess>();
            await process.RunAsync(args);
        }
    }
}