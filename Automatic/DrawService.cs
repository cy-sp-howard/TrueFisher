using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD;
using Blish_HUD.Controls;
using BhModule.TrueFisher.Utils;

namespace BhModule.TrueFisher.Automatic
{
    public class DrawService
    {


        // 基本效果
        private TrueFisherModule module;
        private Blish_HUD.Modules.Module pathingModule { get => GameService.Module.Modules.ToList().Find(i => i.ModuleInstance.Name == "Pathing")?.ModuleInstance; }

        private List<Control> controlCollection = new List<Control>();
        private List<Hole> nodes = new List<Hole>();
        private double nextScanTick = 0;
        public DrawService(TrueFisherModule module)
        {
            this.module = module;
            //DateTime now = DateTime.Now;
            //MEMORY_BASIC_INFORMATION mbi = new MEMORY_BASIC_INFORMATION();
            //var handle = MemUtil.OpenProcess(0x400, false, GameService.GameIntegration.Gw2Instance.Gw2Process.Id);
            //MemUtil.VirtualQueryEx(handle, IntPtr.Zero, out mbi, Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
            //var a = MemUtil.FindPattern("65 48 8b 04 25 58 00 00 00 ba 08 00 00 00", GameService.GameIntegration.Gw2Instance.Gw2Process);
            //Trace.WriteLine((DateTime.Now - now).TotalSeconds);
            //Trace.WriteLine("11");
            //DrawCenterDot();
        }
        public void Update(GameTime gameTime)
        {
            if (gameTime.TotalGameTime.TotalMilliseconds > nextScanTick)
            {
                nextScanTick = gameTime.TotalGameTime.TotalMilliseconds + 5000;
                //getResources();
                getModels();
            }
            float scaleRate = GameService.Graphics.UIScaleMultiplier; // screen.width * scake = window.width
            foreach (var dot in controlCollection)
            {
                dot.Parent = null;
            }
            controlCollection.Clear();
            foreach (var node in nodes)
            {
                var dot = new Dot();
                controlCollection.Add(dot);
                dot.Location = new Point((int)(node.ScreenPos.X / scaleRate), (int)(node.ScreenPos.Y / scaleRate));
            }

        }
        public void Unload()
        {

        }
        void getResources()
        {
            nodes.Clear();
            IntPtr currentNode = DataService.Read<IntPtr>(NodeMem.Start).value;
            IntPtr nodeEnd = DataService.Read<IntPtr>(NodeMem.End).value;

            while (currentNode.ToInt64() < nodeEnd.ToInt64())
            {
                byte[] posBytes = MemUtil.ReadMem(DataService.Handle, currentNode, 12, [0xe8]).value;
                float x = BitConverter.ToSingle(posBytes, 0);
                float y = BitConverter.ToSingle(posBytes, 4);
                float z = BitConverter.ToSingle(posBytes, 8) * -1;
                var node = new Hole(currentNode, new(x, y, z));
                if (node.Distance <= 5000)
                {
                    nodes.Add(node);
                }
                currentNode = IntPtr.Add(currentNode, 0x8);
            }
            // prepare next scan
            DataService.Write(FishMem.Scanned, [0]);
        }
        void getModels()
        {
            nodes.Clear();
            IntPtr currentNode = DataService.Read<IntPtr>(ModelMem.Start).value;
            IntPtr nodeEnd = DataService.Read<IntPtr>(ModelMem.End).value;

            while (currentNode.ToInt64() < nodeEnd.ToInt64())
            {
                byte[] posBytes = MemUtil.ReadMem(DataService.Handle, currentNode, 12, [0x1b0,0x8,0x30]).value;
                float x = BitConverter.ToSingle(posBytes, 0);
                float y = BitConverter.ToSingle(posBytes, 4);
                float z = BitConverter.ToSingle(posBytes, 8) * -1;
                var node = new Hole(currentNode, new(x / 0.0254f, y / 0.0254f, z / 0.0254f));
                nodes.Add(node);
                currentNode = IntPtr.Add(currentNode, 0x8);
            }
            // prepare next scan
            DataService.Write(FishMem.Scanned, [0]);
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
    public class Dot : Control
    {
        public Dot()
        {
            Parent = GameService.Graphics.SpriteScreen;
            Size = new Point(5, 5);

        }
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, 0, Size.X, Size.Y), Color.Yellow);
        }
        protected override CaptureType CapturesInput()
        {
            return CaptureType.None;
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
}
