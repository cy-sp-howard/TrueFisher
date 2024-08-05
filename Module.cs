using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel.Composition;
using System.Runtime;
using System.Threading.Tasks;

namespace BhModule.TrueFisher
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module
    {

        private static readonly Logger Logger = Logger.GetLogger<Module>();

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion
        public ModuleSettings Settings { get; private set; }

        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) {
            Console.WriteLine("hellpw");
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            this.Settings = new ModuleSettings(this, settings);


        }

        protected override void Initialize()
        {

        }

        protected override async Task LoadAsync()
        {
            
        }

        protected override void OnModuleLoaded(EventArgs e)
        {

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        protected override void Update(GameTime gameTime)
        {
            var b = GameService.GameIntegration.Gw2Instance.Gw2Process.Handle;
            var a = GameService.Gw2Mumble.PlayerCharacter.Forward.Y;
            //"Gw2-64.exe"+027A2D38
            MemUtil.WriteMem(b,);
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            // Unload here

            // All static members must be manually unset
        }

    }

}
