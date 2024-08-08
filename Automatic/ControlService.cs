using BhModule.TrueFisher.Utils;
using Blish_HUD;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Controls.Intern;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
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
    public enum Lang
    {
        ENG = 1,
        CN = 5
    }
    public class ControlService
    {
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
        public VirtualKeyShort Skill_1 { get=>GetGameBindButton(SettingMem.Skill_1); }
        public VirtualKeyShort MoveBack { get; }
        public VirtualKeyShort MoveForward { get; }
        public VirtualKeyShort MoveLeft { get; }
        public VirtualKeyShort MoveRight { get; }
        public VirtualKeyShort CameraDown { get; }
        public VirtualKeyShort CameraUp { get; }
        public VirtualKeyShort CameraLeft { get; }
        public VirtualKeyShort CameraRight { get; }


        public ControlService(TrueFisherModule module)
        {
            this.module = module;
            SetUILang();
            GameService.GameIntegration.Gw2Instance.Gw2Started += (sender, args) => { SetUILang(); };



        }
        public void Update(GameTime gameTime)
        {
            if (!Enable) return;
        }
        public void Unload()
        {

        }
        public void CastLine()
        {
            //range 500
            Trace.WriteLine("Cast");

        }
        public void SetHook()
        {
            Keyboard.Stroke(Skill_1);
            Thread.Sleep(50);
            module.FishService.YellowBarWidth = 1.1f;
        }
        public void SetFishSucess()
        {
            module.FishService.Progression = 1f;
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
            module.FishService.StateChanged += OnFishStateChange;
            module.FishService.ProgressionChanged += OnFishProgressionChange;
        }
        public void Stop()
        {
            module.FishService.StateChanged -= OnFishStateChange;
            module.FishService.ProgressionChanged -= OnFishProgressionChange;
        }
        public void SetUILang()
        {
            Lang val = module.Settings.ChineseUI.Value ? Lang.CN : Lang.ENG;
            MemUtil.WriteMem(GameProcess.Handle, MemUtil.Gw2Ptr(SettingMem.Language), BitConverter.GetBytes((int)val));
        }
        public VirtualKeyShort GetGameBindButton(int offset)
        {
            return (VirtualKeyShort)MemUtil.ReadMem(GameProcess.Handle, MemUtil.Gw2Ptr(0x26EFE28), 2, new List<int>() { offset * 0x8 + 0x8, 0x34 }).Parse<Int16>().value;
        }

    }
}
