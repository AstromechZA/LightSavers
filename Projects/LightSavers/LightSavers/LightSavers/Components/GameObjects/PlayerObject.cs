﻿using LightPrePassRenderer;
using LightPrePassRenderer.partitioning;
using LightSavers.Components.Guns;
using LightSavers.Components.Projectiles;
using LightSavers.Utils;
using LightSavers.Utils.Geometry;
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
        const float PLAYER_YORIGIN = 1f;
        const float PLAYER_SCALE = 0.75f;

        // Light stuff
        const float TORCH_HEIGHT = 2.5f;
        const float TORCH_ANGLE = 30.0f;
        const float HALO_HEIGHT = 6.0f;

        Matrix mPlayerScale = Matrix.CreateScale(PLAYER_SCALE);
        Matrix mHaloPitch = Matrix.CreateRotationX(-90);
        Matrix mTorchPitch = Matrix.CreateRotationX(-0.4f);
        const float boundingBoxSize = 0.45f;
        #endregion

        private PlayerIndex playerIndex;
        private Color color;
        public Color PlayerColour { get { return color; } }

        private SkinnedMesh mesh;

        public RectangleF collisionRectangle;

        private float rotation;

        // Lighting variables
        private Light torchlight;
        private Light halolight;
        private Light haloemitlight;       

        private DurationBasedAnimator upPlayer;
        private DurationBasedAnimator lowPlayer;
        
        // Scenegraphstuff
        private MeshSceneGraphReceipt modelReceipt;
        private LightSceneGraphReceipt light1receipt;
        private LightSceneGraphReceipt light2receipt;
        private LightSceneGraphReceipt light3receipt;

        private BaseGun[] weapons;
        public int currentWeapon;
        private int currentAnimation;
        private int currentFiringAnimation;
        int moving=0, weapon=Animation_States.pistol, shooting=0;

        //basic default health is always 100
        public double health = 100.0;
        public double oldHealth = 100.0;

        //state
        public bool alive = true;
        public bool dead = false;

        public PlayerObject(PlayerIndex playerIndex, Color color, Vector3 pos, float initialYRot)
        {
            this.playerIndex = playerIndex;
            this.color = color;

            // initial transform
            this._position = pos;
            rotation = initialYRot;

            SetupLights();

            mesh = new SkinnedMesh();
            mesh.Model = AssetLoader.mdl_character;

            //Create a new Animation Player that will take the bone dictionaries as arguments allowing individual animation with upper and lower body
            upPlayer = new DurationBasedAnimator(mesh.SkinningData, mesh.SkinningData.AnimationClips["Take 001"],  Animation_States.upperCharacterBones);
            lowPlayer = new DurationBasedAnimator(mesh.SkinningData, mesh.SkinningData.AnimationClips["Take 001"], Animation_States.lowerCharacterBonesandRoot);
            //Load the animations from the asset loader (these are in an Animation Package)
            upPlayer.AddAnimationPackage = AssetLoader.ani_character;
            upPlayer.StartClip(moving + shooting + weapon);
            lowPlayer.AddAnimationPackage = AssetLoader.ani_character;
            lowPlayer.StartClip(moving+weapon);
            

            UpdateAnimation(0);
            UpdateMajorTransforms(0);

            Globals.gameInstance.sceneGraph.Setup(mesh);
            modelReceipt = Globals.gameInstance.sceneGraph.Add(mesh);
            light1receipt = Globals.gameInstance.sceneGraph.Add(torchlight);
            light2receipt = Globals.gameInstance.sceneGraph.Add(haloemitlight);
            light3receipt = Globals.gameInstance.sceneGraph.Add(halolight);

            SetupWeapons();
            SwitchWeapon(0);

            collisionRectangle = new RectangleF(
                _position.X - boundingBoxSize,
                _position.Y - boundingBoxSize,
                boundingBoxSize * 2,
                boundingBoxSize * 2
            );

        }

        private void UpdateAnimation(float ms)
        {
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, (int) ms);
            lowPlayer.Update(ts, true, Matrix.Identity, Matrix.Identity);
            upPlayer.Update(ts, true, Matrix.Identity, lowPlayer.GetWorldTransforms()[0]);
            
            mesh.BoneMatrixes = lowPlayer.MergeTransforms(upPlayer.GetSkinTransforms());
        }

        private void UpdateMajorTransforms(float ms)
        {
            mesh.Transform = mPlayerScale * Matrix.CreateRotationY(rotation + (float)Math.PI) * Matrix.CreateTranslation(_position + new Vector3(0, PLAYER_YORIGIN, 0));

            halolight.Transform = mHaloPitch * Matrix.CreateTranslation(_position + new Vector3(0, HALO_HEIGHT, 0));
            haloemitlight.Transform = mHaloPitch * Matrix.CreateTranslation(_position + new Vector3(0, 2, 0));
            torchlight.Transform = mTorchPitch * Matrix.CreateTranslation(0, 0, 0.3f) * Matrix.CreateRotationY(rotation) * Matrix.CreateTranslation(_position + new Vector3(0, TORCH_HEIGHT, 0));
        }

        public override void Update(float ms)
        {
            if (health <= 0)
            {
                health = 0;
                this.alive = false;

                if (currentFiringAnimation != Animation_States.death)
                {
                    currentFiringAnimation = Animation_States.death;
                    upPlayer.StartClip(currentFiringAnimation);
                    lowPlayer.StartClip(currentFiringAnimation);
                }
                else if (upPlayer.GetLoopCount() > 1)
                {
                    this.dead = true;
                }
               
                UpdateAnimation(ms);
                UpdateMajorTransforms(ms);
                
                
            }
            else
            {
                IProjectile p = Globals.gameInstance.alienProjectileManager.CheckHit(this);
                if (p != null)
                {
                    p.PreDestroy();
                    p.Destroy();
                    this.health -= p.GetDamage();
                    //if (this._health <= 0)
                    //{
                    //    this._state = AlienState.DYING;
                    //    this._aplayer.StartClip(Animation_States.death);
                    //    Globals.audioManager.PlayGameSound("aliendeath1");
                    //}
                }

                double temp = this.oldHealth;
                this.oldHealth = this.health;

                if (temp == health && health >= 0 && health <100)
                {
                    this.health = this.health + ((ms / 1000) * 0.6);
                }
                Vector3 newposition = new Vector3(_position.X, _position.Y, _position.Z);

                // 1. == Update Movement
                // movement is done via analog sticks
                Vector2 vleft = Globals.inputController.getAnalogVector(AnalogStick.Left, playerIndex);
                Vector2 vright = Globals.inputController.getAnalogVector(AnalogStick.Right, playerIndex);
                // 1.1 = Update player rotation based on RIGHT analog stick
                if (vright.LengthSquared() > 0.01f)
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
                    rotation += (ms / 300) * deltarotation;

                    // sanitise rotation
                    if (rotation > MathHelper.TwoPi) rotation -= MathHelper.TwoPi;
                    if (rotation < -MathHelper.TwoPi) rotation += MathHelper.TwoPi;
                }

                // 1.2 = Update player movement based on LEFT analog stick
                if (vleft.LengthSquared() > 0.1f)
                {
                    Vector3 pdelta = new Vector3(vleft.X, 0, -vleft.Y);
                    pdelta.Normalize();
                    // modifies the horizantal direction
                    newposition += pdelta * ms / 300;

                    // 1.3 = If no rotation was changed, pull player angle toward forward vector
                    if (vright.LengthSquared() < 0.01f)
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
                        rotation += (ms / 300) * deltarotation;

                        // sanitise rotation
                        if (rotation > MathHelper.TwoPi) rotation -= MathHelper.TwoPi;
                        if (rotation < -MathHelper.TwoPi) rotation += MathHelper.TwoPi;
                    }

                    moving = Animation_States.run;
                }
                else
                {
                    moving = Animation_States.idle;
                }

                if (Globals.inputController.isTriggerDown(Triggers.Right, playerIndex))
                {

                    if (currentWeapon > -1 && weapons[currentWeapon].CanFire())
                    {
                        weapons[currentWeapon].Fire(rotation + MathHelper.PiOver2);
                        shooting = Animation_States.shoot;
                    }

                }

                if(Globals.inputController.isButtonPressed(Microsoft.Xna.Framework.Input.Buttons.Y, playerIndex))
                {
                    int nw = (currentWeapon + 1) % 5;
                
                    SwitchWeapon(nw);
                }

                if (Globals.inputController.isButtonPressed(Microsoft.Xna.Framework.Input.Buttons.X, playerIndex))
                {
                    // check game section
                    WeaponDepot wd = Globals.gameInstance.campaignManager.GetNearestActiveDepot(_position);
                    if (wd != null)
                    {
                        SwitchWeapon(wd.GetIndex());
                        wd.Deactivate();
                    }
                }


                // collision stuff
                if (_position != newposition)
                {
                    // First test X collision
                    // Xmovement can collide with walls or doors
                    // first check the doors, this is fast
                    collisionRectangle.Left = newposition.X - boundingBoxSize;
                    collisionRectangle.Top = _position.Z - boundingBoxSize;
                    if (Globals.gameInstance.cellCollider.RectangleCollides(collisionRectangle))
                    {
                        // if it does collide, pull it back
                        newposition.X = _position.X;
                    }

                    if (Globals.gameInstance.campaignManager.RectangleCollidesDoor(collisionRectangle))
                    {
                        newposition.X = _position.X;
                    }

                    // Then test Z collision
                    collisionRectangle.Left = _position.X - boundingBoxSize;
                    collisionRectangle.Top = newposition.Z - boundingBoxSize;
                    if (Globals.gameInstance.cellCollider.RectangleCollides(collisionRectangle))
                    {
                        // if it does collide, pull it back
                        newposition.Z = _position.Z;
                    }

                    collisionRectangle.Left = newposition.X - boundingBoxSize;
                    collisionRectangle.Top = newposition.Z - boundingBoxSize;
                    if (Globals.gameInstance.campaignManager.CollideCurrentEntities(this))
                    {
                        newposition = _position;
                    }

                    //lastly check the distance between players
                    if (Globals.gameInstance.players.Length > 1)
                    {
                        int otherPlayer = Math.Abs((int)playerIndex - 1);
                        if (Math.Abs(Globals.gameInstance.players[otherPlayer]._position.X - newposition.X) > CameraController.MAX_DISTANCE_BETWEEN_PLAYERS)
                        {
                            newposition.X = _position.X;
                        }
                    }

                    // if there is still a new position
                    if (_position != newposition)
                    {
                        _position = newposition;

                        modelReceipt.graph.Renew(modelReceipt);
                        light1receipt.graph.Renew(light1receipt);
                        light2receipt.graph.Renew(light2receipt);
                        light3receipt.graph.Renew(light3receipt);
                    }
                }

                UpdateAnimation(ms);
                UpdateMajorTransforms(ms);

                if (currentWeapon > -1)
                {
                    weapons[currentWeapon].SetTransform(upPlayer.GetWorldTransforms()[31], mesh.Transform);
                    weapons[currentWeapon].receipt.graph.Renew(weapons[currentWeapon].receipt);

                    if (weapons[currentWeapon].AnimationComplete()) shooting = 0;

                }
                    // Update Top half of body
                    if (currentFiringAnimation != moving + shooting + weapon)
                    {
                        currentFiringAnimation = moving + shooting + weapon;
                        upPlayer.StartClip(currentFiringAnimation);
                    }

                    //Update Bottom half of body
                    if (currentAnimation != moving + weapon)
                    {
                        currentAnimation = moving + weapon;
                        lowPlayer.StartClip(currentAnimation);
                    }
            }
            
        }

        public void SetupWeapons()
        {
            weapons = new BaseGun[5];
            weapons[0] = new Pistol();
            Globals.gameInstance.sceneGraph.Setup(weapons[0].mesh);
            weapons[1] = new Shotgun();
            Globals.gameInstance.sceneGraph.Setup(weapons[1].mesh);
            weapons[2] = new AssaultRifle();
            Globals.gameInstance.sceneGraph.Setup(weapons[2].mesh);
            weapons[3] = new SniperRifle();
            Globals.gameInstance.sceneGraph.Setup(weapons[3].mesh);
            weapons[4] = new Sword();
            Globals.gameInstance.sceneGraph.Setup(weapons[4].mesh);

            currentWeapon = -1;
            
        }

        public void SwitchWeapon(int to)
        {
            //Switch weapon animations
            if (to == 1 || to == 3)
                weapon = Animation_States.snipshot;
            else if (to == 0)
                weapon = Animation_States.pistol;
            else if (to == 2)
                weapon = Animation_States.assault;
            else if (to == 4)
                weapon = Animation_States.sword;

            //disable current weapon
            if (currentWeapon > -1)
            {
                Globals.gameInstance.sceneGraph.Remove(weapons[currentWeapon].receipt);
            }

            currentWeapon = to;
            BaseGun g = weapons[to];
            g.SetTransform(upPlayer.GetWorldTransforms()[31], mesh.Transform);

            if (g.receipt != null) Globals.gameInstance.sceneGraph.Remove(g.receipt);
            g.receipt = Globals.gameInstance.sceneGraph.Add(g.mesh);
        }

        public void SetupLights()
        {

            torchlight = new Light();
            torchlight.LightType = Light.Type.Spot;
            torchlight.ShadowDepthBias = 0.002f;
            torchlight.Radius = 15;
            torchlight.SpotAngle = TORCH_ANGLE;
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
            haloemitlight.Intensity = 0.6f;
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

        public void AddCriticalPoints(List<Vector2> outputPoints)
        {
            outputPoints.Add(new Vector2(_position.X, _position.Z));

            Vector3 v = new Vector3(0, 0, -6);
            Matrix m = Matrix.CreateRotationY(rotation) * Matrix.CreateTranslation(_position);
            Vector3 t = Vector3.Transform(v, m);
            outputPoints.Add(new Vector2(t.X,t.Z));
        }

        public void Respawn(Vector3 vector3)
        {
            this._position = vector3;
            alive = true;
            dead = false;
            health = 100.0;
            oldHealth = 100.0;
            upPlayer.StartClip(moving + shooting + weapon);
            lowPlayer.StartClip(moving + weapon);
            SwitchWeapon(0);
        }
    }
}
