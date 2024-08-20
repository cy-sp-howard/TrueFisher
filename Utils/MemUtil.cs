using Blish_HUD.Gw2Mumble;
using SharpDX;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        internal static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);

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
        internal static IntPtr FindPattern(string pattern, Process process, ProcessModule moudle = null)
        {
            if (moudle == null) moudle = process.MainModule;
            string[] patternAry = PatternToAry(pattern);
            //byte[] bytes_a = new byte[hexValuesSplit.Length];

            //for (int i = 0; i < hexValuesSplit.Length; i++)
            //{
            //    bytes_a[i] = Convert.ToByte(hexValuesSplit[i], 16);
            //}


            int patternSize = pattern.Replace(" ", "").Length / 2;
            int totalMemoryBytesSize = moudle.ModuleMemorySize;
            IntPtr startAddr = moudle.BaseAddress;
            IntPtr endAddr = IntPtr.Add(moudle.BaseAddress, totalMemoryBytesSize - patternSize);


            int pageSize = 6400000;
            int remainSize = moudle.ModuleMemorySize / pageSize;
            int maxPage = moudle.ModuleMemorySize / pageSize + (remainSize == 0 ? 0 : 1);
            int currentPage = 1;


            byte[] buffer = new byte[totalMemoryBytesSize < pageSize ? totalMemoryBytesSize : pageSize];
            IntPtr bufferReadSize;
            do
            {
                IntPtr pageStartAddr = IntPtr.Add(startAddr, (currentPage - 1) * pageSize);
                ReadProcessMemory(process.Handle, pageStartAddr, buffer, buffer.Length, out bufferReadSize);
                IndexOfPattern(ref patternAry, ref buffer);
                currentPage += 1;
            } while (currentPage <= maxPage);

            return IntPtr.Zero;
        }
        internal static string[] PatternToAry(string pattern)
        {
            bool isEven = pattern.Replace(" ", "").Length % 2 == 0;
            if (!isEven) pattern = "0" + pattern;
            string[] result = new string[pattern.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = pattern.Substring(i * 2, 2);
            }
            return result;
        }
        internal static int IndexOfPattern(ref string[] pattern, ref byte[] source)
        {
            if (source.Length < pattern.Length) { return -1; }
            for (int i = 0; i <= source.Length - pattern.Length; i++)
            {

                isEqual(pattern, SubAry(ref source, 0, pattern.Length));
            }
            return 0;
        }
        internal static T[] SubAry<T>(ref T[] ary, int startIndex, int size)
        {
            T[] result = new T[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = ary[i + startIndex];
            }
            return result;
        }
        internal static bool isEqual(string[] pattern, byte[] memory)
        {
            if (pattern.Length != memory.Length) return false;

            for (int i = 0; i < pattern.Length; i++)
            {
                string patternItem = pattern[i];
                if (patternItem.Length > 2)
                {
                    //////////////////
                    patternItem.Substring(patternItem.Length, 2);
                }
                Convert.ToByte(pattern[i], 16);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_BASIC_INFORMATION
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public IntPtr RegionSize;
        public uint Type;
        public uint State;
        public uint Protect;
        public uint TypeOfSection;
    }
}
