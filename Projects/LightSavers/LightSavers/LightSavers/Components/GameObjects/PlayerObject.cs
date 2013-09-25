﻿using LightPrePassRenderer;
using LightPrePassRenderer.partitioning;
using LightSavers.Utils;
using Microsoft.Xna.Framework;
using SkinnedModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightSavers.Components.GameObjects
{
    public class PlayerObject : GameObject
    {
        #region CONSTANTS
        // Player orientation
        float PLAYER_YORIGIN = 0.8f;
        float PLAYER_SCALE = 0.6f;

        // Light stuff
        float TORCH_HEIGHT = 1.6f;
        float HALO_HEIGHT = 6.0f;
        #endregion



        private PlayerIndex playerIndex;
        private Color color;

        private SkinnedMesh mesh;

        // Transform info
        private Vector3 position;

        private float rotation;

        // Lighting variables
        private Light torchlight;
        private Light halolight;
        private Light haloemitlight;

        private AnimationPlayer aplayer;

        private RealGame game;

        // Scenegraphstuff
        private MeshSceneGraphReceipt modelReceipt;
        private LightSceneGraphReceipt light1receipt;
        private LightSceneGraphReceipt light2receipt;
        private LightSceneGraphReceipt light3receipt;

        public PlayerObject(RealGame game, PlayerIndex playerIndex, Color color, Vector3 pos, float initialYRot)
        {
            this.game = game;
            this.playerIndex = playerIndex;
            this.color = color;

            // initial transform
            position = pos;
            rotation = initialYRot;

            mesh = new SkinnedMesh();
            mesh.Model = AssetLoader.mdl_character;

            aplayer = new AnimationPlayer(mesh.SkinningData);
            aplayer.StartClip(mesh.SkinningData.AnimationClips["Take 001"]);

            SetupLights();
            UpdateTransform(0);
        }

        private void UpdateTransform(float ms)
        {
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, (int) ms);

            aplayer.Update(ts, true, Matrix.Identity);
            mesh.BoneMatrixes = aplayer.GetSkinTransforms();

            mesh.Transform = Matrix.CreateScale(PLAYER_SCALE) * Matrix.CreateRotationY(rotation+(float)Math.PI) * Matrix.CreateTranslation(position + new Vector3(0, PLAYER_YORIGIN, 0));
            
            halolight.Transform = Matrix.CreateRotationX(MathHelper.ToRadians(-90)) * Matrix.CreateTranslation(position + new Vector3(0, HALO_HEIGHT, 0));
            haloemitlight.Transform = Matrix.CreateRotationX(MathHelper.ToRadians(-90)) * Matrix.CreateTranslation(position + new Vector3(0, 2, 0));
            torchlight.Transform = Matrix.CreateTranslation(new Vector3(0, 0, 0.0f)) * Matrix.CreateRotationX(-0.2f) * Matrix.CreateRotationY(rotation) * Matrix.CreateTranslation(position + new Vector3(0, TORCH_HEIGHT, 0));
            
        }

        public override void Update(float ms)
        {

            Vector3 newposition = new Vector3(position.X, position.Y, position.Z);

            // 1. == Update Movement
            // movement is done via analog sticks
            Vector2 vleft = Globals.inputController.getAnalogVector(AnalogStick.Left, playerIndex);
            Vector2 vright = Globals.inputController.getAnalogVector(AnalogStick.Right, playerIndex);
            // 1.1 = Update player rotation based on RIGHT analog stick
            if (vright.Length() > 0.01f)
            {
                // get target angle
                float targetrotation = (float)Math.Atan2(vright.Y, vright.X) - MathHelper.PiOver2;

                // get difference
                float deltarotation = targetrotation - rotation;

                // sanitise
                if (deltarotation > Math.PI)
                    deltarotation -= MathHelper.TwoPi;
                if (deltarotation < -Math.PI)
                    deltarotation += MathHelper.TwoPi;

                // add difference
                rotation += 0.1f * deltarotation;

                // sanitise rotation
                if (rotation > MathHelper.TwoPi) rotation -= MathHelper.TwoPi;
                if (rotation < -MathHelper.TwoPi) rotation += MathHelper.TwoPi;
            }

            // 1.2 = Update player movement based on LEFT analog stick
            if (vleft.Length() > 0.1f)
            {
                Vector3 pdelta = new Vector3(vleft.X, 0, -vleft.Y);
                pdelta.Normalize();
                // modifies the horizantal direction
                newposition += pdelta * ms / 300;

                // 1.3 = If no rotation was changed, pull player angle toward forward vector
                if (vright.Length() < 0.1f)
                {
                    // get target angle
                    float targetrotation = (float)Math.Atan2(vleft.Y, vleft.X) - MathHelper.PiOver2;

                    // get difference
                    float deltarotation = targetrotation - rotation;

                    // sanitise
                    if (deltarotation > Math.PI)
                        deltarotation -= MathHelper.TwoPi;
                    if (deltarotation < -Math.PI)
                        deltarotation += MathHelper.TwoPi;

                    // add difference
                    rotation += 0.1f * deltarotation;

                    // sanitise rotation
                    if (rotation > MathHelper.TwoPi) rotation -= MathHelper.TwoPi;
                    if (rotation < -MathHelper.TwoPi) rotation += MathHelper.TwoPi;

                }


            }

            // collision stuff
            if (position != newposition)
            {
                Vector3 cd = new Vector3(position.X, 0, position.Z);
                if (newposition.X < position.X) cd.X -= 0.2f;
                else if (newposition.X > position.X) cd.X += 0.2f;


                if (newposition.Z < position.Z) cd.Z -=0.2f;
                else if (newposition.Z > position.Z) cd.Z += 0.2f;

                if (game.cellCollider.GetCollision(cd.X, position.Z))
                {
                    newposition.X = position.X;
                }

                if (game.cellCollider.GetCollision(position.X, cd.Z))
                {
                    newposition.Z = position.Z;
                }

                if (position != newposition)
                {

                    position = newposition;
                    UpdateTransform(ms);

                    // check if it has moved into another box
                    int oldx = (int)modelReceipt.oldGlobalTransform.Translation.X / 32;
                    int newx = (int)mesh.Transform.Translation.X / 32;

                    if (oldx != newx)
                    {
                        modelReceipt.parentlist.Remove(mesh);
                        light1receipt.parentlist.Remove(torchlight);
                        light2receipt.parentlist.Remove(halolight);
                        light3receipt.parentlist.Remove(haloemitlight);

                        AddToSG(game.sceneGraph);
                    }
                }


            }




            

        }

        public void SetupLights()
        {

            torchlight = new Light();
            torchlight.LightType = Light.Type.Spot;
            torchlight.ShadowDepthBias = 0.002f;
            torchlight.Radius = 15;
            torchlight.SpotAngle = 25;
            torchlight.Intensity = 1.0f;
            torchlight.SpotExponent = 6;
            torchlight.Color = color*1.1f;
            torchlight.CastShadows = true;
            torchlight.Transform = Matrix.Identity;

            halolight = new Light();
            halolight.LightType = Light.Type.Spot;
            halolight.ShadowDepthBias = 0.001f;
            halolight.Radius = HALO_HEIGHT+1;
            halolight.SpotAngle = 35;
            halolight.SpotExponent = 0.6f;
            halolight.Intensity = 0.8f;
            halolight.Color = color;
            halolight.CastShadows = true;
            halolight.Transform = Matrix.Identity;

            haloemitlight = new Light();
            haloemitlight.LightType = Light.Type.Point;
            haloemitlight.Intensity = 0.4f;
            haloemitlight.Radius = 3;
            haloemitlight.Color = color * 1.4f;
            haloemitlight.Transform = Matrix.Identity;
            haloemitlight.SpecularIntensity = 3;

        }


        public Light[] GetLights()
        {
            return new Light[] { 
                torchlight, 
                halolight, 
                haloemitlight 
            };
        }
        public Mesh GetMesh()
        {
            return mesh;
        }

        public override RectangleF GetBoundRect()
        {
            return new RectangleF();
        }

        public void AddToSG(AwesomeSceneGraph lightAndMeshContainer)
        {
            modelReceipt = game.sceneGraph.AddMesh(mesh);
            light1receipt = game.sceneGraph.AddLight(torchlight);
            light2receipt = game.sceneGraph.AddLight(halolight);
            light3receipt = game.sceneGraph.AddLight(haloemitlight);
        }
    }
}
