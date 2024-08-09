using BhModule.TrueFisher.Utils;
using Blish_HUD;
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
    public class GameProcess
    {
        static public Process Process { get => GameService.GameIntegration.Gw2Instance.Gw2Process; }

        // 這必要嗎 可以用Process.Handle?
        static public IntPtr Handle { get => Process == null ? IntPtr.Zero : MemUtil.AttachProcess(0x001F0FFF, false, Process.Id); }
        static public IntPtr Address { get => Process == null ? IntPtr.Zero : Process.MainModule.BaseAddress; }
        static public Mem<T> Read<T>(MemTrail trail)
        {
            return MemUtil.ReadMem(GameProcess.Handle, trail.StartAddress, Marshal.SizeOf<T>(), trail.Offset).Parse<T>();
        }
        static public int Write(MemTrail trail, byte[] val)
        {
            return MemUtil.WriteMem(GameProcess.Handle, trail.StartAddress, val, trail.Offset);
        }

    }

    public static class FishMem
    {
        public static readonly MemTrail BaseMemAddr = new(0x027A2D38, [0x10, 0x20, 0x8, 0x8, 0x0, 0x108]);
        public static readonly MemTrail State = BaseMemAddr.AddOffset(0x68);
        public static readonly MemTrail Progression = BaseMemAddr.AddOffset(0x80);
        public static readonly MemTrail FisPos = BaseMemAddr.AddOffset(0x84);
        public static readonly MemTrail YellowBarWidth = BaseMemAddr.AddOffset(0x8C);
        public static readonly MemTrail UserPos = BaseMemAddr.AddOffset(0x88);
        public static readonly MemTrail InRange = BaseMemAddr.AddOffset(0x90);
    }
    public static class SettingMem
    {
        public static readonly MemTrail Language = new(0x80);
        private static MemTrail KeyBindTemplate(int val) => new(0x26EFE28, [val * 0x8 + 0x8, 0x34]);
        public static readonly MemTrail Skill_1 = KeyBindTemplate(0x222);
    }

    public class MemTrail
    {
        public IntPtr StartAddress { get => IntPtr.Add(GameProcess.Address, firstOffset); }
        public IReadOnlyList<int> Offset { get => _offset.AsReadOnly(); }
        private int firstOffset;
        private List<int> _offset = new();
        public MemTrail(int firstOffset, int[] offset)
        {
            this.firstOffset = firstOffset;
            _offset.AddRange(offset);
        }
        public MemTrail(int firstOffset)
        {
            this.firstOffset = firstOffset;
        }
        public MemTrail AddOffset(int val)
        {
            List<int> n_offset = _offset.ToList();
            n_offset.Add(val);
            return new(firstOffset, n_offset.ToArray());
        }
        public MemTrail Add(int[] vals)
        {
            List<int> n_offset = _offset.ToList();
            n_offset.AddRange(vals);
            return new(firstOffset, n_offset.ToArray());
        }
    }
}
