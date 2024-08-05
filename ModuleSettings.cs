using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework.Input;

namespace BhModule.TrueFisher
{
    public class ModuleSettings
    {

        private const string KEYBIND_SETTINGS = "keybind-settings";


        private readonly Module _module;
        public SettingCollection KeyBindSettings { get; private set; }
        public SettingEntry<KeyBinding> Key_Skill_1 { get; private set; }
        public ModuleSettings(Module module, SettingCollection settings) {
            _module = module;
            InitKeyBindSettings(settings);
        }
        private void InitKeyBindSettings(SettingCollection settings) {
            this.KeyBindSettings = settings.AddSubCollection(KEYBIND_SETTINGS,true);
            this.Key_Skill_1 = this.KeyBindSettings.DefineSetting(nameof(this.Key_Skill_1), new KeyBinding(Keys.D1), () => "Skill 1 Key", () => "For Cast Line skill");
            this.Key_Skill_1.Value.Enabled = true;
        }
    }
}
