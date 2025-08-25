using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AorusLcdServiceLinux.PInvoke
{
    internal class GbtSioDll
    {

        [DllImport("lib\\GbtSioLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateInstance(ushort addressPort, ushort dataPort);

        [DllImport("lib\\GbtSioLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DisposeInstance(IntPtr pCSuperIOInstance);

        [DllImport("lib\\GbtSioLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint SIOReadTemperature(
          IntPtr pCSuperIOInstance,
          IntPtr pTemperatureBuffer,
          uint nTemperatureBufferSize,
          ref uint pNumberOfTemperatureRead);
    }
}
