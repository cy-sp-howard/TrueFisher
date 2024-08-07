using BhModule.TrueFisher.Utils;
using Blish_HUD;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Controls.Intern;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BhModule.TrueFisher.Automatic
{
    public class ControlService
    {
        private Module module;
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
        public ControlService(Module module)
        {
            this.module = module;
            Enable = true;
            // 開始快速鍵


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
            Keyboard.Stroke(VirtualKeyShort.KEY_1);
            Thread.Sleep(50);
            module.FishService.YellowBarWidth = 1.25f;
        }
        private void OnStateChange(object sender, ChangeEventArgs<FishState> evt)
        {
            if (evt.Current == FishState.READY) SetHook();
        }
        public void Start()
        {
            module.FishService.StateChanged += OnStateChange;
        }
        public void Stop()
        {

            module.FishService.StateChanged -= OnStateChange;
        }


    }
}
