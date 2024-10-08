﻿using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BhModule.TrueFisher.Utils
{
    public static class WindowUtil
    {
        public static NotifyClass Notify = new NotifyClass();
        public class NotifyClass : Control
        {
            private const float duration = 3000;
            private string message;
            private DateTime msgStartTime = DateTime.Now;
            public NotifyClass()
            {
      

            }
            protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
            {
                if (message == null) return;
                double existTime = (DateTime.Now - msgStartTime).TotalMilliseconds;
                float opacity = (duration - (float)existTime) / duration;
                if (opacity < 0)
                {
                    Clear();
                    return;
                }
                Color textColor = Color.Yellow * opacity;
                spriteBatch.DrawStringOnCtrl(this, message, GameService.Content.DefaultFont32, new Rectangle(0, 0, Width, Height), textColor, false, false, 1, HorizontalAlignment.Center, VerticalAlignment.Middle);

            }
            public void Clear()
            {
                Parent = null;
                message = null;
            }
            public void Show(string text)
            {
                Parent = GameService.Graphics.SpriteScreen;
                Size = new Point(Parent.Size.X, 50);
                Location = new Point(0, Parent.Size.Y / 10 * 2);
                msgStartTime = DateTime.Now;
                message = text;
            }
            protected override CaptureType CapturesInput()
            {
                return CaptureType.None;
            }
        }

    }
}