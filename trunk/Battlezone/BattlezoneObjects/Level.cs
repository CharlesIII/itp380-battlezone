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
            
            
            Building building1 = new Building(Game, "buildings\\building1");
            Game.Components.Add(building1);

            Building building2 = new Building(Game, "buildings\\building2");
            Game.Components.Add(building2);

            Building building3 = new Building(Game, "buildings\\building3");
            Game.Components.Add(building3);

            Building building4 = new Building(Game, "buildings\\building4");
            Game.Components.Add(building4);

            Building building5 = new Building(Game, "buildings\\building5");
            Game.Components.Add(building5);

            Building building6 = new Building(Game, "buildings\\building6");
            Game.Components.Add(building6);
            
            Building building7 = new Building(Game, "buildings\\building7");
            //Game.Components.Add(building7);

            Building building8 = new Building(Game, "buildings\\building8");
            //Game.Components.Add(building8);

            Building building9 = new Building(Game, "buildings\\building9");
            //Game.Components.Add(building9);

            Building building10 = new Building(Game, "buildings\\building10");
            Game.Components.Add(building10);
            
            Building building11 = new Building(Game, "buildings\\building11");
            Game.Components.Add(building11);

            Building building12 = new Building(Game, "buildings\\building12");
            Game.Components.Add(building12);

            Building building13 = new Building(Game, "buildings\\building13");
            Game.Components.Add(building13);

            Building building14 = new Building(Game, "buildings\\building14");
            Game.Components.Add(building14);

            Building building15 = new Building(Game, "buildings\\building15");
            Game.Components.Add(building15);

            Building building16 = new Building(Game, "buildings\\building16");
            Game.Components.Add(building16);

            Building building17 = new Building(Game, "buildings\\building17");
            Game.Components.Add(building17);

            Building building18 = new Building(Game, "buildings\\building18");
            Game.Components.Add(building18);

            Building building19 = new Building(Game, "buildings\\building19");
            Game.Components.Add(building19);

            Building building20 = new Building(Game, "buildings\\building20");
            Game.Components.Add(building20);

            Building building21 = new Building(Game, "buildings\\building21");
            Game.Components.Add(building21);

            Building building22 = new Building(Game, "buildings\\building22");
            Game.Components.Add(building22);

            Building building23 = new Building(Game, "buildings\\building23");
            Game.Components.Add(building23);

            Building building24 = new Building(Game, "buildings\\building24");
            Game.Components.Add(building24);

            Building building25 = new Building(Game, "buildings\\building25");
            Game.Components.Add(building25);

            Building building26 = new Building(Game, "buildings\\building26");
            Game.Components.Add(building26);

            Building building27 = new Building(Game, "buildings\\building27");
            Game.Components.Add(building27);

            Building building28 = new Building(Game, "buildings\\building28");
            Game.Components.Add(building28);

            Building building29 = new Building(Game, "buildings\\building29");
            Game.Components.Add(building29);

            Building building30 = new Building(Game, "buildings\\building30");
            Game.Components.Add(building30);

            Building building31 = new Building(Game, "buildings\\building31");
            Game.Components.Add(building31);

            Building building32 = new Building(Game, "buildings\\building32");
            Game.Components.Add(building32);

            Building building33 = new Building(Game, "buildings\\building33");
            Game.Components.Add(building33);

            Building building34 = new Building(Game, "buildings\\building34");
            Game.Components.Add(building34);

            Building building35 = new Building(Game, "buildings\\building35");
            Game.Components.Add(building35);

            Building building36 = new Building(Game, "buildings\\building36");
            Game.Components.Add(building36);

            Building building37 = new Building(Game, "buildings\\building37");
            Game.Components.Add(building37);

            Building building38 = new Building(Game, "buildings\\building38");
            Game.Components.Add(building38);

            Building building39 = new Building(Game, "buildings\\building39");
            Game.Components.Add(building39);

            Building building40 = new Building(Game, "buildings\\building40");
            Game.Components.Add(building40);

            Building building41 = new Building(Game, "buildings\\building41");
            Game.Components.Add(building41);

            Building building42 = new Building(Game, "buildings\\building42");
            Game.Components.Add(building42);

            Building building43 = new Building(Game, "buildings\\building43");
            Game.Components.Add(building43);

            Building building44 = new Building(Game, "buildings\\building44");
            Game.Components.Add(building44);

            Building building45 = new Building(Game, "buildings\\building45");
            Game.Components.Add(building45);

            Building building46 = new Building(Game, "buildings\\building46");
            Game.Components.Add(building46);

            Building building47 = new Building(Game, "buildings\\building47");
            Game.Components.Add(building47);

            Building building48 = new Building(Game, "buildings\\building48");
            Game.Components.Add(building48);

            Building building49 = new Building(Game, "buildings\\building49");
            Game.Components.Add(building49);

            Building building50 = new Building(Game, "buildings\\building50");
            Game.Components.Add(building50);

            Building building51 = new Building(Game, "buildings\\building51");
            Game.Components.Add(building51);

            Building building52 = new Building(Game, "buildings\\building52");
            Game.Components.Add(building52);

            Building building53 = new Building(Game, "buildings\\building53");
            Game.Components.Add(building53);

            Building building54 = new Building(Game, "buildings\\building54");
            Game.Components.Add(building54);

            Building building55 = new Building(Game, "buildings\\building55");
            Game.Components.Add(building55);

            Building wall1 = new Building(Game, "walls\\wall1");
            Game.Components.Add(wall1);

            Building wall2 = new Building(Game, "walls\\wall2");
            Game.Components.Add(wall2);

            Building wall3 = new Building(Game, "walls\\wall3");
            Game.Components.Add(wall3);

            Building wall4 = new Building(Game, "walls\\wall4");
            Game.Components.Add(wall4);

            Building wall5 = new Building(Game, "walls\\wall5");
            Game.Components.Add(wall5);

            Building wall6 = new Building(Game, "walls\\wall6");
            Game.Components.Add(wall6);

            Building wall7 = new Building(Game, "walls\\wall7");
            Game.Components.Add(wall7);

            Building wall8 = new Building(Game, "walls\\wall8");
            Game.Components.Add(wall8);

            Building wall9 = new Building(Game, "walls\\wall9");
            Game.Components.Add(wall9);

            Building wall10 = new Building(Game, "walls\\wall10");
            Game.Components.Add(wall10);

            Building wall11 = new Building(Game, "walls\\wall11");
            Game.Components.Add(wall11);

            Building wall12 = new Building(Game, "walls\\wall12");
            Game.Components.Add(wall12);

            Building wall13 = new Building(Game, "walls\\wall13");
            Game.Components.Add(wall13);

            Building wall14 = new Building(Game, "walls\\wall14");
            Game.Components.Add(wall14);

            Building wall15 = new Building(Game, "walls\\wall15");
            Game.Components.Add(wall15);

            Building wall16 = new Building(Game, "walls\\wall16");
            Game.Components.Add(wall16);

            Building wall17 = new Building(Game, "walls\\wall17");
            Game.Components.Add(wall17);

            Building wall18 = new Building(Game, "walls\\wall18");
            Game.Components.Add(wall18);

            Building wall19 = new Building(Game, "walls\\wall19");
            Game.Components.Add(wall19);

            Building wall20 = new Building(Game, "walls\\wall20");
            Game.Components.Add(wall20);

            Building wall21 = new Building(Game, "walls\\wall21");
            Game.Components.Add(wall21);

            Building wall22 = new Building(Game, "walls\\wall22");
            Game.Components.Add(wall22);

            Building wall23 = new Building(Game, "walls\\wall23");
            Game.Components.Add(wall23);

            Building wall24 = new Building(Game, "walls\\wall24");
            Game.Components.Add(wall24);

            Building wall25 = new Building(Game, "walls\\wall25");
            Game.Components.Add(wall25);

            Building wall26 = new Building(Game, "walls\\wall26");
            Game.Components.Add(wall26);

            Building wall27 = new Building(Game, "walls\\wall27");
            Game.Components.Add(wall27);

            Building wall28 = new Building(Game, "walls\\wall28");
            Game.Components.Add(wall28);

            Building wall29 = new Building(Game, "walls\\wall29");
            Game.Components.Add(wall29);

            Building wall30 = new Building(Game, "walls\\wall30");
            Game.Components.Add(wall30);

            Building wall31 = new Building(Game, "walls\\wall31");
            Game.Components.Add(wall31);

            Building wall32 = new Building(Game, "walls\\wall32");
            Game.Components.Add(wall32);

            Building wall33 = new Building(Game, "walls\\wall33");
            Game.Components.Add(wall33);

            Building wall34 = new Building(Game, "walls\\wall34");
            Game.Components.Add(wall34);

            Building wall35 = new Building(Game, "walls\\wall35");
            Game.Components.Add(wall35);

            Building wall36 = new Building(Game, "walls\\wall36");
            Game.Components.Add(wall36);

            Building wall37 = new Building(Game, "walls\\wall37");
            Game.Components.Add(wall37);

            Building wall38 = new Building(Game, "walls\\wall38");
            Game.Components.Add(wall38);

            Building wall39 = new Building(Game, "walls\\wall39");
            Game.Components.Add(wall39);

            Building wall40 = new Building(Game, "walls\\wall40");
            Game.Components.Add(wall40);

            Building wall41 = new Building(Game, "walls\\wall41");
            Game.Components.Add(wall41);

            Building wall42 = new Building(Game, "walls\\wall42");
            Game.Components.Add(wall42);

            Building wall43 = new Building(Game, "walls\\wall43");
            Game.Components.Add(wall43);

            Building wall44 = new Building(Game, "walls\\wall44");
            Game.Components.Add(wall44);

            Building wall45 = new Building(Game, "walls\\wall45");
            Game.Components.Add(wall45);

            Building wall46 = new Building(Game, "walls\\wall46");
            Game.Components.Add(wall46);

            Building wall47 = new Building(Game, "walls\\wall47");
            Game.Components.Add(wall47);

            Building wall48 = new Building(Game, "walls\\wall48");
            Game.Components.Add(wall48);

            Building wall49 = new Building(Game, "walls\\wall49");
            Game.Components.Add(wall49);

            Building wall50 = new Building(Game, "walls\\wall50");
            Game.Components.Add(wall50);

            Building wall51 = new Building(Game, "walls\\wall51");
            Game.Components.Add(wall51);

            Building wall52 = new Building(Game, "walls\\wall52");
            Game.Components.Add(wall52);

            Building wall53 = new Building(Game, "walls\\wall53");
            Game.Components.Add(wall53);

            Building wall54 = new Building(Game, "walls\\wall54");
            Game.Components.Add(wall54);

            Building wall55 = new Building(Game, "walls\\wall55");
            Game.Components.Add(wall55);

            Building wall56 = new Building(Game, "walls\\wall56");
            Game.Components.Add(wall56);

            Building wall57 = new Building(Game, "walls\\wall57");
            Game.Components.Add(wall57);

            Building wall58 = new Building(Game, "walls\\wall58");
            Game.Components.Add(wall58);

            Building wall59 = new Building(Game, "walls\\wall59");
            Game.Components.Add(wall59);

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