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

        static public IntPtr Handle { get => Process == null ? IntPtr.Zero : Process.Handle; }
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
        public static readonly MemTrail Fishing = BaseMemAddr.AddOffset(0x1A0);
        public static readonly MemTrail Progression = BaseMemAddr.AddOffset(0x80);
        public static readonly MemTrail FisPos = BaseMemAddr.AddOffset(0x84);
        public static readonly MemTrail YellowBarWidth = BaseMemAddr.AddOffset(0x8C);
        public static readonly MemTrail UserPos = BaseMemAddr.AddOffset(0x88);
        public static readonly MemTrail InRange = BaseMemAddr.AddOffset(0x90);
    }
    public static class SettingMem
    {
        public static readonly MemTrail Language = new(0x26EDB00, [0x38, 0x50, 0x334]);
        private static MemTrail KeyBindTemplate(int val) => new(0x26EFE28, [val * 0x8 + 0x8, 0x34]);
        public static int SecondKeyOffset = 0x50;
        public static readonly MemTrail Skill_1 = KeyBindTemplate(0x222);
        public static readonly MemTrail Skill_2 = KeyBindTemplate(0x372);
        public static readonly MemTrail Skill_3 = KeyBindTemplate(0x438);
        public static readonly MemTrail Interact = KeyBindTemplate(0x156);
        public static readonly MemTrail Anchor = KeyBindTemplate(0x303);
        public static readonly MemTrail TurnLeft = KeyBindTemplate(0x555);
        public static readonly MemTrail TurnRight = KeyBindTemplate(0x1CE);
        public static readonly MemTrail MoveForward = KeyBindTemplate(0x49B);
        public static readonly MemTrail MoveBackward = KeyBindTemplate(0x5A0);
    }

    public class MemTrail
    {
        public IntPtr StartAddress { get => IntPtr.Add(GameProcess.Address, FirstOffset); }
        public IReadOnlyList<int> Offset { get => _offset.AsReadOnly(); }
        public int FirstOffset { get; private set; }
        private List<int> _offset = new();
        public MemTrail(int firstOffset, int[] offset)
        {
            this.FirstOffset = firstOffset;
            _offset.AddRange(offset);
        }
        public MemTrail(int firstOffset)
        {
            this.FirstOffset = firstOffset;
        }
        public MemTrail AddOffset(int val)
        {
            return AddOffset([val]);
        }
        public MemTrail AddOffset(int[] vals)
        {
            List<int> n_offset = _offset.ToList();
            n_offset.AddRange(vals);
            return new(FirstOffset, n_offset.ToArray());
        }
    }
}
