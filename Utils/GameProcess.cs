using BhModule.TrueFisher.Utils;
using Blish_HUD;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BhModule.TrueFisher.Utils
{
    public class GameProcess
    {
        static public Process Process { get => GameService.GameIntegration.Gw2Instance.Gw2Process; }

        // 這必要嗎?
        static public IntPtr Handle { get => Process == null ? IntPtr.Zero : MemUtil.AttachProcess(0x001F0FFF, false, Process.Id); }
        static public IntPtr Address { get => Process == null ? IntPtr.Zero : Process.MainModule.BaseAddress; }


    }
}
