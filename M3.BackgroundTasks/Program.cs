using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Repository.Dapper;
using Repository.Dapper.Connection;
using Repository.Dapper.Contracts;

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
                    services.AddHostedService<Worker>();
                    services.AddSingleton<IConfigRepository, ConfigRepository>();
                    services.AddSingleton<IConnectionFactory, DeafultSqlConnectionFactory>();
                });
    }
}
