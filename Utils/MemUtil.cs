using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BhModule.TrueFisher.Utils
{
    public class Mem<T>
    {
        public IntPtr address { get; set; }
        public T value { get; set; }

        public Mem<TT> Parse<TT>()
        {
            if (typeof(T) != typeof(byte[])) throw new NotImplementedException("Value not byte[]");
            byte[] val = (byte[])(object)value;
            if (typeof(TT) == typeof(long))
            {
                return (Mem<TT>)(object)new Mem<long>() { address = address, value = BitConverter.ToInt64(val, 0) };
            }
            else if (typeof(TT) == typeof(int))
            {
                return (Mem<TT>)(object)new Mem<int>() { address = address, value = BitConverter.ToInt32(val, 0) };

            }
            else if (typeof(TT) == typeof(float))
            {
                return (Mem<TT>)(object)new Mem<float>() { address = address, value = BitConverter.ToSingle(val, 0) };
            }
            else if (typeof(TT) == typeof(Int16))
            {
                return (Mem<TT>)(object)new Mem<Int16>() { address = address, value = BitConverter.ToInt16(val, 0) };
            }
            else if (typeof(TT) == typeof(byte))
            {
                return (Mem<TT>)(object)new Mem<byte>() { address = address, value = val[0] };
            }
            throw new NotImplementedException("Not match type");
        }
    }
    internal class MemUtil
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesRead);

        internal static int WriteMem(IntPtr hProcess, IntPtr addr, byte[] val, IReadOnlyList<int> ptrOffsetList = null)
        {
            IntPtr targetAddr = addr;
            if (ptrOffsetList != null)
            {
                targetAddr = ReadMem(hProcess, addr, val.Length, ptrOffsetList).address;
            }
            int bytesWrittenSize;
            WriteProcessMemory(hProcess, targetAddr, val, val.Length, out bytesWrittenSize);
            return bytesWrittenSize;
        }
        internal static Mem<byte[]> ReadMem(IntPtr hProcess, IntPtr addr, int memSize, IReadOnlyList<int> ptrOffsetList = null)
        {
            int bufferSize = (ptrOffsetList ?? new List<int>()).Count > 0 ? sizeof(long) : memSize;
            byte[] buffer = new byte[bufferSize];
            IntPtr bufferReadSize;

            ReadProcessMemory(hProcess, addr, buffer, buffer.Length, out bufferReadSize);
            if (ptrOffsetList == null || ptrOffsetList.Count == 0 || bufferReadSize == IntPtr.Zero)
            {
                return new Mem<byte[]>() { address = addr, value = buffer };
            }

            int offset = ptrOffsetList[0];
            List<int> _ptrOffsetListt = ptrOffsetList.ToList();
            _ptrOffsetListt.RemoveAt(0);

            return ReadMem(hProcess, IntPtr.Add(new IntPtr(BitConverter.ToInt64(buffer, 0)), offset), memSize, _ptrOffsetListt);
        }
    }
}
