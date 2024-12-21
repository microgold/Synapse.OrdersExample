using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Synapse.OrdersExample
{
    /// <summary>
    /// Main program class for initializing Process and services.
    /// </summary>
    public class Program
    {
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.AddSingleton<IConfiguration>(configuration);



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