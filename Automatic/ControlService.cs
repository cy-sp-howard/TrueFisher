﻿using BhModule.TrueFisher.Utils;
using Blish_HUD;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Controls.Intern;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
        static public Dictionary<VirtualKeyShort, VirtualKeyShort> GameKeyMap = new() {
            {VirtualKeyShort.ACCEPT,VirtualKeyShort.RIGHT },
            {VirtualKeyShort.NONCONVERT,VirtualKeyShort.LEFT },
        };
        public VirtualKeyShort Skill_1 { get => GetGameBindButton(SettingMem.Skill_1); }
        public VirtualKeyShort Skill_3 { get => GetGameBindButton(SettingMem.Skill_3); }
        public VirtualKeyShort Interact { get => GetGameBindButton(SettingMem.Interact); }
        public VirtualKeyShort Anchor { get => GetGameBindButton(SettingMem.Anchor); }
        public VirtualKeyShort AboutFace { get => GetGameBindButton(SettingMem.AboutFace); }



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
            DataService.AgentDLL.Ready += delegate { SetUILang(); };

        }
        public void Update(GameTime gameTime)
        {
            if (!Enable) return;
        }
        public void Unload()
        {
            SetUILang(originUILanguage);
        }

        public void CastLine()
        {
            if (module.FishService.State != FishState.UNKNOWN) return;
            if (!module.FishService.HoleInRange) return;
            var targetHole = module.FishService.NearestHole;
            if (targetHole == null) return;


            Vector2 holeScreenPos = targetHole.ScreenPos;
            if (holeScreenPos.X < 0 || holeScreenPos.Y < 0 || holeScreenPos.X > GameService.Graphics.WindowWidth || holeScreenPos.Y > GameService.Graphics.WindowHeight)
            {
                Keyboard.Stroke(AboutFace);
                module.FishService.NextUpdated += delegate { CastLine(); };
                return;
            }
            Mouse.SetPosition(((int)holeScreenPos.X), ((int)holeScreenPos.Y));
            Thread.Sleep(10);
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
            else if (evt.Current == FishState.UNKNOWN)
            {
                Thread.Sleep(5000);
                CastLine();
            }
        }
        private void OnFishProgressionChange(object sender, ChangeEventArgs<float> evt)
        {
            if (!module.Settings.EnsureFishSuccess.Value) return;
            if (evt.Current <= 0.1f) SetFishSucess();
        }
        public void Start()
        {
            module.FishService.Enable = true;
            module.FishService.HoleNeard += OnHoldNeard;
            module.FishService.StateChanged += OnFishStateChange;
            module.FishService.ProgressionChanged += OnFishProgressionChange;
        }
        public void Stop()
        {
            module.FishService.Enable = false;
            module.FishService.HoleNeard -= OnHoldNeard;
            module.FishService.StateChanged -= OnFishStateChange;
            module.FishService.ProgressionChanged -= OnFishProgressionChange;
        }
        public void SetUILang(Lang val = Lang.UNKNOWN)
        {
            if (DataService.AgentDLL.BaseAddress == IntPtr.Zero) return;
            if (originUILanguage == Lang.UNKNOWN)
            {
                originUILanguage = (Lang)DataService.Read<int>(SettingMem.Language).value;
                if (originUILanguage == Lang.CN) originUILanguage = Lang.ENG;
            }
            if (val == Lang.UNKNOWN)
            {
                val = module.Settings.ChineseUI.Value ? Lang.CN : originUILanguage;
            }

            DataService.Write(SettingMem.Language, BitConverter.GetBytes((int)val));
        }
        static public VirtualKeyShort GetGameBindButton(int index)
        {
            Mem<short> result = DataService.Read<short>(SettingMem.KeyBind(index));
            if (result.value == 0)
            {
                Mem<short> result2 = DataService.Read<short>(SettingMem.KeyBind(index, 1));
                return GameKeyToVirtualKey((VirtualKeyShort)result2.value);
            }
            return GameKeyToVirtualKey((VirtualKeyShort)result.value);
        }
        static public VirtualKeyShort GameKeyToVirtualKey(VirtualKeyShort key)
        {
            if (GameKeyMap.ContainsKey(key))
            {
                return GameKeyMap[key];
            }
            return key;
        }
    }


}

