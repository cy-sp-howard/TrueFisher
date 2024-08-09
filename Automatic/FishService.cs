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
        private TrueFisherModule module;
        public event EventHandler<ChangeEventArgs<FishState>> StateChanged;
        public event EventHandler<ChangeEventArgs<float>> ProgressionChanged;

        public FishState State { get => _state; }
        private FishState _state = FishState.UNKNOWN;

        public float Progression
        {
            get => _progression;
            set { GameProcess.Write(FishMem.Progression, BitConverter.GetBytes(value)); }
        }
        private float _progression;

        public float YellowBarWidth
        {
            get => _yellowBarWidth;
            set { GameProcess.Write(FishMem.YellowBarWidth, BitConverter.GetBytes(value)); }
        }
        private float _yellowBarWidth;
        public Blish_HUD.Gw2Mumble.CurrentMap CurrentMap { get => GameService.Gw2Mumble.CurrentMap; }

        public FishService(TrueFisherModule module)
        {
            this.module = module;
        }
        public void Update(GameTime gameTime)
        {
            UpdateState();
            UpdateYellowBarWidth();
            UpdateProgression();
        }
        private void UpdateState()
        {
            Mem<byte> mem = GameProcess.Read<byte>(FishMem.State);
            FishState state = mem == null ? FishState.UNKNOWN : (FishState)mem.value;
            EventUtil.CheckAndHandleEvent(ref _state, state, (evt) => StateChanged?.Invoke(this, evt));
        }
        private void UpdateYellowBarWidth()
        {
            Mem<float> mem = GameProcess.Read<float>(FishMem.YellowBarWidth);
            _yellowBarWidth = mem == null ? 0.0f : mem.value;
        }
        private void UpdateProgression()
        {
            Mem<float> mem = GameProcess.Read<float>(FishMem.Progression);
            float progression = mem == null ? 0.0f : mem.value;
            EventUtil.CheckAndHandleEvent(ref _progression, progression, (evt) => ProgressionChanged?.Invoke(this, evt));
        }

        public void Unload()
        {
        }


    }
}
