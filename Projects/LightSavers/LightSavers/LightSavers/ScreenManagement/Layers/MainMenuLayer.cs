﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LightSavers.Utils;
using LightSavers.Components.MenuObjects;

namespace LightSavers.ScreenManagement.Layers
{
    public class MainMenuLayer : ScreenLayer
    {
        private Viewport viewport;
        private RenderTarget2D menu3dscene;
        private SpriteBatch canvas;

        private MenuBackground menuBackground;

        // list of submenues
        public List<Submenu> submenus;
        // current index
        public int currentSubMenuIndex;

        private Color transitionColour;

        private Rectangle titleRect;

        public bool gameRunning;

        /// <summary>
        /// The constructor for the Main Menu.
        /// </summary>
        public MainMenuLayer(bool gameRunning) : base()
        {
            this.gameRunning = gameRunning;
            isTransparent = gameRunning;
            SetLayerAttributes();

            menuBackground = new MenuBackground();

            ConstructDrawingObjects();

            ConstructSubMenus(gameRunning);
        }

        #region == constructor submethods ==

        private void SetLayerAttributes()
        {
            
            transitionOnTime = TimeSpan.FromSeconds(0.6);
            transitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        private void ConstructDrawingObjects()
        {
            viewport = Globals.graphics.GraphicsDevice.Viewport;
            canvas = new SpriteBatch(Globals.graphics.GraphicsDevice);
            menu3dscene = new RenderTarget2D(
                Globals.graphics.GraphicsDevice,
                viewport.Width,
                viewport.Height,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24,
                0,
                RenderTargetUsage.DiscardContents);


            int tx = (viewport.Width - 800) / 2;
            titleRect = new Rectangle(tx, 60, 800, 209);

        }

        private void ConstructSubMenus(bool gameRunning)
        {
            submenus = new List<Submenu>();

            Submenu s0 = new Submenu();
            //s0.items.ElementAt(0)

            if (gameRunning == true)
            {
                s0.AddItem(new DelegateItem("Resume Game", goBack, Color.White, Color.Gray));
                s0.AddItem(new DelegateItem("New Game", RestartQuestion, Color.White, Color.Gray));
            }
            else
            {
                s0.AddItem(new TransitionItem("New Game", 1));
            }

            
            s0.AddItem(new DelegateItem("Controls", OpenControl, Color.White, Color.Gray));
            s0.AddItem(new DelegateItem("Settings", OpenSettings, Color.White, Color.Gray));
            s0.AddItem(new DelegateItem("About", OpenAbout, Color.White, Color.Gray));
            s0.AddItem(new DelegateItem("Exit", ExitQuestion, Color.White, Color.Gray));

            submenus.Add(s0);

            Submenu s1 = new Submenu();
            s1.AddItem(new DelegateItem("Start Game", StartGame, Color.LightBlue, Color.CornflowerBlue));
            s1.AddItem(new ToggleItem("Players", new String[] { "1", "2" }, 0));
            s1.AddItem(new ToggleItem("Level Length", new String[] { "Short", "Medium", "Tiring" }, 0));
            s1.AddItem(new ToggleItem("Difficulty", new String[] { "Easy", "Medium", "Hard" }, 0));

            submenus.Add(s1);

            currentSubMenuIndex = 0;

        }

        #endregion

        /// <summary>
        /// The main draw method called by the ScreenManager
        /// </summary>
        public override void Draw()
        {

            // Draw the layers
            canvas.Begin();
            
            if (gameRunning == false)
            {
                // Draw the 3d background
               // Console.WriteLine("Bool gameRunning: " + gameRunning);
                canvas.Draw(menu3dscene, viewport.Bounds, Color.White);
                menuBackground.Draw(canvas);
                canvas.Draw(AssetLoader.title2, titleRect, Color.White);
            }
            else
            {
                isTransparent = true;
                Color talpha = new Color(1.0f, 1.0f, 1.0f, 0.7f);
                canvas.Draw(AssetLoader.tex_black, viewport.Bounds, talpha);
                canvas.Draw(AssetLoader.paused, titleRect, Color.White);
            }

            Draw2DLayers();

            // finish drawing
            canvas.End();

            transitionColour = new Color(0, 0, 0);
        }

        #region == drawing submethods ==


        private void Draw2DLayers()
        {
            submenus[currentSubMenuIndex].Draw(canvas, 60, 400);
            
            if (state == ScreenState.TransitioningOff || state == ScreenState.TransitioningOn)
            {
                int trans = (int)((1 - transitionPercent) * 255.0f);
                transitionColour = new Color(trans, trans, trans, trans);
                canvas.Draw(AssetLoader.tex_black, viewport.Bounds, transitionColour);
            }
        }


        #endregion

        /// <summary>
        /// The main update method, called by the ScreenManager
        /// </summary>
        public override void Update(float ms)
        {
            if (this.state == ScreenState.Active)
            {
                CheckControls();
            }

            if (gameRunning == true && isTransparent!= true)
            {
                isTransparent = true;
            }

            base.Update(ms);
        }

        #region == update submethods ==

        private void CheckControls()
        {
            //back button is pressed
            if (Globals.inputController.isButtonPressed(Buttons.B, null))
            {
                if (currentSubMenuIndex == 0 && Globals.screenManager.layers.Count == 1)
                {
                    endGame();
                }
                else if (Globals.screenManager.layers.Count > 1)
                {

                    this.fadeOutCompleteCallback = goBack;
                    this.StartTransitionOff();
                }
                else
                {
                    currentSubMenuIndex = 0;
                }
                
            }
            
            //select (enter)
            else if (Globals.inputController.isButtonPressed(Buttons.A, null))
            {
                //Globals.audioManager.PlayMenuSound("menu_select");
                /*
                */
                if (submenus[currentSubMenuIndex].items[submenus[currentSubMenuIndex].selected] is TransitionItem)
                {
                    TransitionItem current = (TransitionItem)submenus[currentSubMenuIndex].items[submenus[currentSubMenuIndex].selected];
                    int destination = current.destination;
                    if (destination == -1)
                    {
                        this.fadeOutCompleteCallback = ExitQuestion;
                        this.StartTransitionOff();
                    }
                        
                    else
                    {
                        currentSubMenuIndex = destination;
                    }
                }
                else if(submenus[currentSubMenuIndex].items[submenus[currentSubMenuIndex].selected] is DelegateItem)
                {
                    DelegateItem current = (DelegateItem)submenus[currentSubMenuIndex].items[submenus[currentSubMenuIndex].selected];

                    this.fadeOutCompleteCallback = current.function;
                    this.StartTransitionOff();
                }
            }

            //GOING UP IN MENUS
            else if (Globals.inputController.isButtonPressed(Buttons.DPadUp, null) || Globals.inputController.isButtonPressed(Buttons.LeftThumbstickUp, null))
            {
                int selectedIndex = submenus[currentSubMenuIndex].selected;
                int size = submenus[currentSubMenuIndex].items.Count();
                int newIndex = selectedIndex-1;

                if (newIndex < 0)
                {
                    newIndex = size-1;
                }

                submenus[currentSubMenuIndex].selected = newIndex;
            }
            
            //GOING DOWN IN MENUS
            else if (Globals.inputController.isButtonPressed(Buttons.DPadDown, null) || Globals.inputController.isButtonPressed(Buttons.LeftThumbstickDown, null))
            {
                int selectedIndex = submenus[currentSubMenuIndex].selected;
                int size = submenus[currentSubMenuIndex].items.Count();
                int newIndex = ++selectedIndex;

                if (newIndex == size)
                {
                    newIndex = 0;
                }

                submenus[currentSubMenuIndex].selected = newIndex;
            }
            
            //GOING LEFT IN MENUS (TOGGLE)
            else if (Globals.inputController.isButtonPressed(Buttons.DPadLeft, null) || Globals.inputController.isButtonPressed(Buttons.LeftThumbstickLeft, null))
            {
                if (submenus[currentSubMenuIndex].items[submenus[currentSubMenuIndex].selected] is ToggleItem)
                {
                    ToggleItem currentToggle = (ToggleItem)submenus[currentSubMenuIndex].items[submenus[currentSubMenuIndex].selected];

                    int selectedIndex = currentToggle.current;
                    int size = currentToggle.values.Count();
                    int newIndex = selectedIndex - 1;

                    if (newIndex < 0)
                    {
                        newIndex = size - 1;
                    }
                    currentToggle.current = newIndex;
                }                
            }

            //GOING RIGHT IN MENUS (TOGGLE)
            else if (Globals.inputController.isButtonPressed(Buttons.DPadRight, null) || Globals.inputController.isButtonPressed(Buttons.LeftThumbstickRight, null))
            {
                if (submenus[currentSubMenuIndex].items[submenus[currentSubMenuIndex].selected] is ToggleItem)
                {
                    ToggleItem currentToggle = (ToggleItem)submenus[currentSubMenuIndex].items[submenus[currentSubMenuIndex].selected];

                    int selectedIndex = currentToggle.current;
                    int size = currentToggle.values.Count();
                    int newIndex = ++selectedIndex;

                    if (newIndex == size)
                    {
                        newIndex = 0;
                    }
                    currentToggle.current = newIndex;
                }
            }
        }
        #endregion
        public bool StartGame()
        {
            //sets player choices for xbox
            //windows will always defalut to 1
            ToggleItem playerToggle = (ToggleItem)submenus[1].items[1];
            ToggleItem LengthToggle = (ToggleItem)submenus[1].items[2];
            ToggleItem DiffToggle = (ToggleItem)submenus[1].items[3];

            GameLayer temp = new GameLayer(playerToggle.current + 1, LengthToggle.current + 1, DiffToggle.current + 1);

            Globals.screenManager.Push(new IntroStory(temp));
                                         
            return true;
        }

        public bool OpenControl()
        {
            //Globals.screenManager.Pop();
            Globals.screenManager.Push(new ControlScreenLayer());
            return true;
        }

        public bool OpenAbout()
        {
           // Globals.screenManager.Pop();
            Globals.screenManager.Push(new AboutScreenLayer());
            return true;
        }

        public bool endGame()
        {
            // Globals.screenManager.Pop();
            while (Globals.screenManager.layers.Count > 0)
            {
                Globals.screenManager.Pop();
            }
            return true;
        }

        public bool goBack()
        {
            Globals.audioManager.SwitchToGame();
            Globals.screenManager.Pop();
            return true;
        }

        public bool OpenSettings()
        {
            // Globals.screenManager.Pop();
            Globals.screenManager.Push(new SettingsMenuLayer());
            return true;
        }

        public bool ExitQuestion()
        {
            // Globals.screenManager.Pop();
            Globals.screenManager.Push(new AreYouSureLayer("\n  Are you sure you want to exit?", 0));
            return true;
        }

        public bool RestartQuestion()
        {
            // Globals.screenManager.Pop();
            Globals.screenManager.Push(new AreYouSureLayer("Are you sure you want to restart?\nYou will lose all progress in the\ncurrent game.", 1));
            return true;
        }
    }
}
