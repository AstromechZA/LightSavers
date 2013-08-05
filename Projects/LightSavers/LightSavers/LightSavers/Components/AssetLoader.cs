﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using LightSavers.ScreenManagement;
using System.Threading;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace LightSavers
{
    /** AssetLoader layer ***
     * loads larger assets at the beginning of the game with nice fade in / out and progress bar.
     * To add a new asset, add a static variable for it and add it to the LoadAssets method. 
     * 
     * Naming convention:
     *   Model -> mdl_relevantname
     *   Texture -> tex_relevantname
     *   Sound -> snd_relevantname
     *   Font -> fnt_relevantname
     * 
     ************************/ 
    public class AssetLoader : ScreenLayer
    {
        /************* ASSETS **************/
        public static Model mld_combine;
        public static Texture2D tex_black;
        public static Texture2D tex_white;
        public static Texture2D tex_sand;
        public static Model mdl_menuscene;
        /***********************************/

        private String loading_msg = "Loading Assets";
        private int loading_bar_length = 0;
        private int loaded_assets = 0;
        private int num_assets = -1;            

        private Viewport viewport;
        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private int drawingY;

        public AssetLoader() : base()
        {
            spriteBatch = new SpriteBatch(Globals.graphics.GraphicsDevice);
            viewport = Globals.graphics.GraphicsDevice.Viewport;

            // Assets required to view the loading screen
            spriteFont = Globals.content.Load<SpriteFont>("fonts/LoadingFont");
            tex_black = new Texture2D(Globals.graphics.GraphicsDevice, 1, 1);
            tex_black.SetData(new Color[] { Color.Black });
            tex_white = new Texture2D(Globals.graphics.GraphicsDevice, 1, 1);
            tex_white.SetData(new Color[] { Color.White });

            // Fade times
            transitionOnTime = TimeSpan.FromSeconds(0.6);
            transitionOffTime = TimeSpan.FromSeconds(0.5);      
     
            // vertical position
            drawingY = (int)(viewport.Height * 0.75f);
        }

        public bool Start()
        {
            Thread t = new Thread(new ThreadStart(LoadAssets));
            t.Start();
            return true;
        }

        // Asset loading thread
        // Each asset updates the message and increments the load count
        private void LoadAssets()
        {
            // number of assets to be loaded. (used to compute progress bar size)
            num_assets = 3;

            // assets
            mld_combine = loadModel("combine");
            tex_sand = loadTexture("sand");
            mdl_menuscene = loadModel("menuscene");

            // once its loaded, fade out
            StartTransitionOff();
        }

        private Model loadModel(String modelfile)
        {
            loading_msg = String.Format("Loading model '{0}'", modelfile);
            Stopwatch s = new Stopwatch(); s.Start();
            Model m = Globals.content.Load<Model>(modelfile);
            System.Diagnostics.Debug.WriteLine(String.Format("Loading model: '{0}' took {1}ms", modelfile, s.ElapsedMilliseconds));
            loaded_assets += 1;
            return m;
        }

        private Texture2D loadTexture(String texfile)
        {
            loading_msg = String.Format("Loading texture '{0}'", texfile);
            Stopwatch s = new Stopwatch(); s.Start();
            Texture2D t = Globals.content.Load<Texture2D>(texfile);
            System.Diagnostics.Debug.WriteLine(String.Format("Loading texture: '{0}' took {1}ms", texfile, s.ElapsedMilliseconds));
            loaded_assets += 1;
            return t;
        }

        // Draw the layer
        // A black background + text + progress bar
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(tex_black, viewport.Bounds, new Color(transitionPercent, transitionPercent, transitionPercent, transitionPercent));

            spriteBatch.DrawString(spriteFont, loading_msg, new Vector2((viewport.Width - spriteFont.MeasureString(loading_msg).X) / 2, drawingY), Color.White);

            spriteBatch.Draw(tex_white, new Rectangle(0, drawingY + 50-2, viewport.Width, 6), Color.Gray); 

            spriteBatch.Draw(tex_white, new Rectangle(0, drawingY + 50, loading_bar_length, 2), Color.White); 

            spriteBatch.End();
        }

        // Update the size of the loading bar
        public override void Update(GameTime gameTime)
        {
            loading_bar_length = (int)((loaded_assets / (float)num_assets) * viewport.Width);
            base.Update(gameTime);
        }
        

        
    }
}
