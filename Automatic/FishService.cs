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
        public event EventHandler<ChangeEventArgs<bool>> HoleNeard;
        public event EventHandler<ChangeEventArgs<float>> ProgressionChanged;

        public FishState State { get => _state; }
        private FishState _state = FishState.UNKNOWN;
        public bool HoleInRange { get => _holeInRange; }
        private bool _holeInRange = false;

        public Hole NearestHole { get => _nearestHole; }
        private Hole _nearestHole = new Hole();

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
            UpdateHoleInfo();
            UpdateYellowBarWidth();
            UpdateProgression();
        }
        private void UpdateHoleInfo()
        {
            _nearestHole.Position = new(-99999, -99999, -99999);
            bool holeInRange = _nearestHole.Distance <= 600 && _nearestHole.Distance > 200;
            EventUtil.CheckAndHandleEvent(ref _holeInRange, holeInRange, (evt) => HoleNeard?.Invoke(this, evt));
        }
        private void UpdateState()
        {
            Mem<byte> mem = GameProcess.Read<byte>(FishMem.State);
            FishState state = (FishState)mem.value;
            if (GameProcess.Read<int>(FishMem.Fishing).value == 0)
            {
                state = FishState.UNKNOWN;
            }
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
    public class Hole
    {
        public Vector3 Position = new Vector3(99999,99999,99999);
        public Vector2 HoleScreenPos { get => MapUtil.MapPosToScreenPos(Position); }

        public double Distance
        {
            get
            {
                Vector3 playerPos = GameService.Gw2Mumble.PlayerCharacter.Position;
                return MapUtil.GetDistance(Position.X, Position.Y, playerPos.X, playerPos.Y);
            }
        }
    }
}
