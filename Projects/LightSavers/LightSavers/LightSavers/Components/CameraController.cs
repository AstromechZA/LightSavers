﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightSavers.Components
{
    public class CameraController
    {
        private Camera camera;
        public Camera Camera { get { return camera; } }

        // == Constructors ===
        public CameraController(Viewport v)
        {
            camera = new Camera();
            camera.Aspect = v.AspectRatio;
            camera.NearClip = 0.1f;
            camera.FarClip = 1000;
            camera.Viewport = v;
            camera.Transform = Matrix.Identity;
        }

        public CameraController(Viewport v, Matrix initialTransform)
        {
            camera = new Camera();
            camera.Aspect = v.AspectRatio;
            camera.NearClip = 0.1f;
            camera.FarClip = 1000;
            camera.Viewport = v;

            camera.Transform = initialTransform;
        }
                
        public void HandleInput(float ms)
        {
            // Handle LEFT analog stick (TFGH)
            Vector2 v = Globals.inputController.getAnalogVector(AnalogStick.Left, null);
            if (v.Length() > 0.1f)
            {
                // modifies the horizantal direction
                Vector3 v3 = new Vector3(v.X, 0, -v.Y) * ms / 100;

                Matrix t = camera.Transform;

                t.Translation += v3;

                camera.Transform = t;
            }

            // Handle RIGHT analog stick (IJKL)
            Vector2 v2 = Globals.inputController.getAnalogVector(AnalogStick.Right, null);
            if (v2.Length() > 0.1f)
            {
                // modifies the vertical direction
                Vector3 v3 = new Vector3(0, v2.Y, 0)* ms / 100;

                Matrix t = camera.Transform;

                t.Translation += v3;

                camera.Transform = t;
            }

        }
    }
}
