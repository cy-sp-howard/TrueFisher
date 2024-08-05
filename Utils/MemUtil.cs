using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BhModule.TrueFisher
{
    internal class MemUtil
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);


        internal static void WriteMem(IntPtr hProcess, IntPtr lpBaseAddress, byte[] val)
        {
            int bytesWritten;
            WriteProcessMemory(hProcess, lpBaseAddress, val, (uint)val.Length, out bytesWritten);

        }
    }
}
