using System.Diagnostics;
using System.Text;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace ToPrinterWrapper
{
     public static class MemoryPressure
     {
#if NET6_0_OR_GREATER
        public static bool IsSystemMemoryPressureHigh(double maxUsageRatio)
        {
            // Windows only: use GlobalMemoryStatusEx via P/Invoke for system-wide memory
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                ulong total = memStatus.ullTotalPhys;
                ulong avail = memStatus.ullAvailPhys;
                double usedRatio = (double)(total - avail) / total;
                return usedRatio >= maxUsageRatio;
            }
            // If unable to determine, be conservative and do not block
            return false;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx([System.Runtime.InteropServices.In, System.Runtime.InteropServices.Out] MEMORYSTATUSEX lpBuffer);
#else
        private static bool IsSystemMemoryPressureHigh(double maxUsageRatio)
        {
            // Not supported on this platform, do not block
            return false;
        }
#endif
    }
}

