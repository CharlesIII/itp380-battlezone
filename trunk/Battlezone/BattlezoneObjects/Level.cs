using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace Battlezone.BattlezoneObjects
{
    /// <summary>
    /// Inherits from Actor; represents level geometry. Uses BoundingBox for collision.
    /// </summary>
    public class Level : Actor
    {
        BoundingBox WorldBoundsBox;

        public Level(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
            sMeshToLoad = "ground";
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
            Scale = 1.0f;
            
            Building building1 = new Building(Game, "building1");
            Game.Components.Add(building1);

            Building building2 = new Building(Game, "building2");
            Game.Components.Add(building2);

            Building building3 = new Building(Game, "building3");
            Game.Components.Add(building3);

            Building building4 = new Building(Game, "building4");
            Game.Components.Add(building4);

            Building building5 = new Building(Game, "building5");
            Game.Components.Add(building5);

            Building building6 = new Building(Game, "building6");
            Game.Components.Add(building6);

            Building building7 = new Building(Game, "building7");
            Game.Components.Add(building7);

            Building building8 = new Building(Game, "building8");
            Game.Components.Add(building8);

            Building building9 = new Building(Game, "building9");
            Game.Components.Add(building9);

            Building building10 = new Building(Game, "building10");
            Game.Components.Add(building10);

            Building building11 = new Building(Game, "building11");
            Game.Components.Add(building11);

            Building building12 = new Building(Game, "building12");
            Game.Components.Add(building12);

            Building building13 = new Building(Game, "building13");
            Game.Components.Add(building13);

            Building building14 = new Building(Game, "building14");
            Game.Components.Add(building14);

            Building building15 = new Building(Game, "building15");
            Game.Components.Add(building15);

            Building building16 = new Building(Game, "building16");
            Game.Components.Add(building16);

            Building building17 = new Building(Game, "building17");
            Game.Components.Add(building17);

            Building building18 = new Building(Game, "building18");
            Game.Components.Add(building18);

            Building building19 = new Building(Game, "building19");
            Game.Components.Add(building19);

            Building building20 = new Building(Game, "building20");
            Game.Components.Add(building20);

            Building building21 = new Building(Game, "building21");
            Game.Components.Add(building21);

            Building building22 = new Building(Game, "building22");
            Game.Components.Add(building22);

            Building building23 = new Building(Game, "building23");
            Game.Components.Add(building23);

            Building building24 = new Building(Game, "building24");
            Game.Components.Add(building24);

            Building building25 = new Building(Game, "building25");
            Game.Components.Add(building25);

            Building building26 = new Building(Game, "building26");
            Game.Components.Add(building26);

            Building building27 = new Building(Game, "building27");
            Game.Components.Add(building27);

            Building building28 = new Building(Game, "building28");
            Game.Components.Add(building28);

            Building building29 = new Building(Game, "building29");
            Game.Components.Add(building29);

            Building building30 = new Building(Game, "building30");
            Game.Components.Add(building30);

            Building building31 = new Building(Game, "building31");
            Game.Components.Add(building31);

            Building building32 = new Building(Game, "building32");
            Game.Components.Add(building32);

            Building building33 = new Building(Game, "building33");
            Game.Components.Add(building33);

            Building building34 = new Building(Game, "building34");
            Game.Components.Add(building34);

            Building building35 = new Building(Game, "building35");
            Game.Components.Add(building35);

            Building building36 = new Building(Game, "building36");
            Game.Components.Add(building36);

            Building building37 = new Building(Game, "building37");
            Game.Components.Add(building37);

            Building building38 = new Building(Game, "building38");
            Game.Components.Add(building38);

            Building building39 = new Building(Game, "building39");
            Game.Components.Add(building39);

            Building building40 = new Building(Game, "building40");
            Game.Components.Add(building40);

            Building building41 = new Building(Game, "building41");
            Game.Components.Add(building41);

            Building building42 = new Building(Game, "building42");
            Game.Components.Add(building42);

            Building building43 = new Building(Game, "building43");
            Game.Components.Add(building43);

            Building building44 = new Building(Game, "building44");
            Game.Components.Add(building44);

            Building building45 = new Building(Game, "building45");
            Game.Components.Add(building45);

            Building building46 = new Building(Game, "building46");
            Game.Components.Add(building46);

            Building building47 = new Building(Game, "building47");
            Game.Components.Add(building47);

            Building building48 = new Building(Game, "building48");
            Game.Components.Add(building48);

            Building building49 = new Building(Game, "building49");
            Game.Components.Add(building49);

            Building building50 = new Building(Game, "building50");
            Game.Components.Add(building50);

            Building building51 = new Building(Game, "building51");
            Game.Components.Add(building51);

            Building building52 = new Building(Game, "building52");
            Game.Components.Add(building52);

            Building building53 = new Building(Game, "building53");
            Game.Components.Add(building53);

            Building building54 = new Building(Game, "building54");
            Game.Components.Add(building54);

            Building building55 = new Building(Game, "building55");
            Game.Components.Add(building55);
             
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            Matrix []m_transforms = new Matrix[ActorModel.Bones.Count];
            ActorModel.CopyAbsoluteBoneTransformsTo(m_transforms);

            foreach (ModelMesh mesh in ActorModel.Meshes)
            {
                VertexPositionNormalTexture[] vertices=
                    new VertexPositionNormalTexture[mesh.VertexBuffer.SizeInBytes / mesh.MeshParts[0].VertexStride];

                mesh.VertexBuffer.GetData<VertexPositionNormalTexture>(vertices);

                // Find min, max xyz for this mesh - assumes will be centred on 0,0,0 as BB is initialised to 0,0,0
                Vector3 min = vertices[0].Position;
                Vector3 max = vertices[0].Position;

                for (int i = 1; i < vertices.Length; i++)
                {
                    min = Vector3.Min(min, vertices[i].Position);
                    max = Vector3.Max(max, vertices[i].Position);
                }                

                // We need to take into account the fact that the mesh may have a bone transform
                min = Vector3.Transform(min, m_transforms[mesh.ParentBone.Index]);
                max = Vector3.Transform(max, m_transforms[mesh.ParentBone.Index]);

                // Now expand main bb by this mesh's box
                WorldBoundsBox.Min = Vector3.Min(WorldBoundsBox.Min, min);
                WorldBoundsBox.Max = Vector3.Max(WorldBoundsBox.Max, max);
            }

        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }
    }
}