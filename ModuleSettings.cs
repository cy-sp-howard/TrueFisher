using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework.Input;

namespace BhModule.TrueFisher
{
    public class ModuleSettings
    {
        private readonly Module module;
        public SettingEntry<KeyBinding> EnableToggle { get; private set; }
        public SettingEntry<bool> EnsureFishSuccess { get; private set; }
        public SettingEntry<bool> ChineseUI { get; private set; }
        public ModuleSettings(Module module, SettingCollection settings)
        {
            this.module = module;
            InitFishSettings(settings);
            InitUISetting(settings);
        }
        private void InitFishSettings(SettingCollection settings)
        {
            this.EnableToggle = settings.DefineSetting(nameof(this.EnableToggle), new KeyBinding(Keys.K), () => "Toggle auto fishing", () => "");
            this.EnableToggle.Value.Enabled = true;
            this.EnableToggle.Value.Activated += (sender, args) => { module.ControlService.Enable = !module.ControlService.Enable; };

            this.EnsureFishSuccess = settings.DefineSetting(nameof(this.EnsureFishSuccess), false, () => "Fishing always success", () => "When fishing progression < 0.05, will force success.");
        }
        private void InitUISetting(SettingCollection settings)
        {
            this.ChineseUI = settings.DefineSetting(nameof(this.ChineseUI), false, () => "Use Chinese UI", () => "");
            this.ChineseUI.SettingChanged += (sender, args) => { module.ControlService.SetUILang(); };
        }
    }
}
