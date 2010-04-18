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

        public Building(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
            //sMeshToLoad = "playerTank";
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

            foreach (ModelMesh mesh in ActorModel.Meshes)
            {
                Vector3[] vertices = new Vector3[mesh.IndexBuffer.SizeInBytes / mesh.MeshParts[0].VertexStride];
                mesh.VertexBuffer.GetData<Vector3>(vertices);
                WorldBoundsBox = BoundingBox.CreateFromPoints(vertices);
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