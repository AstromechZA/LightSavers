﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LightSavers.ScreenManagement
{
    public class ColourLayer : ScreenLayer
    {
        private Color background;
        private SpriteBatch spriteBatch;
        private Viewport viewport;
        private Texture2D tex;

        public ColourLayer(Color bg) : base()
        {
            background = bg;
            isTransparent = false;
            spriteBatch = new SpriteBatch(Globals.graphics.GraphicsDevice);
            viewport = Globals.graphics.GraphicsDevice.Viewport;
            tex = new Texture2D(Globals.graphics.GraphicsDevice, 1, 1);
            tex.SetData(new Color[] {Color.Red});

            this.transitionOnTime = TimeSpan.FromSeconds(0.6);
            this.transitionOffTime = TimeSpan.FromSeconds(0.5);

            this.fadeOutCompleteCallback = bob;

        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            spriteBatch.Draw(tex, viewport.Bounds, new Color(transitionPercent, transitionPercent, transitionPercent, transitionPercent));

            

            spriteBatch.End();
        }

        public new void Update(GameTime gameTime)
        {   

            base.Update(gameTime);
        }

        public bool bob()
        {
            System.Diagnostics.Debug.WriteLine("derp");
            Globals.screenManager.Push(new TextLayer("Hello World"));
            this.mustExit = true;
            return true;
        }

    }
}
