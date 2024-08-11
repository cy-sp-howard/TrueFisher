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
using Blish_HUD.Controls;
using Microsoft.Xna.Framework.Input;
using Blish_HUD.Entities;

namespace BhModule.TrueFisher.Automatic
{
    public class DrawService
    {


        // 基本效果
        private TrueFisherModule module;
        private Blish_HUD.Modules.Module pathingModule { get => GameService.Module.Modules.ToList().Find(i => i.ModuleInstance.Name == "Pathing")?.ModuleInstance; }

        private List<Control> controlCollection = new List<Control>();
        public DrawService(TrueFisherModule module)
        {
            this.module = module;
            DrawPic(new Toarupic(new Vector3(GameService.Gw2Mumble.PlayerCharacter.Position.X, GameService.Gw2Mumble.PlayerCharacter.Position.Y, GameService.Gw2Mumble.PlayerCharacter.Position.Z)));
        }
        public void Update(GameTime gameTime)
        {


            Trace.WriteLine('a');

        }
        public void Unload()
        {

        }
        public void DrawPic(Control control)
        {
            controlCollection.Add(control);
        }


    }

    public class Toarupic : Control
    {
        private Vector3 pos;
        private Test abc = new Test();
        VertexPositionColor[] vertices;
        DynamicVertexBuffer vertexBuffer;
        private BasicEffect effect;
        private VertexPositionColor firstDot;
        private Vector2 move = new Vector2(150, 300);
        public Toarupic(Vector3 pos)
        {
            Parent = GameService.Graphics.SpriteScreen;
            this.pos = pos;
            Size = new Point(Parent.Size.X,Parent.Size.Y );
            Location = new Point(0 , 0);
            // 創建頂點緩衝區
            using (var gdctx = GameService.Graphics.LendGraphicsDeviceContext())
            {


                effect = new BasicEffect(gdctx.GraphicsDevice);
                effect.VertexColorEnabled = true;
                effect.Projection = Matrix.CreateOrthographicOffCenter
                    (0, gdctx.GraphicsDevice.Viewport.Width,
                     gdctx.GraphicsDevice.Viewport.Height, 0,
                     0, 1);
                firstDot = new VertexPositionColor(new Vector3(move.X, move.Y, 0), Color.Yellow);
                ref var a = ref firstDot;
                vertices = new VertexPositionColor[] { new VertexPositionColor(new Vector3(0, 0, 0), Color.Blue), new VertexPositionColor(new Vector3(500, 0, 0), Color.Green),a};
                vertexBuffer = new DynamicVertexBuffer(gdctx.GraphicsDevice, typeof(VertexPositionColor), vertices.Length, BufferUsage.WriteOnly);

                vertexBuffer.SetData(vertices);

            }


        }
        public override void DoUpdate(GameTime gameTime)
        {


            Size = new Point(Parent.Size.X, Parent.Size.Y);
            var kstate = Keyboard.GetState();
            var ballSpeed = 1000f;
            if (kstate.IsKeyDown(Keys.Up))
            {
                move.Y -= ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (kstate.IsKeyDown(Keys.Down))
            {
                move.Y += ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (kstate.IsKeyDown(Keys.Left))
            {
                move.X -= ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (kstate.IsKeyDown(Keys.Right))
            {
                move.X += ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            UpdateVertices();
        }
        private void UpdateVertices()
        {
            var a = GameService.Gw2Mumble.PlayerCharacter;
            var b = GameService.Gw2Mumble.PlayerCamera;
            firstDot.Position.X = move.X;
            firstDot.Position.Y = move.Y;
        }
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {

            spriteBatch.GraphicsDevice.SetVertexBuffer(vertexBuffer);

            // 繪製三角形
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                spriteBatch.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
            }
        }
    }
    public class Test : IEntity
    {
        public Vector3 Position = new Vector3(0, 0, 0);
        public float DrawOrder { get => Vector3.DistanceSquared(this.Position, GameService.Gw2Mumble.PlayerCamera.Position); }
        public void Update(GameTime gameTime)
        {
            Trace.Write("a");
        }
        public void Render(GraphicsDevice gd, IWorld world, ICamera camera)
        {
            Trace.Write("a");
        }
    }
}
