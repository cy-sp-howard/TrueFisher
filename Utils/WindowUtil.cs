using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BhModule.TrueFisher.Utils
{
    public static class WindowUtil
    {
        public static Notify Notify_ = new Notify() { Parent = GameService.Graphics.SpriteScreen, };
        public class Notify : Control
        {
            public bool showHint = false;
            public Notify()
            {
                Trace.WriteLine("ss");
            }
            protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
            {
                Viewport viewport = spriteBatch.GraphicsDevice.Viewport;
                Vector3 screenPosition = viewport.Project(new Vector3(WorldUtil.GameToWorldCoord(GameService.Gw2Mumble.PlayerCharacter.Position.X), WorldUtil.GameToWorldCoord(GameService.Gw2Mumble.PlayerCharacter.Position.Y), WorldUtil.GameToWorldCoord(GameService.Gw2Mumble.PlayerCharacter.Position.Z)), GameService.Gw2Mumble.PlayerCamera.Projection, GameService.Gw2Mumble.PlayerCamera.View, GameService.Gw2Mumble.PlayerCamera.PlayerView);
                Vector2 spritePosition = new Vector2(screenPosition.X, screenPosition.Y);

                if (!showHint) return;
                spriteBatch.DrawStringOnCtrl(this, "Hello!", GameService.Content.DefaultFont18, new Rectangle((int)screenPosition.X, (int)screenPosition.Y, 50, 40), Color.Magenta);
            }
        }
    }
}