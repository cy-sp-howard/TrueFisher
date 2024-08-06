using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BhModule.TrueFisher.Utils
{
    internal class MemUtil
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesRead);

        internal static void WriteMem(IntPtr hProcess, uint _addr, byte[] val)
        {
            IntPtr bytesWrittenSize;
            IntPtr addr = new(_addr);
            WriteProcessMemory(hProcess, addr, val, val.Length, out bytesWrittenSize);

        }
        internal static Mem<byte[]> ReadMem(IntPtr hProcess, uint _addr, uint memSize, List<int> ptrOffsetList = null)
        {
            IntPtr addr = new(_addr);
            uint bufferSize = ptrOffsetList.Count > 0 ? sizeof(int) : memSize;
            byte[] buffer = new byte[bufferSize];
            IntPtr bufferReadSize;

            ReadProcessMemory(hProcess, addr, buffer, buffer.Length, out bufferReadSize);
            if (ptrOffsetList.Count == 0)
            {
                return new Mem<byte[]>() { address = _addr, value = buffer };
            }

            int offset = ptrOffsetList[0];
            ptrOffsetList.RemoveAt(0);
            return ReadMem(hProcess, BitConverter.ToUInt32(buffer, 0) + (uint)offset, memSize, ptrOffsetList);
        }

    }
}
