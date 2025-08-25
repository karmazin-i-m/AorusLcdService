using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AorusLcdServiceLinux.Models
{
    public class CoolerData
    {
        public string CpuName { get; set; } = string.Empty;

        public int CpuVendor { get; set; }

        public int CpuTemperature { get; set; }

        public int CpuFrequency { get; set; }

        public int CpuThreadCount { get; set; }

        public int CpuCoreCount { get; set; }

        public int CpuUsage { get; set; }

        public int CpuPower { get; set; }

        public int VRAMTemperature { get; set; }

        public int LiqiudTemperature => 0;
    }
}
