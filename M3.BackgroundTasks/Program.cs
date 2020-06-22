using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace M3.BackgroundTasks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;

                    WorkerSettings options = configuration.GetSection("ServiceConfigurations").Get<WorkerSettings>();

                    services.AddSingleton(options);
                    services.AddHostedService<Worker>();
                }).UseWindowsService();
    }
}
