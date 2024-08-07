using Blish_HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BhModule.TrueFisher.Utils
{

    internal class EventUtil
    {
        public class ChangeEventArgs<T> : EventArgs
        {
            public T Prev { get; }
            public T Current { get; }

            public ChangeEventArgs(T current, T prev)
            {
                Prev = prev;
                Current = current;
            }
        }
        public static void CheckAndHandleEvent<T>(ref T previousValue, T currentValue, Action<ChangeEventArgs<T>> eventRef)
        {
            if (!Equals(previousValue, currentValue))
            {
                eventRef(new ChangeEventArgs<T>(currentValue, previousValue));
            }
        }
    }
}
