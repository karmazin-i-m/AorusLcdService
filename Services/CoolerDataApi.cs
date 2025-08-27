using AorusLcdServiceLinux.Abstract;
using AorusLcdServiceLinux.Models;

using HidSharp;

using Microsoft.Extensions.Hosting;

namespace AorusLcdServiceLinux.Services
{
    internal class CoolerDataSenderService : IHostedService
    {
        private static readonly IEnumerable<HIDDevice> _supportedDevices = new List<HIDDevice>
        {
            new HIDDevice { VID = 0x1044, PID = 0x7A51 },
            new HIDDevice { VID = 0x1044, PID = 0x7A4D },
            new HIDDevice { VID = 0x0414, PID = 0x7A5E }
        };
        private readonly ICpuApi _cpuApi;

        public CoolerDataSenderService(ICpuApi cpuApi)
        {
            _cpuApi = cpuApi;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var device in _supportedDevices)
            {
                var deviceList = DeviceList.Local;
                var hidDevice = deviceList.GetHidDeviceOrNull(device.VID, device.PID);

                if (hidDevice == null)
                {
                    Console.WriteLine($"HID device with VID {device.VID} and {device.PID} not found.");
                    continue;
                }

                Console.WriteLine($"Found device: {hidDevice.DevicePath}");

                using (var stream = hidDevice.Open())
                {
                    var coolerData = new CoolerData
                    {
                        CpuName = _cpuApi.GetCpuName(),
                        CpuCoreCount = _cpuApi.GetCpuCoreCount(),
                        CpuThreadCount = _cpuApi.GetCpuThreadCount(),
                        CpuVendor = _cpuApi.GetCpuManufacturer().Contains("Intel") ? 1 : 0
                    };

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        coolerData.CpuTemperature = Convert.ToInt32(Math.Round(_cpuApi.GetCpuTemperature(), 0));
                        coolerData.CpuFrequency = Convert.ToInt32(await _cpuApi.GetCpuFrequencyAsync());
                        coolerData.CpuUsage = Convert.ToInt32(await _cpuApi.GetCpuUsageAsync());
                        coolerData.CpuPower = Convert.ToInt32(_cpuApi.GetCpuPower());
                        coolerData.VRAMTemperature = Convert.ToInt32(_cpuApi.GetVramTemperature());

                        var maxOutputReportLength = hidDevice.GetMaxOutputReportLength();
                        byte[] data = new byte[maxOutputReportLength];

                        data[0] = (byte)153;
                        data[1] = (byte)224;
                        data[2] = (byte)coolerData.CpuVendor;
                        data[3] = (byte)coolerData.CpuTemperature;
                        data[4] = (byte)coolerData.CpuThreadCount;
                        data[5] = (byte)(coolerData.CpuFrequency / 1000);
                        data[6] = (byte)(coolerData.CpuFrequency / 100 % 10);
                        data[7] = (byte)coolerData.CpuCoreCount;
                        data[8] = (byte)coolerData.VRAMTemperature;
                        data[9] = (byte)coolerData.LiqiudTemperature;
                        data[10] = (byte)coolerData.CpuUsage;
                        data[11] = (byte)(coolerData.CpuPower % 256);
                        data[12] = (byte)(coolerData.CpuPower / 256);

                        stream.Write(data);
                        Console.WriteLine("SendCoolerData Data:CpuVendor:" + coolerData.CpuVendor.ToString() + "|CpuName:" + coolerData.CpuName + "|CpuTemperature:" + coolerData.CpuTemperature.ToString() + "|CpuThreadCount:" + coolerData.CpuThreadCount.ToString() + "|CpuFrequency:" + (coolerData.CpuFrequency / 1000).ToString() + "." + (coolerData.CpuFrequency / 100 % 10).ToString() + "GHz|CpuCoreCount:" + coolerData.CpuCoreCount.ToString() + "|VRAMTemperature:" + coolerData.VRAMTemperature.ToString() + "|LiqiudTemperature:" + coolerData.LiqiudTemperature.ToString() + "|CpuUsage:" + coolerData.CpuUsage.ToString() + "|CpuPower:" + coolerData.CpuPower.ToString());
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
            //throw new NotImplementedException();
        }
    }
}
