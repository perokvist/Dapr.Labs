using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DaprSamples
{
    public class Program
    {
        public static void Main(string[] args)
            => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(b => b.AddConsole())
                .ConfigureWebHostDefaults(webBuilder =>
                    webBuilder.ConfigureServices(services =>
                        services
                            .AddControllers()
                            .AddDapr()
                    )
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseCloudEvents();
                        app.UseAuthorization();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapSubscribeHandler();
                            endpoints.MapControllers();
                        });
                    })
                );
    }
}
