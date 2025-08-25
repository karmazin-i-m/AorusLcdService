using AorusLcdServiceLinux.Abstract;
using AorusLcdServiceLinux.PInvoke;

using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AorusLcdServiceLinux.API
{

    internal class CpuWinApi : ICpuApi
    {
        [SupportedOSPlatform("windows")]
        public string GetCpuName()
        {
            try
            {
                using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    using (ManagementObjectCollection.ManagementObjectEnumerator enumerator = managementObjectSearcher.Get().GetEnumerator())
                    {
                        if (enumerator.MoveNext())
                            return enumerator?.Current["Name"]?.ToString() ?? "Unknown";
                    }
                }
                return "Unknown";
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetCpuName fail:" + ex.Message);
                return "Unknown";
            }
        }

        [SupportedOSPlatform("windows")]
        public string GetCpuManufacturer()
        {
            try
            {
                string cpuManufacturer = "";
                foreach (ManagementBaseObject managementBaseObject in new ManagementObjectSearcher("Select * from Win32_Processor").Get())
                    cpuManufacturer = managementBaseObject["Manufacturer"].ToString() ?? "Unknown";
                return cpuManufacturer;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetCpuManufacturer fail:" + ex.Message);
                return "Unknown";
            }
        }

        [SupportedOSPlatform("windows")]
        public int GetCpuCoreCount()
        {
            try
            {
                int cpuCoreCount = 0;
                foreach (ManagementBaseObject managementBaseObject in new ManagementObjectSearcher("Select * from Win32_Processor").Get())
                    cpuCoreCount += int.Parse(managementBaseObject["NumberOfCores"].ToString() ?? "0");
                int processorCount = Environment.ProcessorCount;
                return cpuCoreCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetCpuCoreCount fail:" + ex.Message);
                return 0;
            }
        }

        public int GetCpuThreadCount()
        {
            try
            {
                return Environment.ProcessorCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetCpuThreadCount fail:" + ex.Message);
                return 0;
            }
        }

        [SupportedOSPlatform("windows")]
        public async Task<string> GetCpuFrequencyAsync()
        {
            try
            {
                using (PerformanceCounter performanceCounter = new PerformanceCounter("Processor Information", "% Processor Performance", "_Total"))
                {
                    double num = (double)performanceCounter.NextValue();
                    await Task.Delay(1000);
                    return (performanceCounter.NextValue() / 100f * GetMaxCpuFrequency()).ToString("F0");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetCpuFrequency fail:" + ex.Message);
                return "0";
            }
        }

        [SupportedOSPlatform("windows")]
        public float GetMaxCpuFrequency()
        {
            using (PerformanceCounter performanceCounter = new PerformanceCounter("Processor Information", "Processor Frequency", "_Total"))
                return performanceCounter.NextValue();
        }

        [SupportedOSPlatform("windows")]
        public async Task<int> GetCpuUsageAsync()
        {
            try
            {
                PerformanceCounter performanceCounter = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
                CounterSample counterSample = performanceCounter.NextSample();
                await Task.Delay(500);
                CounterSample nextCounterSample = performanceCounter.NextSample();
                double oValue = (double)CounterSample.Calculate(counterSample, nextCounterSample);
                if (oValue < 0.0)
                    oValue = 0.0;
                if (oValue > 100.0)
                    oValue = 100.0;
                performanceCounter.Dispose();
                return Convert.ToInt32((object)oValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetCpuUsage fail:" + ex.Message);
                return 0;
            }
        }

        public double GetCpuTemperature()
            => GbtCpuDll.CPUGetCurrentTemperature(out var pfTemperature) == 0U ? pfTemperature : 0.0;

        public double GetCpuPower()
            => GbtCpuDll.CPUGetPackagePower(out var pfPackagePower) == 0U ? pfPackagePower : 0.0;

        public int GetVramTemperature() 
        {
            int nValue = 0;
            IntPtr instance = GbtSioDll.CreateInstance((ushort)46, (ushort)47);
            if (instance != IntPtr.Zero)
            {
                uint pNumberOfTemperatureRead = 0;
                int[] destination = new int[6];
                if (instance == IntPtr.Zero)
                    return 0;
                uint length = (uint)destination.Length;
                IntPtr num1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * destination.Length);
                int num2 = (int)GbtSioDll.SIOReadTemperature(instance, num1, length, ref pNumberOfTemperatureRead);
                Marshal.Copy(num1, destination, 0, destination.Length);
                Marshal.FreeHGlobal(num1);
                for (int index = 0; index < destination.Length; ++index)
                {
                    if (index == 4)
                        nValue = destination[index];
                }
            }

            if (instance != IntPtr.Zero)
                GbtSioDll.DisposeInstance(instance);

            return nValue;
        }
    }
}
