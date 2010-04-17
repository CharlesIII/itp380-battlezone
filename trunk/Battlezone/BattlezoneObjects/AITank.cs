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
        const float TURRET_ROTATION_SPEED = 2.5f;
        const float AUTOMATIC_DETECTION_RADIUS = 20.0f;

        ModelBone chassisBone;
        ModelBone turretBone;
        ModelBone cannonBone;
        Model tankModel;

        Matrix chassisTransform;
        Matrix turretTransform;
        Matrix cannonTransform;

        PathFinder navigation;
        ArrayList navNodes;
        ArrayList path;
        ArrayList pathFromPatrolBeginToEnd;
        ArrayList pathFromPatrolEndToBegin;

        Vector3 m_vPlayerPosition;
        Vector3 m_vPlayerLastKnownPosition;
        Vector3 m_vTarget;
        Vector3 m_vPatrolBegin;
        Vector3 m_vPatrolEnd;
        Vector3 m_vCurrentPathTarget;

        enum AIStates {PURSUE, PATROL, NEED_PATROL, SCAN, ATTACK};
        AIStates currentState;

        float turretRotationValue;
        float turretTargetRotationValue;


        /// <summary>
        /// Constructs an AI controlled tank.
        /// </summary>
        /// <param name="game">A reference to the game.</param>
        /// <param name="pf">A* Path Finding implementation.</param>
        /// <param name="playerPos">A reference to the player's position.</param>
        public AITank(Game game, PathFinder pf, Vector3 playerPos)
            : base(game)
        {
            // TODO: Construct any child components here
            sMeshesToLoad.Add("enemyTank");
            navigation = pf;
            m_vPlayerPosition = playerPos;  //AI always knows where the player is
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();

            navNodes = navigation.GetNavigationNodes();

            Random rg = new Random();
            m_vPatrolBegin = (Vector3)navNodes[rg.Next(navNodes.Count)];
            m_vPatrolEnd = (Vector3)navNodes[rg.Next(navNodes.Count)];

            //compute the patrol paths once and store them to save on computation costs
            pathFromPatrolBeginToEnd = navigation.GetPath(m_vPatrolBegin, m_vPatrolEnd);
            pathFromPatrolEndToBegin = navigation.GetPath(m_vPatrolEnd, m_vPatrolBegin);

            WorldPosition = m_vPatrolBegin; //start off at the beginning of the patrol path;

            m_vTarget = m_vPatrolEnd;
            path = pathFromPatrolBeginToEnd;
            m_vCurrentPathTarget = WorldPosition;   //set the initial targets to be the current position so the tank doesn't move
            
            currentState = AIStates.PATROL;

            Scale = 50.0f;

            COLLISION_IDENTIFIER = CollisionIdentifier.TANK;         
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
        /// This works like a scheduler in Agent design.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            float fDelta = gameTime.ElapsedGameTime.Ticks / System.TimeSpan.TicksPerMillisecond / 1000.0f;
            timer.Update(gameTime);

            //first update the tank's position
            if ((m_vCurrentPathTarget - WorldPosition).Length() < 0.5f)
            {
                //we're close enough so stop moving and snap position
                Velocity = new Vector3(0.0f);
                WorldPosition = m_vCurrentPathTarget;
            }
            else
            {
                if (bPhysicsDriven)
                {

                }
                else
                {
                    
                    WorldPosition = Velocity * fDelta;
                }
            }

            //now perform AI logic
            /****************************************************************
             * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
             * REMINDER TO SELF: come up with a way to resolve or avoid 
             * collisions in paths of the AI tanks.
             * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
             ****************************************************************/
            if ((WorldPosition - m_vPlayerPosition).Length() < AUTOMATIC_DETECTION_RADIUS)
            {
                //player has been automatically detected so set PlayerLastKnownPosition
                //m_vPlayerLastKnownPosition = new Vector3(m_vPlayerPosition.X, m_vPlayerPosition.Y, m_vPlayerPosition.Z);

                //perform quick instant vision check
                //if visible, set turretRotationTarget and stop moving and set state to attack
                //if not visisble, find the closest nav node to the player position and navigate to it and set state to pursue
            }

            if (currentState == AIStates.PATROL)
            {
                //if we've reached CurrentPathTarget, scan for player
                if (WorldPosition.Equals(m_vCurrentPathTarget))
                {
                    currentState = AIStates.SCAN;
                }
            }
            else if (currentState == AIStates.NEED_PATROL)
            {
                if (path.IndexOf(m_vCurrentPathTarget) == path.Count - 1)
                {
                    if (WorldPosition.Equals(m_vPatrolEnd))
                    {
                        m_vTarget = m_vPatrolBegin;
                        path = pathFromPatrolEndToBegin;
                    }
                    else if (WorldPosition.Equals(m_vPatrolBegin))
                    {
                        m_vTarget = m_vPatrolEnd;
                        path = pathFromPatrolBeginToEnd;
                    }
                    else
                    {
                        m_vTarget = m_vPatrolBegin;
                        path = navigation.GetPath(WorldPosition, m_vPatrolBegin);
                    }
                    m_vCurrentPathTarget = (Vector3)path[0];
                }
                else
                {
                    m_vCurrentPathTarget = (Vector3)path[path.IndexOf(m_vCurrentPathTarget) + 1];
                }
                currentState = AIStates.PATROL;
            }
            else if (currentState == AIStates.SCAN)
            {
                //scan for the player
                if (turretRotationValue + TURRET_ROTATION_SPEED * fDelta < 2*Math.PI)
                {
                    turretRotationValue += TURRET_ROTATION_SPEED * fDelta;

                    if (CheckPlayerSighted())
                    {
                        //player is visible, switch over to attack mode
                        currentState = AIStates.ATTACK;
                    }
                }
                else
                {
                    //finished or almost finished a full rotation of the turret so snap it to 0 and perform once last scan
                    turretRotationValue = 0;
             
                    if (CheckPlayerSighted())
                    {
                        currentState = AIStates.ATTACK;
                    }
                    else
                        currentState = AIStates.NEED_PATROL;
                }
            }
            //Console.Out.WriteLine("Current state: " + currentState);
            /*
            //check to see if we're close to the target position
            if ((m_vTarget - WorldPosition).Length() > 0.5f)
            {
                Vector3 temp = m_vTarget - WorldPosition;
                Vector3 facing = GetWorldFacing();

                temp.Normalize();
                facing.Normalize();

                if (Vector3.Dot(facing, temp) != 1)
                Velocity =  temp * 5.0f;
            }
            else
            {
                

                //check to see if there's a new target that was set while we were moving to the current one
                if (!m_vNewTarget.Equals(m_vTarget))
                {
                    //there's another target to move to so set the current target to be that
                    m_vTarget = m_vNewTarget;
                }
            }
            */
            /*
            if (bPhysicsDriven)
            {
                m_vVelocity += vAcceleration * fDelta / 2.0f;
                //TODO: Make sure this is being assigned by value, not by reference
                m_vPreviousWorldPosition = m_vWorldPosition;
                m_vWorldPosition += m_vVelocity * fDelta;
                vAcceleration = vForce / fMass;
                m_vVelocity += vAcceleration * fDelta / 2.0f;
                if (m_vVelocity.Length() >= fTerminalVelocity)
                {
                    m_vVelocity.Normalize();
                    m_vVelocity *= fTerminalVelocity;
                }
            }
            else
            {
                m_vWorldPosition += Vector3.Multiply(m_vVelocity, gameTime.ElapsedGameTime.Ticks / System.TimeSpan.TicksPerMillisecond / 1000.0f);

                //TODO: Add World Bound check so the player doesn't fall off the world
            }*/

            //when the tank reaches its target, it performs a radial check for the player
            //if the player is within a minimum detection distance, the AI will instantly "discover" the player
            //AI tank has a 20 degree viewing angle for checking? maybe?
        }

        //Helper functions to keep the Update method clean
        private bool CheckPlayerSighted()
        {
            return false;
        }

        /// <summary>
        /// Finds the closest navigation node to the given position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns>Closest navigation node.</returns>
        private Vector3 FindClosestNavNode(Vector3 position)
        {
            return new Vector3();
        }
    }
}