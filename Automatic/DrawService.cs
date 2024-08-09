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
        VertexPositionColor[] vertices = new VertexPositionColor[]{
            new VertexPositionColor(new Vector3(-1, 1, 0), Color.White),
            new VertexPositionColor(new Vector3(1, 1, 0), Color.White),
            new VertexPositionColor(new Vector3(1, -1, 0), Color.White),
            new VertexPositionColor(new Vector3(-1, -1, 0), Color.White)
   };
        short[] indices = new short[] { 0, 1, 2, 0, 2, 3 };

        // 基本效果
        private TrueFisherModule module;
        private Blish_HUD.Modules.Module pathingModule { get => GameService.Module.Modules.ToList().Find(i => i.ModuleInstance.Name == "Pathing")?.ModuleInstance; }
        private GameService GG { get => GameService.Graphics; }
        public DrawService(TrueFisherModule module)
        {
            this.module = module;
            using (var a = GameService.Graphics.LendGraphicsDeviceContext())
            {
                var GD = a.GraphicsDevice;
                //GD.Clear(Color.CornflowerBlue);

                var effect = new BasicEffect(GD);
                effect.VertexColorEnabled = true;

                // 設定視圖矩陣
                effect.View = Matrix.CreateLookAt(new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up);
                // 設定投影矩陣
                effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GD.Viewport.AspectRatio, 1, 1000);

                //繪製正方形
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GD.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2);
                }
            }


        }
        public void Update(GameTime gameTime)
        {
            Trace.WriteLine("22");



        }
        public void Unload()
        {

        }


    }


}
