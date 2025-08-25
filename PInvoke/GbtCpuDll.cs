using System.Runtime.InteropServices;

namespace AorusLcdServiceLinux.PInvoke
{
    internal class GbtCpuDll
    {
        [DllImport("lib\\GbtCpuLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint CPUGetCurrentTemperature(out double pfTemperature);

        [DllImport("lib\\GbtCpuLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint CPUGetPackagePower(out double pfPackagePower);
    }
}
