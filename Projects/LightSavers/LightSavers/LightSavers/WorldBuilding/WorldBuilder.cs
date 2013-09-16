﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightSavers.Components;
using Microsoft.Xna.Framework;
using LightPrePassRenderer;
using Microsoft.Xna.Framework.Graphics;

namespace LightSavers.WorldBuilding
{

    public class WorldBuilder
    {
        
        public RealGame game;
        public Vector3 origin;

        public WorldBuilder(RealGame game, int size, Vector3 origin)
        {
            this.game = game;
            this.origin = origin;

            if (size < 2) size = 2;

            Vector3 corigin = origin;

            // add start
            BuildSection(0, corigin);
            corigin += Vector3.Right * 32;

            // add middles
            Random r = new Random();
            for (int i=0;i<size-2;i++)
            {
                int ri = 2 + r.Next(AssetLoader.mdl_section.Length-2);
                BuildSection(ri, corigin);
                corigin += Vector3.Right * 32;
            }            

            // add end
            BuildSection(1, corigin);

        }

        public void BuildSection(int index, Vector3 corigin)
        {
            Mesh mesh = new Mesh();
            mesh.Model = AssetLoader.mdl_section[index];
            mesh.Transform = Matrix.CreateTranslation(corigin);
            this.game.lightAndMeshContainer.AddMesh(mesh);

            SpawnEntities(index, corigin);
        }

        public void SpawnEntities(int index, Vector3 corigin)
        {

            Texture2D t = AssetLoader.tex_section_ent[index];
            Color[] colours = new Color[32*32];
            t.GetData<Color>(colours);

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    Color c = colours[y * 32 + x];
                    Vector3 center = corigin + new Vector3(0.5f + x, 0, 0.5f + y);
                    if (c == Color.Yellow)
                    {
                        SpawnOverheadLight(center);
                    }
                }
            }
        }

        public void SpawnOverheadLight(Vector3 position)
        {
            System.Diagnostics.Debug.WriteLine("Spawned light at " + position.ToString());
            Mesh m = new Mesh();
            m.Model = AssetLoader.mdl_ceilinglight;
            m.Transform = Matrix.CreateTranslation(position + Vector3.Up * 4);
            game.lightAndMeshContainer.AddMesh(m);

            Light l = new Light();
            l.LightType = Light.Type.Point;
            l.Radius = 7;
            l.Intensity = 0.5f;

            
            l.Transform = Matrix.CreateTranslation(position + Vector3.Up * 4);
            game.lightAndMeshContainer.AddLight(l);

        }
        


    }
}
