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

namespace BhModule.TrueFisher.Automatic
{
    public enum FishMemAddrOffset
    {
        PROGRESSION = -0x10,
        STATE = 0x20
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
        private readonly List<int> ptrOffsetList = new List<int>() { 0x027A2D38, 10, 20, 8, 8, 0, 108, 0 };
        private Module TrueFisher;

        public IntPtr GW2Ptr { get; set; }
        public uint FishMemBaseAddr { get; private set; }

        public bool Enabled { get; set; } = false;
        public FishState State
        {
            get
            {
                byte state = ReadFishMem<byte>((int)FishMemAddrOffset.PROGRESSION).value;
                return (FishState)state;
            }
        }
        public float Progression
        {
            get
            {
                return ReadFishMem<float>((int)FishMemAddrOffset.PROGRESSION).value;

            }
            private set
            {
                WriteFishMem((int)FishMemAddrOffset.PROGRESSION, BitConverter.GetBytes(value));
            }
        }
        public float YellowBarWidth
        {
            get
            {
                return ReadFishMem<float>((int)FishMemAddrOffset.PROGRESSION).value;

            }
            private set
            {
                WriteFishMem((int)FishMemAddrOffset.PROGRESSION, BitConverter.GetBytes(value));
            }
        }

        public FishService(Module module)
        {
            TrueFisher = module;
            GW2Ptr = GameService.GameIntegration.Gw2Instance.Gw2Process.Handle;
            GameService.Gw2Mumble.CurrentMap.MapChanged += OnMapChanged;


        }
        internal void Update(GameTime gameTime)
        {
            if (!Enabled) return;
            if(State == FishState.READY)
            {
                useSkill1();
                YellowBarWidth = (float)1.3;
            }
        }

        private void useSkill1()
        {
            Blish_HUD.Controls.Intern.Keyboard.Press(TrueFisher.Settings.Key_Skill_1.Value, false);
        }
        private void OnMapChanged(object sender, ValueEventArgs<int> e)
        {
            this.FishMemBaseAddr = MemUtil.ReadMem(GW2Ptr, (uint)GW2Ptr.ToInt32(), 4, ptrOffsetList).address;
        }
        private Mem<T> ReadFishMem<T>(int offset)
        {
            Mem<byte[]> mem = MemUtil.ReadMem(GW2Ptr, FishMemBaseAddr + (uint)offset, 8);

            return mem.Parse<T>();

        }
        private void WriteFishMem(int offset, byte[] val)
        {
            uint addr = ReadFishMem<long>(offset).address;
            MemUtil.WriteMem(GW2Ptr, addr, val);
        }
    }
}
