using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD;
using System.IO;
using System.Diagnostics;

namespace BhModule.TrueFisher.Automatic
{
    public class DrawService
    {
     

        // 基本效果
        private TrueFisherModule module;
        private Blish_HUD.Modules.Module pathingModule { get => GameService.Module.Modules.ToList().Find(i => i.ModuleInstance.Name == "Pathing")?.ModuleInstance; }
        public DrawService(TrueFisherModule module)
        {
            this.module = module;
     


        }
        public void Update(GameTime gameTime)
        {
        



        }
        public void Unload()
        {

        }


    }


}
