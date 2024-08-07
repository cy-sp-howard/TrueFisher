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

namespace BhModule.TrueFisher.Automatic
{
    public enum MemAddrOffset
    {
        PROGRESSION = 0x80,
        STATE = 0x68,
        YellowBarWidth = 0x8C,
        FISHPOS = 0x84, //float
        USERPOS = 0x88,  //float
        INRANGE = 0x90 //byte
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
        private const int ptrBaseOffset = 0x027A2D38;
        private readonly List<int> ptrOffsetList = new List<int>() { 10, 20, 8, 8, 0, 108, 0 };
        private Module module;
        public event EventHandler<ChangeEventArgs<FishState>> StateChanged;

        public IntPtr MemoryAddress
        {
            get; private set;
        }

        public FishState State
        {
            get => _state;
        }
        private FishState _state = FishState.UNKNOWN;
        private float _progression;
        public float Progression
        {
            get => _progression; set { WriteFishMem((int)MemAddrOffset.PROGRESSION, BitConverter.GetBytes((int)value)); }
        }
        private float _yellowBarWidth;
        public float YellowBarWidth
        {
            get => _yellowBarWidth;
            set
            {
                WriteFishMem((int)MemAddrOffset.YellowBarWidth, BitConverter.GetBytes(value));
            }
        }

        public FishService(Module module)
        {
            this.module = module;
            GameService.Gw2Mumble.CurrentMap.MapChanged += OnMapChanged;


        }
        public void Update(GameTime gameTime)
        {
            if (MemoryAddress == IntPtr.Zero) UpdateMemoryAddress();
            UpdateState();
            UpdateYellowBarWidth();
            UpdateProgression();
        }
        public void Unload()
        {
            GameService.Gw2Mumble.CurrentMap.MapChanged -= OnMapChanged;
        }


        private void UpdateState()
        {

            Mem<byte> mem = ReadFishMem<byte>((int)MemAddrOffset.STATE);
            FishState state = mem == null ? FishState.UNKNOWN : (FishState)mem.value;
            EventUtil.CheckAndHandleEvent(ref _state, state, (evt) => StateChanged?.Invoke(this, evt));



        }
        private void UpdateYellowBarWidth()
        {

            Mem<float> mem = ReadFishMem<float>((int)MemAddrOffset.YellowBarWidth);
            _yellowBarWidth = mem == null ? 0.0f : mem.value;

        }
        private void UpdateMemoryAddress()
        {

            if (module.ProcessService.Address == IntPtr.Zero || module.ProcessService.Handle == IntPtr.Zero) return;
            long addr = MemUtil.ReadMem(module.ProcessService.Handle, IntPtr.Add(module.ProcessService.Address, ptrBaseOffset), 8, new List<int>() { 0x10, 0x20, 0x8, 0x8, 0x0, 0x108 }).Parse<long>().value;
            MemoryAddress = new IntPtr(addr);

        }
        private void UpdateProgression()
        {

            Mem<float> mem = ReadFishMem<float>((int)MemAddrOffset.PROGRESSION);

            _progression = mem == null ? 0.0f : mem.value;

        }
        private void OnMapChanged(object sender, ValueEventArgs<int> e)
        {
            UpdateMemoryAddress();
        }
        private Mem<T> ReadFishMem<T>(int offset)
        {
            if (MemoryAddress == IntPtr.Zero) return null;
            Mem<byte[]> mem = MemUtil.ReadMem(module.ProcessService.Handle, IntPtr.Add(MemoryAddress, offset), 8);

            return mem.Parse<T>();

        }
        private void WriteFishMem(int offset, byte[] val)
        {
            MemUtil.WriteMem(module.ProcessService.Handle, IntPtr.Add(MemoryAddress, offset), val);
        }

    }
}
