using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using AorusLcdServiceLinux.Abstract;

internal class CpuLinApi : ICpuApi
{
    public int GetCpuCoreCount()
    {
        // фізичні ядра можна визначити через /proc/cpuinfo -> "core id"
        var lines = File.ReadAllLines("/proc/cpuinfo");
        var coreIds = lines
            .Where(l => l.StartsWith("core id"))
            .Select(l => l.Split(':')[1].Trim())
            .Distinct()
            .Count();

        return coreIds > 0 ? coreIds : Environment.ProcessorCount / 2;
    }

    public async Task<string> GetCpuFrequencyAsync()
    {
        try
        {
            var freqPath = "/proc/cpuinfo";
            var line = (await File.ReadAllLinesAsync(freqPath))
                .FirstOrDefault(l => l.StartsWith("cpu MHz"));

            if (line != null)
            {
                var mhz = line.Split(':')[1].Trim();
                return Convert.ToInt32(Double.Parse(mhz)).ToString();
            }
        }
        catch { }

        return "Unknown";
    }

    public string GetCpuManufacturer()
    {
        var vendor = File.ReadLines("/proc/cpuinfo")
            .FirstOrDefault(l => l.StartsWith("vendor_id"));
        return vendor?.Split(':')[1].Trim() ?? "Unknown";
    }

    public string GetCpuName()
    {
        var model = File.ReadLines("/proc/cpuinfo")
            .FirstOrDefault(l => l.StartsWith("model name"));
        return model?.Split(':')[1].Trim() ?? "Unknown";
    }

    public double GetCpuPower()
    {
        try
        {
            // Intel RAPL: /sys/class/powercap/intel-rapl:0/energy_uj
            var path = "/sys/class/powercap/intel-rapl:0/energy_uj";
            if (File.Exists(path))
            {
                var energy1 = ulong.Parse(File.ReadAllText(path));
                Task.Delay(100).Wait();
                var energy2 = ulong.Parse(File.ReadAllText(path));
                // різниця в джоулях за час = потужність (W)
                return (energy2 - energy1) / 1000000.0 * 10; // приблизно
            }
        }
        catch { }
        return -1;
    }

    public double GetCpuTemperature()
    {
        try
        {
            foreach (var dir in Directory.GetDirectories("/sys/class/hwmon/"))
            {
                var namePath = Path.Combine(dir, "name");
                if (!File.Exists(namePath)) continue;

                var name = File.ReadAllText(namePath).Trim();
                if (name.Contains("coretemp") || name.Contains("k10temp"))
                {
                    var tempFile = Directory.GetFiles(dir, "temp*_input").FirstOrDefault();
                    if (tempFile != null)
                    {
                        var temp = double.Parse(File.ReadAllText(tempFile)) / 1000.0;
                        return temp;
                    }
                }
            }
        }
        catch { }
        return -1;
    }

    public int GetCpuThreadCount()
    {
        return Environment.ProcessorCount;
    }

    public async Task<int> GetCpuUsageAsync()
    {
        var (idle1, total1) = GetCpuTimes();
        await Task.Delay(1000);
        var (idle2, total2) = GetCpuTimes();

        var idle = idle2 - idle1;
        var total = total2 - total1;

        return (int)(100 * (1.0 - (double)idle / total));
    }

    private (long idle, long total) GetCpuTimes()
    {
        var line = File.ReadLines("/proc/stat").First(l => l.StartsWith("cpu "));
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Skip(1).Select(long.Parse).ToArray();

        long idle = parts[3];
        long total = parts.Sum();
        return (idle, total);
    }

    public float GetMaxCpuFrequency()
    {
        try
        {
            var path = "/sys/devices/system/cpu/cpu0/cpufreq/cpuinfo_max_freq";
            if (File.Exists(path))
            {
                var khz = int.Parse(File.ReadAllText(path).Trim());
                return khz / 1000f; // MHz
            }
        }
        catch { }
        return -1;
    }

    public int GetVramTemperature()
    {
        try
        {
            // NVIDIA через nvidia-smi
            var psi = new ProcessStartInfo("nvidia-smi", "--query-gpu=temperature.memory --format=csv,noheader")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            using var proc = Process.Start(psi);
            var output = proc?.StandardOutput.ReadToEnd().Trim() ?? string.Empty;
            proc?.WaitForExit();

            if (int.TryParse(output, out int temp))
                return temp;
        }
        catch { }
        return 0;
    }
}
