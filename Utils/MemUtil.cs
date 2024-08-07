using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BhModule.TrueFisher.Utils
{

    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_BASIC_INFORMATION
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public uint RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }
    internal class MemUtil
    {
        [DllImport("kernel32.dll")]
        static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesRead);

        internal static void WriteMem(IntPtr hProcess, IntPtr addr, byte[] val)
        {
            IntPtr bytesWrittenSize;
            WriteProcessMemory(hProcess, addr, val, val.Length, out bytesWrittenSize);

        }
        internal static MEMORY_BASIC_INFORMATION GetMemInfo(IntPtr hProcess, uint _addr)
        {
            MEMORY_BASIC_INFORMATION buffer;
            var a = VirtualQueryEx(hProcess, IntPtr.Zero, out buffer, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
            return buffer;
        }
        internal static IntPtr AttachProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId)
        {
            return OpenProcess(dwDesiredAccess, bInheritHandle, dwProcessId);
        }
        internal static Mem<byte[]> ReadMem(IntPtr hProcess, IntPtr addr, uint memSize, List<int> ptrOffsetList = null)
        {
            uint bufferSize = (ptrOffsetList ?? new List<int>()).Count > 0 ? sizeof(long) : memSize;
            byte[] buffer = new byte[bufferSize];
            IntPtr bufferReadSize;

            ReadProcessMemory(hProcess, addr, buffer, buffer.Length, out bufferReadSize);
            if (ptrOffsetList == null || ptrOffsetList.Count == 0 || bufferReadSize == IntPtr.Zero)
            {
                return new Mem<byte[]>() { address = addr, value = buffer };
            }

            int offset = ptrOffsetList[0];
            ptrOffsetList.RemoveAt(0);
            
            return ReadMem(hProcess, IntPtr.Add(new IntPtr(BitConverter.ToInt64(buffer, 0)), offset), memSize, ptrOffsetList);
        }

    }
}
