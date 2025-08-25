using AorusLcdServiceLinux.Abstract;
using AorusLcdServiceLinux.API;
using AorusLcdServiceLinux.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AorusLcdService
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                        .UseWindowsService(options =>
                        {
                            options.ServiceName = "AorusLcdService";
                        })
                        .ConfigureServices(services =>
                        {
                            services.AddSingleton<ICpuApi, CpuWinApi>();

                            services.AddHostedService<CoolerDataSenderService>();
                        })
                        .Build();

            await host.RunAsync();
        }
    }
}
