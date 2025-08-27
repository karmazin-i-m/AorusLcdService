using System.Runtime.InteropServices;
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
                        .UseSystemd()
                        .UseWindowsService(options =>
                        {
                            options.ServiceName = "AorusLcdService";
                        })
                        .ConfigureServices(services =>
                        {
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                                services.AddSingleton<ICpuApi, CpuWinApi>();
                            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                                services.AddSingleton<ICpuApi, CpuLinApi>();
                            else
                                throw new PlatformNotSupportedException();

                            services.AddHostedService<CoolerDataSenderService>();
                        })
                        .Build();

            await host.RunAsync();
        }
    }
}
