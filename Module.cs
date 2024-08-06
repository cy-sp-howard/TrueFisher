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

namespace BhModule.TrueFisher
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module
    {

        private static readonly Logger Logger = Logger.GetLogger<Module>();
         public FishService FishService { get; private set; }
         public PathService PathService  { get; private set; }

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

        // game attached run; use --module "**.bhm"   will call when Blish BeginRun()
        public Mem<T> abc<T>()
        {
            var a= new Mem<byte[]>() { address = 0x11, value = [ 3, 2,4 ,7]};
            if (typeof(T) == typeof(int))
            {
                var b = new Mem<int>() { address = a.address, value = BitConverter.ToInt32(a.value,0) };
                return (Mem<T>)(object)b;
            }
            return (Mem<T>)(object)a;
        }
        protected override void Initialize()
        {

            int a = -2;

            FishState b = (FishState)a;
            Trace.WriteLine("sss");

        }

        //game attached run; use --module "**.bhm"   will call when Blish BeginRun()
        protected override async Task LoadAsync()
        {
            this.FishService = new FishService();
            this.PathService = new PathService();
            Console.WriteLine("sss");
        }
  


        protected override void Update(GameTime gameTime)
        {
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
