using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BhModule.TrueFisher.Utils;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework.Input;

namespace BhModule.TrueFisher
{
    public class ModuleSettings
    {
        private readonly TrueFisherModule module;
        public SettingEntry<KeyBinding> AutoFishKey { get; private set; }
        public SettingEntry<KeyBinding> ChineseUIKey { get; private set; }
        public SettingEntry<bool> EnsureFishSuccess { get; private set; }
        public SettingEntry<float> FishYellowBarWidth { get; private set; }
        public SettingEntry<bool> ChineseUI { get; private set; }
        public ModuleSettings(TrueFisherModule module, SettingCollection settings)
        {
            this.module = module;
            InitFishSettings(settings);
            InitUISetting(settings);
        }
        private void InitFishSettings(SettingCollection settings)
        {
            this.AutoFishKey = settings.DefineSetting(nameof(this.AutoFishKey), new KeyBinding(Keys.P), () => "Toggle auto fishing", () => "");
            this.AutoFishKey.Value.Enabled = true;
            this.AutoFishKey.Value.Activated += (sender, args) =>
            {
                module.ControlService.Enable = !module.ControlService.Enable;
                WindowUtil.Notify.Show(module.ControlService.Enable ? "Enable auto fish." : "Disable auto fish.");
            };

            this.EnsureFishSuccess = settings.DefineSetting(nameof(this.EnsureFishSuccess), false, () => "Fishing always success", () => "When fishing progression < 0.05, will force success.");

            this.FishYellowBarWidth = settings.DefineSetting(nameof(this.FishYellowBarWidth), 1.1f, () => "Fish yellow bar width", () => "");
            this.FishYellowBarWidth.SetRange(0.1f, 2);
        }
        private void InitUISetting(SettingCollection settings)
        {
            this.ChineseUI = settings.DefineSetting(nameof(this.ChineseUI), false, () => "Use Chinese Game UI", () => "");
            this.ChineseUI.SettingChanged += (sender, args) => { module.ControlService.SetUILang(); };
            this.ChineseUIKey = settings.DefineSetting(nameof(this.ChineseUIKey), new KeyBinding(Keys.O), () => "Toggle chinese UI", () => "");
            this.ChineseUIKey.Value.Enabled = true;
            this.ChineseUIKey.Value.Activated += (sender, args) =>
            {
                ChineseUI.Value = !ChineseUI.Value;
                WindowUtil.Notify.Show(ChineseUI.Value ? "Enable Chinese UI." : "Disable Chinese UI.");
            };
        }
    }
}
