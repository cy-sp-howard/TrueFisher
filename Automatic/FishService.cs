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
using System.Threading;
using Blish_HUD.ArcDps.Models;
using MonoGame.Extended.Timers;

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
        public event EventHandler<ChangeEventArgs<int>> NextUpdated;

        public bool Enable
        {
            get => enable;
            set
            {
                enable = value;
                _holes.Clear();
                holeInRange = false;
            }
        }
        private bool enable = false;
        public FishState State { get => _state; }
        private FishState _state = FishState.UNKNOWN;

        private double nextScanTick = 0;
        public IReadOnlyList<Hole> Holes { get => _holes; }
        private List<Hole> _holes = new List<Hole>();
        public Hole NearestHole { get => Holes.OrderBy(p => p.Distance).FirstOrDefault(); }
        public bool HoleInRange => holeInRange;
        private bool holeInRange = false;

        public float Progression
        {
            get => _progression;
            set { DataService.Write(FishMem.Progression, BitConverter.GetBytes(value)); }
        }
        private float _progression;

        public float YellowBarWidth
        {
            get => _yellowBarWidth;
            set { DataService.Write(FishMem.YellowBarWidth, BitConverter.GetBytes(value)); }
        }
        private float _yellowBarWidth;
        public Blish_HUD.Gw2Mumble.CurrentMap CurrentMap { get => GameService.Gw2Mumble.CurrentMap; }

        public FishService(TrueFisherModule module)
        {
            this.module = module;
            GameService.Gw2Mumble.CurrentMap.MapChanged += delegate { _holes.Clear(); };
        }
        public void Update(GameTime gameTime)
        {
            if (!enable) return;
            if (gameTime.TotalGameTime.TotalMilliseconds > nextScanTick)
            {
                nextScanTick = gameTime.TotalGameTime.TotalMilliseconds + 1000;
                UpdateHolesInfo();
            }

            UpdateState();
            UpdateYellowBarWidth();
            UpdateProgression();
            CheckNearest();
        }
        private void CheckNearest()
        {
            bool inRange = false;
            if (NearestHole != null)
            {
                float distance = NearestHole.Distance;
                inRange = distance <= 825 && distance >= 100;
            }
            EventUtil.CheckAndHandleEvent(ref holeInRange, inRange, (evt) => HoleNeard?.Invoke(this, evt));
        }
        private void UpdateHolesInfo()
        {
            int holeLen= _holes.Count;
            _holes.Clear();
            IntPtr currentHole = DataService.Read<IntPtr>(FishMem.HolesStart).value;
            IntPtr holeEnd = DataService.Read<IntPtr>(FishMem.HolesEnd).value;

            while (currentHole.ToInt64() < holeEnd.ToInt64())
            {
                byte[] posBytes = MemUtil.ReadMem(DataService.Handle, currentHole, 12, [0xe8]).value;
                float x = BitConverter.ToSingle(posBytes, 0);
                float y = BitConverter.ToSingle(posBytes, 4);
                float z = BitConverter.ToSingle(posBytes, 8);
                _holes.Add(new Hole(new(x, y, z)));

                currentHole = IntPtr.Add(currentHole, 0x8);
            }
            // prepare next scan
            DataService.Write(FishMem.Scanned, [0]);
            NextUpdated?.Invoke(this, new ChangeEventArgs<int>(_holes.Count, holeLen));
            NextUpdated = null;
        }
        private void UpdateState()
        {
            Mem<byte> mem = DataService.Read<byte>(FishMem.State);
            FishState state = (FishState)mem.value;
            if (DataService.Read<int>(FishMem.Fishing).value == 0)
            {
                state = FishState.UNKNOWN;
            }
            EventUtil.CheckAndHandleEvent(ref _state, state, (evt) => StateChanged?.Invoke(this, evt));
        }
        private void UpdateYellowBarWidth()
        {
            Mem<float> mem = DataService.Read<float>(FishMem.YellowBarWidth);
            _yellowBarWidth = mem == null ? 0.0f : mem.value;
        }
        private void UpdateProgression()
        {
            Mem<float> mem = DataService.Read<float>(FishMem.Progression);
            float progression = mem == null ? 0.0f : mem.value;
            EventUtil.CheckAndHandleEvent(ref _progression, progression, (evt) => ProgressionChanged?.Invoke(this, evt));
        }

        public void Unload()
        {
        }


    }
    public class Hole
    {
        public Vector3 Position;
        public Vector2 HoleScreenPos { get => MapUtil.MapPosToScreenPos(Position); }
        public float Distance { get => MapUtil.GetPlayerDistance(Position); }
        public Hole(Vector3 pos)
        {
            Position = pos;
        }
    }
}
