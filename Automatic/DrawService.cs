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
using BhModule.TrueFisher.Utils;
using System.Net;
using System.Runtime.InteropServices;

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
            MEMORY_BASIC_INFORMATION mbi  = new MEMORY_BASIC_INFORMATION();
            MEMORY_BASIC_INFORMATION mbi2 = new MEMORY_BASIC_INFORMATION();
            var a = GameService.GameIntegration;
            int result = MemUtil.VirtualQueryEx(GameService.GameIntegration.Gw2Instance.Gw2Process.Handle, IntPtr.Zero, out mbi, Marshal.SizeOf(mbi));

            Process[] processes = Process.GetProcessesByName("WindowsTerminal");
            if (processes.Length == 0) return; 
            MemUtil.FindPattern("58 C8 EC 89 F7 7F 00 00 05 00 00 00 00 00 00 00", processes[0]);
            //DrawCenterDot();
            //DrawPic(new Toarupic(new Vector3(GameService.Gw2Mumble.PlayerCharacter.Position.X, GameService.Gw2Mumble.PlayerCharacter.Position.Y, GameService.Gw2Mumble.PlayerCharacter.Position.Z)));
        }
        public void Update(GameTime gameTime)
        {



        }
        public void Unload()
        {

        }
        public void DrawCenterDot()
        {

            controlCollection.Add(new CenterDot());
        }
        public void DrawPic(Control control)
        {
            controlCollection.Add(control);
        }


    }
    public class CenterDot : Control
    {
        Texture2D circleTexture;

        public CenterDot()
        {
            Parent = GameService.Graphics.SpriteScreen;
            // 自己的大小
            Size = new Point(100, 100);
            UpdateLocation(null, null);
            Graphics.SpriteScreen.Resized += UpdateLocation;

            using (var gdctx = GameService.Graphics.LendGraphicsDeviceContext())
            {
                // 幾個像素
                circleTexture = new Texture2D(gdctx.GraphicsDevice, Width, Height);
                Color[] data = new Color[Width * Height];
                int radius = 50;
                Vector2 center = new Vector2(circleTexture.Width / 2, circleTexture.Height / 2);
                for (int x = 0; x < circleTexture.Width; x++)
                {
                    for (int y = 0; y < circleTexture.Height; y++)
                    {
                        Vector2 pixel = new Vector2(x, y);
                        if (Vector2.Distance(pixel, center) <= radius)
                        {
                            data[x + y * circleTexture.Width] = Color.White; // 填充白色
                        }
                    }
                }
                circleTexture.SetData(data);
            }

        }
        private void UpdateLocation(object sender, EventArgs e)
        {
            //
            this.Location = new Point((Graphics.SpriteScreen.Width / 2 - this.Width / 2), (Graphics.SpriteScreen.Height / 2 - this.Height / 2));
        }
        public override void DoUpdate(GameTime gameTime)
        {
            var speed = 100;
            var val = speed * gameTime.ElapsedGameTime.TotalSeconds;
            Size = new Point(Size.X + (int)val, Size.Y + (int)val);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            //dst 設定一個畫布 於 control上 的pos 和大小，texture塞進去 縮放到合適大小
            spriteBatch.DrawOnCtrl(this, circleTexture, new Rectangle(-100, 50, 300, 300), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None);
        }

    }
    public class Toarupic : Control
    {
        VertexPositionColor[] vertices;
        DynamicVertexBuffer vertexBuffer;
        private BasicEffect effect;
        private Vector2 move = new Vector2(150, 300);
        private Vector2 p_move = new Vector2(0, 0);
        private Vector2 p_move2 = new Vector2(1, 0);
        public Toarupic(Vector3 pos)
        {
            Parent = GameService.Graphics.SpriteScreen;
            // 創建頂點緩衝區
            using (var gdctx = GameService.Graphics.LendGraphicsDeviceContext())
            {


                effect = new BasicEffect(gdctx.GraphicsDevice);
                effect.VertexColorEnabled = true;
                effect.Projection = Matrix.CreateOrthographicOffCenter
                    (0, gdctx.GraphicsDevice.Viewport.Width,
                     gdctx.GraphicsDevice.Viewport.Height, 0,
                     0, 1);
                vertices = new VertexPositionColor[] { new VertexPositionColor(new Vector3(0, 0, 0), Color.Blue), new VertexPositionColor(new Vector3(500, 0, 0), Color.Green), new VertexPositionColor(new Vector3(move.X, move.Y, 0), Color.Yellow) };
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
            if (kstate.IsKeyDown(Keys.W))
            {
                p_move.Y -= ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (kstate.IsKeyDown(Keys.S))
            {
                p_move.Y += ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (kstate.IsKeyDown(Keys.A))
            {
                p_move.X -= ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (kstate.IsKeyDown(Keys.D))
            {
                p_move.X += ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }


            if (kstate.IsKeyDown(Keys.D))
            {
                p_move.X += ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (kstate.IsKeyDown(Keys.F))
            {
                p_move2.X += ballSpeed / 10 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            if (kstate.IsKeyDown(Keys.G))
            {
                p_move2.Y += ballSpeed / 10 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            if (kstate.IsKeyDown(Keys.V))
            {
                p_move2.X -= ballSpeed / 10 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            if (kstate.IsKeyDown(Keys.B))
            {
                p_move2.Y -= ballSpeed / 10 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            UpdateVertices();
            UpdateEffect();
        }
        private void UpdateEffect()
        {
            using (var gdctx = GameService.Graphics.LendGraphicsDeviceContext())
            {



                effect.Projection = Matrix.CreateOrthographicOffCenter
                    (p_move.X, gdctx.GraphicsDevice.Viewport.Width,
                     gdctx.GraphicsDevice.Viewport.Height, p_move.Y,
                       p_move2.Y, p_move2.X);

            }
        }
        private void UpdateVertices()
        {
            vertices[2].Position = new Vector3(move.X, move.Y, 0);

            Trace.WriteLine($"X{move.X}");
            Trace.WriteLine($"Y{move.Y}");
            vertexBuffer.SetData(vertices);

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
