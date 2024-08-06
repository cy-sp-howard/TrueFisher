using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BhModule.TrueFisher.Utils;
using Blish_HUD;
using SharpDX.MediaFoundation;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Blish_HUD.Controls.Extern;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace BhModule.TrueFisher.Automatic
{
    public enum MemAddrOffset
    {
        PROGRESSION = 80,
        STATE = 68,
        YellowBarWidth = 0x8C
    }
    public enum FishState
    {
        START,
        PRE,
        READY,
        UNKNOWN = -1
    }
    public class FishService
    {
        const int ptrBaseOffset = 0x027A2D38;
        private readonly List<int> ptrOffsetList = new List<int>() { 10, 20, 8, 8, 0, 108, 0 };
        private Module module;

        public IntPtr MemoryAddress
        {
            get
            {
                if (module.ProcessService.Address == IntPtr.Zero) return IntPtr.Zero;
                long addr = MemUtil.ReadMem(module.ProcessService.Handle, IntPtr.Add(module.ProcessService.Address, ptrBaseOffset), 8, new List<int>() { 10, 20, 8, 8, 0, 108, 0 }).Parse<long>().value;
                return new IntPtr(addr);
            }
        }

        public bool Enabled { get; set; } = false;
        public FishState State
        {
            get; private set;

        }
        public float Progression
        {
            get; private set;

        }
        public float YellowBarWidth
        {

            get; private set;

        }

        public FishService(Module module)
        {
            this.module = module;
            GameService.Gw2Mumble.CurrentMap.MapChanged += OnMapChanged;


        }
        internal void Update(GameTime gameTime)
        {
            SetState();
            SetYellowBarWidth(null);
            SetProgression(null);
            Trace.WriteLine(State);
            if (!Enabled) return;
            if (State == FishState.READY)
            {
                useSkill1();
                YellowBarWidth = (float)1.3;
            }
        }

        private void useSkill1()
        {
            // Blish_HUD.Controls.Intern.Keyboard.Press(TrueFisher.Settings.Key_Skill_1.Value, false);
        }
        private void SetState()
        {

            Mem<byte> mem = ReadFishMem<byte>((int)MemAddrOffset.STATE);
            State = mem == null ? FishState.UNKNOWN : (FishState)mem.value;


        }
        private void SetYellowBarWidth(float? val)
        {
            if (val != null)
            {
                WriteFishMem((int)MemAddrOffset.YellowBarWidth, BitConverter.GetBytes((int)val));
            }
            Mem<float> mem = ReadFishMem<float>((int)MemAddrOffset.YellowBarWidth);
            YellowBarWidth = mem == null ? 0.0f : mem.value;

        }

        private void SetProgression(float? val)
        {
            if (val != null)
            {
                WriteFishMem((int)MemAddrOffset.PROGRESSION, BitConverter.GetBytes((int)val));
            };
            Mem<float> mem = ReadFishMem<float>((int)MemAddrOffset.PROGRESSION);

            Progression = mem == null ? 0.0f : mem.value;

        }
        private void OnMapChanged(object sender, ValueEventArgs<int> e)
        {

        }
        private Mem<T> ReadFishMem<T>(int offset)
        {
            if (MemoryAddress == IntPtr.Zero) return null;
            Mem<byte[]> mem = MemUtil.ReadMem(module.ProcessService.Handle, IntPtr.Add(MemoryAddress, offset), 8);

            return mem.Parse<T>();

        }
        private void WriteFishMem(int offset, byte[] val)
        {
            IntPtr addr = ReadFishMem<long>(offset).address;
            MemUtil.WriteMem(module.ProcessService.Handle, addr, val);
        }
    }
}
