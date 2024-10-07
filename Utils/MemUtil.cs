using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BhModule.TrueFisher.Utils
{

    internal class MemUtil
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll")]
        internal static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, int flAllocationType, int flProtect);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, int dwFreeType);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);
        [DllImport("kernel32.dll")]
        internal static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, int dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, int dwCreationFlags, IntPtr lpThreadId);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
        [DllImport("kernel32", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern long GetClassName(IntPtr hwnd, StringBuilder lpClassName, long nMaxCount);

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
            if(bufferReadSize == IntPtr.Zero)
            {
                return new Mem<byte[]>() { address = addr, value = new byte[memSize] };
            }
            else if (ptrOffsetList == null || ptrOffsetList.Count == 0)
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
            return Find.FindPattern(pattern, process, moudle);
        }
        internal static string GetClassNameOfWindow(IntPtr hwnd)
        {
            string className = "";
            StringBuilder classText = null;
            try
            {
                int cls_max_length = 1000;
                classText = new StringBuilder("", cls_max_length + 5);
                GetClassName(hwnd, classText, cls_max_length + 2);

                if (!string.IsNullOrEmpty(classText.ToString()))
                    className = classText.ToString();
            }
            catch (Exception ex)
            {
                className = ex.Message;
            }
            return className;
        }
    }

    public class Find
    {
        internal static IntPtr FindPattern(string pattern, Process process, ProcessModule moudle = null)
        {
            if (moudle == null) moudle = process.MainModule;
            string[] patternAry = PatternToAry(pattern);


            int totalMemoryBytesSize = moudle.ModuleMemorySize;
            IntPtr startAddr = moudle.BaseAddress;
            IntPtr endAddr = IntPtr.Add(moudle.BaseAddress, totalMemoryBytesSize - patternAry.Length);


            int pageSize = 6400000;
            int remainSize = moudle.ModuleMemorySize / pageSize;
            int maxPage = moudle.ModuleMemorySize / pageSize + (remainSize == 0 ? 0 : 1);
            int currentPage = 1;


            byte[] buffer = new byte[totalMemoryBytesSize < pageSize ? totalMemoryBytesSize : pageSize];
            IntPtr bufferReadSize;
            do
            {
                IntPtr pageStartAddr = IntPtr.Add(startAddr, (currentPage - 1) * pageSize);
                MemUtil.ReadProcessMemory(process.Handle, pageStartAddr, buffer, buffer.Length, out bufferReadSize);
                int index = IndexOfPattern(ref patternAry, ref buffer);
                if (index > -1) return IntPtr.Add(pageStartAddr, index);
                currentPage += 1;
            } while (currentPage <= maxPage);
            return IntPtr.Zero;
        }
        static string[] PatternToAry(string pattern)
        {
            pattern = pattern.Replace(" ", "").ToUpper();
            bool isEven = pattern.Length % 2 == 0;
            if (!isEven) pattern = "0" + pattern;
            string[] result = new string[pattern.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = pattern.Substring(i * 2, 2);
            }
            return result;
        }
        static int IndexOfPattern(ref string[] pattern, ref byte[] source)
        {
            if (source.Length < pattern.Length) { return -1; }
            for (int i = 0; i <= source.Length - pattern.Length; i++)
            {
                if (isEqual(pattern, SubAry(ref source, i, pattern.Length))) return i;
            }
            return -1;
        }
        static T[] SubAry<T>(ref T[] ary, int startIndex, int size)
        {
            T[] result = new T[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = ary[i + startIndex];
            }
            return result;
        }
        static bool isEqual(string[] pattern, byte[] memory)
        {
            string patternString = String.Join("", pattern);
            if (patternString.IndexOf("?") == -1)
            {
                return patternString == BitConverter.ToString(memory).Replace("-", "");
            }

            for (int i = 0; i < pattern.Length; i++)
            {
                string patternItem = pattern[i];

                int maskIndex = patternItem.IndexOf("?");
                if (maskIndex > -1)
                {
                    if (patternItem != "??" && memory[i].ToString()[maskIndex] != patternItem[maskIndex])
                    {
                        return false;
                    }

                }
                else if (memory[i] != Convert.ToByte(patternItem, 16))
                {
                    return false;
                }
            }
            return true;
        }
    }
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
            else if (typeof(TT) == typeof(IntPtr))
            {
                return (Mem<TT>)(object)new Mem<IntPtr>() { address = address, value = new IntPtr(BitConverter.ToInt64(val, 0)) };
            }
            else if (typeof(TT) == typeof(bool))
            {
                return (Mem<TT>)(object)new Mem<bool>() { address = address, value = BitConverter.ToBoolean(val, 0) };
            }
            throw new NotImplementedException("Not match type");
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
