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
using BhModule.TrueFisher.Utils;
using BhModule.TrueFisher.Automatic;
using SharpDX.Direct3D11;
using System.Diagnostics;
using System.Collections.Generic;
using MonoGame.Extended.Timers;
using System.Security.Cryptography;

namespace BhModule.TrueFisher
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class TrueFisherModule : Blish_HUD.Modules.Module
    {

        private static readonly Logger Logger = Logger.GetLogger<TrueFisherModule>();
        public FishService FishService { get; private set; }
        public PathService PathService { get; private set; }
        public DrawService DrawService { get; private set; }
        public ControlService ControlService { get; private set; }

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion
        public ModuleSettings Settings { get; private set; }
        public ModuleParameters Parameters { get; private set; }

        [ImportingConstructor]
        public TrueFisherModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            Parameters = moduleParameters;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            this.Settings = new ModuleSettings(this, settings);



        }

        // game attached run; use --module "**.bhm"   will call when Blish BeginRun()

        protected override void Initialize()
        {


        }

        //game attached run; use --module "**.bhm"   will call when Blish BeginRun()
        protected override async Task LoadAsync()
        {
            this.FishService = new FishService(this);
            this.PathService = new PathService(this);
            //this.DrawService = new DrawService(this);
            this.ControlService = new ControlService(this);
         
        }



        protected override void Update(GameTime gameTime)
        {

            var a = Parameters.DirectoriesManager.GetFullDirectoryPath("markers");
            this.FishService.Update(gameTime);
            this.ControlService.Update(gameTime);
            //this.DrawService.Update(gameTime);
            //var b = GameService.GameIntegration.Gw2Instance.Gw2Process.Handle;
            // var a = GameService.Gw2Mumble.PlayerCharacter.Forward.Y;
            //"Gw2-64.exe"+027A2D38
            //MemUtil.WriteMem(b,);
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            // Unload here

            // All static members must be manually unset
        }

    }

}
