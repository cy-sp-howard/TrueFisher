using System;
using System.Collections.Generic;
using System.Linq;
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
}
