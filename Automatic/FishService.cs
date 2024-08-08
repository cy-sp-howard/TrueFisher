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
using Blish_HUD.Input;
using SharpDX.XInput;

namespace BhModule.TrueFisher.Automatic
{

    public enum FishState
    {
        START,
        PRE,
        READY,
        UNKNOWN = -1
    }
    public class FishService
    {
        private const int ptrBaseOffset = 0x027A2D38;
        private readonly List<int> ptrOffsetList = new List<int>() { 10, 20, 8, 8, 0, 108, 0 };
        private Module module;
        public event EventHandler<ChangeEventArgs<FishState>> StateChanged;
        public event EventHandler<ChangeEventArgs<float>> ProgressionChanged;


        public IntPtr MemoryAddress { get; private set; }

        public FishState State { get => _state; }
        private FishState _state = FishState.UNKNOWN;
        private float _progression;
        public float Progression
        {
            get => _progression;
            set { WriteFishMem(FishMem.Progression, BitConverter.GetBytes(value)); }
        }
        private float _yellowBarWidth;
        public float YellowBarWidth
        {
            get => _yellowBarWidth;
            set { WriteFishMem(FishMem.YellowBarWidth, BitConverter.GetBytes(value)); }
        }
        public Blish_HUD.Gw2Mumble.CurrentMap CurrentMap { get => GameService.Gw2Mumble.CurrentMap; }

        public FishService(Module module)
        {
            this.module = module;
            InitMemoryAddress();
            GameService.Gw2Mumble.CurrentMap.MapChanged += (sender, args) => { InitMemoryAddress(); };
        }
        public void Update(GameTime gameTime)
        {
            UpdateState();
            UpdateYellowBarWidth();
            UpdateProgression();
        }
        public void Unload()
        {
        }


        private void UpdateState()
        {
            Mem<byte> mem = ReadFishMem<byte>(FishMem.State);
            FishState state = mem == null ? FishState.UNKNOWN : (FishState)mem.value;
            EventUtil.CheckAndHandleEvent(ref _state, state, (evt) => StateChanged?.Invoke(this, evt));
        }
        private void UpdateYellowBarWidth()
        {
            Mem<float> mem = ReadFishMem<float>(FishMem.YellowBarWidth);
            _yellowBarWidth = mem == null ? 0.0f : mem.value;
        }
        private void InitMemoryAddress()
        {
            if (GameProcess.Address == IntPtr.Zero || GameProcess.Handle == IntPtr.Zero) return;
            long addr = MemUtil.ReadMem(GameProcess.Handle, MemUtil.Gw2Ptr(ptrBaseOffset), 8, new List<int>() { 0x10, 0x20, 0x8, 0x8, 0x0, 0x108 }).Parse<long>().value;
            MemoryAddress = new IntPtr(addr);
        }
        private void UpdateProgression()
        {
            Mem<float> mem = ReadFishMem<float>((int)FishMem.Progression);
            float progression = mem == null ? 0.0f : mem.value;
            EventUtil.CheckAndHandleEvent(ref _progression, progression, (evt) => ProgressionChanged?.Invoke(this, evt));
        }

        private Mem<T> ReadFishMem<T>(int offset)
        {
            if (MemoryAddress == IntPtr.Zero) return null;
            Mem<byte[]> mem = MemUtil.ReadMem(GameProcess.Handle, IntPtr.Add(MemoryAddress, offset), 8);

            return mem.Parse<T>();

        }
        private void WriteFishMem(int offset, byte[] val)
        {
            MemUtil.WriteMem(GameProcess.Handle, IntPtr.Add(MemoryAddress, offset), val);
        }

    }
}
