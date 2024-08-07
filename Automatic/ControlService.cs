using Blish_HUD;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        };
        private bool _enable;
        public ControlService(Module module)
        {
            this.module = module;
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

        }
        public void SetHook()
        {

        }
        public void Start() { }
        public void Stop() { }


    }
}
