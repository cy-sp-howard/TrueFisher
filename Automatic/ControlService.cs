using BhModule.TrueFisher.Utils;
using Blish_HUD;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Controls.Intern;
using Blish_HUD.Input;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework;
using SharpDX.DirectWrite;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BhModule.TrueFisher.Automatic
{

    public enum Lang : int
    {
        UNKNOWN = -1,
        ENG = 0,
        CN = 5
    }
    public class ControlService
    {
        private const int castLineMinRange = 300;
        private const int castLineMaxRange = 550;
        private TrueFisherModule module;
        public bool Enable
        {
            get => _enable;
            set
            {
                _enable = value;
                if (value) Start();
                else Stop();
            }
        }
        private bool _enable = false;

        private Blish_HUD.Modules.Module pathingModule { get => GameService.Module.Modules.ToList().Find(i => i.ModuleInstance.Name == "Pathing")?.ModuleInstance; }
        public VirtualKeyShort Skill_1 { get => PathService.GetGameBindButton(SettingMem.Skill_1); }
        public VirtualKeyShort Skill_3 { get => PathService.GetGameBindButton(SettingMem.Skill_3); }
        public VirtualKeyShort Interact { get => PathService.GetGameBindButton(SettingMem.Interact); }
        public VirtualKeyShort Anchor { get => PathService.GetGameBindButton(SettingMem.Anchor); }



        private Lang originUILanguage = Lang.UNKNOWN;
        private bool IsMSkiffounted
        {
            get => GameService.Gw2Mumble.PlayerCharacter.CurrentMount == MountType.Skiff;
        }
        private Vector3 playerPos
        {
            get => GameService.Gw2Mumble.PlayerCharacter.Position;
        }
        private Vector3 playerFoward
        {
            get => GameService.Gw2Mumble.PlayerCharacter.Forward;
        }
        private Vector2 currentCheckPoint = new Vector2(0, 0);


        public ControlService(TrueFisherModule module)
        {
            this.module = module;
            SetUILang();
            GameService.GameIntegration.Gw2Instance.Gw2Started += delegate { SetUILang(); };

        }
        public void Update(GameTime gameTime)
        {
            var a = GameService.Gw2Mumble;
            var _b = playerPos.X;
            var _c = playerPos.Y;

            var b = BitConverter.GetBytes(playerPos.X);
            var c = BitConverter.GetBytes(playerPos.Y);

            if (!Enable) return;
        }
        public void Unload()
        {
            SetUILang(originUILanguage);
        }

        public void MoveTargetToScreenCenter(Vector2 screenPos)
        {
            float screenCenterX = GameService.Graphics.WindowWidth / 2;
            float screenCenterY = GameService.Graphics.WindowHeight / 2;
            float moveX = screenPos.X - screenCenterX;
            float moveY = screenPos.Y - screenCenterY;

            Mouse.Press(MouseButton.LEFT, (int)screenCenterX, (int)screenCenterY);
            Mouse.Release(MouseButton.LEFT, (int)(screenCenterX + moveX), (int)(screenCenterY + moveY));

        }

        public void CastLine()
        {
            if (!module.FishService.HoleInRange) return;
            Vector3 holePos = module.FishService.NearestHole.Position;
            Vector2 resolution = GameService.Graphics.Resolution.ToVector2();
            if (holePos.X < 0 || holePos.Y < 0 || holePos.X > resolution.X || holePos.Y > resolution.Y)
            {
                MoveTargetToScreenCenter(new(holePos.X, holePos.Y));
            }
            Mouse.SetPosition(((int)holePos.X), ((int)holePos.Y));
            Keyboard.Stroke(Skill_1);
            Thread.Sleep(50);
        }
        public void SetHook()
        {
            Keyboard.Stroke(Skill_1);
            Thread.Sleep(50);
            module.FishService.YellowBarWidth = module.Settings.FishYellowBarWidth.Value;
        }
        public void SetFishSucess()
        {
            module.FishService.Progression = 1.1f;
        }

        private void OnHoldNeard(object sender, ChangeEventArgs<bool> evt)
        {
            if (!evt.Current) return;
            CastLine();
        }
        private void OnFishStateChange(object sender, ChangeEventArgs<FishState> evt)
        {
            if (evt.Current == FishState.READY) SetHook();
        }
        private void OnFishProgressionChange(object sender, ChangeEventArgs<float> evt)
        {
            if (!module.Settings.EnsureFishSuccess.Value) return;
            if (evt.Current <= 0.1f) SetFishSucess();
        }
        public void Start()
        {
            CastLine();
            module.FishService.HoleNeard += OnHoldNeard;
            module.FishService.StateChanged += OnFishStateChange;
            module.FishService.ProgressionChanged += OnFishProgressionChange;
        }
        public void Stop()
        {
            module.FishService.HoleNeard += OnHoldNeard;
            module.FishService.StateChanged -= OnFishStateChange;
            module.FishService.ProgressionChanged -= OnFishProgressionChange;
        }
        public void SetUILang(Lang val = Lang.UNKNOWN)
        {

            if (originUILanguage == Lang.UNKNOWN)
            {
                originUILanguage = (Lang)DataService.Read<int>(SettingMem.Language).value;
                if (originUILanguage == Lang.CN) originUILanguage = Lang.ENG;
            }
            if(val == Lang.UNKNOWN)
            {
                val = module.Settings.ChineseUI.Value ? Lang.CN : originUILanguage;
            }

            DataService.Write(SettingMem.Language, BitConverter.GetBytes((int)val));
        }


    }
}
