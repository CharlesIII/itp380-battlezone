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
    /// Inherits from Actor, represents buildings. Uses a BoundingBox for collision instead of BoundingSphere.
    /// 
    /// </summary>
    public class Building : Actor
    {
        //BoundingBox ModelBounds;
        public BoundingBox WorldBoundsBox;

        public Building(Game game, string modelName)
            : base(game)
        {
            // TODO: Construct any child components here
            sMeshToLoad = modelName;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();

            COLLISION_IDENTIFIER = CollisionIdentifier.BUILDING;
        }

        protected override void LoadContent()
        {
            //TODO: Add custom content loading logic
            base.LoadContent();

            Matrix[] m_transforms = new Matrix[ActorModel.Bones.Count];
            ActorModel.CopyAbsoluteBoneTransformsTo(m_transforms);

            foreach (ModelMesh mesh in ActorModel.Meshes)
            {
                VertexPositionNormalTexture[] vertices =
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

        /// <summary>
        /// Allows the game component to draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}