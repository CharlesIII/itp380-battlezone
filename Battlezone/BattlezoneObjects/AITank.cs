using System;
using System.Collections;
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
using Battlezone.Engine;

namespace Battlezone.BattlezoneObjects
{
    /// <summary>
    /// Inherits from Actor. This is the AI controlled tank. A reference to the player's position vector is passed in
    /// as part of the constructor because the AI always knows where the player is.
    /// </summary>
    public class AITank : Actor
    {
        ModelBone chassisBone;
        ModelBone turretBone;
        ModelBone cannonBone;
        Model tankModel;

        Matrix chassisTransform;
        Matrix turretTransform;
        Matrix cannonTransform;

        PathFinder navigation;
        ArrayList navNodes;

        Vector3 m_vPlayerPosition;
        Vector3 m_vCurrentTarget;
        Vector3 m_vNewTarget;
        Vector3[] patrolPath;   //probably need to pass in a file to load

        string patrolFilePath;

        float turretRotationValue;

        public AITank(Game game, PathFinder pf, Vector3 playerPos, string patrolFile)
            : base(game)
        {
            // TODO: Construct any child components here
            sMeshesToLoad.Add("enemyTank");
            navigation = pf;
            m_vPlayerPosition = playerPos;  //AI always knows where the player is

            patrolFilePath = patrolFile;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();

            Scale = 50.0f;

            COLLISION_IDENTIFIER = CollisionIdentifier.TANK;

            //set the initial targets to be the current position so the tank doesn't move
            m_vCurrentTarget = WorldPosition;
            m_vNewTarget = WorldPosition;

            navNodes = navigation.GetNavigationNodes();

            //need to write loader code here for patrol path
            //otherwise, maybe i could perform a DFS of the navigation graph and take the first cycle i find as the path?
        }

        /// <summary>
        /// Load in the components of the enemyTank mesh and keep a reference to them. Also construct
        /// necessary transform matrices.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            tankModel = ActorModels[0] as Model;

            // Look up shortcut references to the bones we are going to animate.
            chassisBone = tankModel.Bones["chassis_geo"];
            turretBone = tankModel.Bones["turret_geo"];
            cannonBone = tankModel.Bones["cannon_geo"];

            // Store the original transform matrix for each animating bone.
            chassisTransform = chassisBone.Transform;
            turretTransform = turretBone.Transform;
            cannonTransform = cannonBone.Transform;
        }

        public override void Draw(GameTime gameTime)
        {
            base.updateWorldTransform();

            tankModel.Root.Transform = worldTransform;

            // Calculate matrices based on the current animation position
            Matrix turretRotation = Matrix.CreateRotationY(turretRotationValue);

            // Apply matrices to the relevant bones
            turretBone.Transform = turretRotation * turretTransform;
            cannonBone.Transform = turretRotation * cannonTransform;    //might change later if cannon can rotate up and down

            // Look up combined bone matrices for the entire model.
            tankModel.CopyAbsoluteBoneTransformsTo(boneTransforms);

            // Draw the model.
            foreach (ModelMesh mesh in tankModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index];
                    effect.View = GameplayScreen.CameraMatrix;
                    effect.Projection = GameplayScreen.ProjectionMatrix;

                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.AmbientLightColor = Color.White.ToVector3();
                }

                mesh.Draw();
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            if ((m_vCurrentTarget - WorldPosition).Length > 0.5f)
                Velocity = (m_vCurrentTarget - WorldPosition).Normalize() * 5.0f;
            else
                Velocity = new Vector3(0.0f);

            base.Update(gameTime);

            //when the tank reaches its target, it performs a radial check for the player
            //if the player is within a minimum detection distance, the AI will instantly "discover" the player
            //AI tank has a 20 degree viewing angle for checking? maybe?
        }
    }
}