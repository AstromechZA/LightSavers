﻿using LightPrePassRenderer;
using LightPrePassRenderer.partitioning;
using LightSavers.Collisions;
using LightSavers.Components.GameObjects;
using LightSavers.Components.Projectiles;
using LightSavers.Utils;
using LightSavers.WorldBuilding;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LightSavers.Components
{
    public class RealGame
    {
        private PlayerObject[] players;

        public ProjectileManager projectileManager;

        public AwesomeSceneGraph sceneGraph;

        public WorldBuilder worldBuilder;

        public CellCollider cellCollider;

        public List<Door> doors;

        public RealGame(int numberOfSections, int numPlayers, AwesomeSceneGraph sg)
        {
            sceneGraph = sg;

            doors = new List<Door>();

            cellCollider = new CellCollider(32, numberOfSections * 32);

            worldBuilder = new WorldBuilder(this, numberOfSections, Vector3.Zero);

            projectileManager = new ProjectileManager();
            
            players = new PlayerObject[numPlayers];


            Color[] playerColours = new Color[] {
                new Color(0.5f, 1.0f, 0.5f),
                new Color(0.5f, 0.6f, 1.0f)
            };

            Vector3[] spawns = new Vector3[] {
                new Vector3(4, 0, 4),
                new Vector3(4, 0, 10)
            };

            for (int i = 0; i < numPlayers; i++)
            {
                players[i] = new PlayerObject(this, (i==0) ? PlayerIndex.One : PlayerIndex.Two, playerColours[i], spawns[i], 0);
                players[i].AddToSG();
            }

        }

        public void Update(float ms)
        {
            for (int i = 0; i < players.Length; i++) players[i].Update(ms);
            projectileManager.Update(ms);
            for (int i = 0; i < doors.Count; i++) doors[i].Update(ms);
        }

        private List<Vector2> criticalPoints = new List<Vector2>(8);
        public List<Vector2> GetCriticalPoints()
        {
            criticalPoints.Clear();
            for (int i = 0; i < players.Length; i++) players[i].AddCriticalPoints(criticalPoints);
            return criticalPoints;
        }


        public PlayerObject GetClosestPlayer(Vector3 position)
        {
            float d = Vector3.DistanceSquared(position, players[0].Position);
            if (players.Length > 1)
            {
                float d2 = Vector3.DistanceSquared(position, players[1].Position);
                if (d2 < d) return players[1];
            }
            return players[0];
        }
    }
}
