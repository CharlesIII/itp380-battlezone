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
        #region Variables

        const float TURRET_ROTATION_SPEED = 1.0f;
        const float AUTOMATIC_DETECTION_RADIUS = 20.0f;
        const float TANK_ROTATION_SPEED = 1.25f;
        const float PURSUIT_DURATION = 30.0f;

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

        enum AIStates {NEED_PURSUE, PURSUE, PATROL, NEED_PATROL, SCAN, ATTACK, DEAD};
        AIStates currentState;

        float turretRotationValue;
        float turretTargetRotationValue;

        float targetTankRotationValue;

        Random rg = new Random();

        #endregion

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
            sMeshToLoad = "enemyTank";
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

            fMass = 10;
            bPhysicsDriven = true;
            fTerminalVelocity = 200.0f;

            navNodes = navigation.GetNavigationNodes();

            m_vPatrolBegin = (Vector3)navNodes[rg.Next(navNodes.Count)];
            do{
                m_vPatrolEnd = (Vector3)navNodes[rg.Next(navNodes.Count)];
            } while (m_vPatrolEnd.Equals(m_vPatrolBegin));

            //compute the patrol paths once and store them to save on computation costs
            pathFromPatrolBeginToEnd = navigation.GetPath(m_vPatrolBegin, m_vPatrolEnd);
            pathFromPatrolEndToBegin = navigation.GetPath(m_vPatrolEnd, m_vPatrolBegin);

            WorldPosition = m_vPatrolBegin; //start off at the beginning of the patrol path;

            m_vTarget = m_vPatrolEnd;
            path = pathFromPatrolBeginToEnd;
            m_vCurrentPathTarget = WorldPosition;   //set the initial targets to be the current position so the tank doesn't move
            
            currentState = AIStates.PATROL;

            Scale = 50.0f;

            COLLISION_IDENTIFIER = CollisionIdentifier.AI_TANK;         
        }

        /// <summary>
        /// Load in the components of the enemyTank mesh and keep a reference to them. Also construct
        /// necessary transform matrices.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            tankModel = ActorModel;

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

            if (currentState != AIStates.DEAD)
            {
                //make sure we're facing the right direction before we start moving
                if (RotAngle < targetTankRotationValue)
                {
                    //we need to rotate CCW
                    RotAngle += TANK_ROTATION_SPEED * fDelta;

                    //clamp RotAngle to target if close enough
                    if (Math.Abs(targetTankRotationValue - RotAngle) <= 0.10f)
                        RotAngle = targetTankRotationValue;
                    Quat = Quaternion.CreateFromAxisAngle(Vector3.UnitY, RotAngle);
                }
                else if (RotAngle > targetTankRotationValue)
                {
                    //we need to rotate CW
                    RotAngle -= TANK_ROTATION_SPEED * fDelta;

                    //clamp RotAngle to target if close enough
                    if (Math.Abs(targetTankRotationValue - RotAngle) <= 0.10f)
                        RotAngle = targetTankRotationValue;
                    Quat = Quaternion.CreateFromAxisAngle(Vector3.UnitY, RotAngle);
                }
                else
                {
                    //we're facing the right direction do movement logic
                    if ((m_vCurrentPathTarget - WorldPosition).Length() < 5.0f)
                    {
                        //we're close enough so stop moving and snap position
                        Velocity = new Vector3(0.0f);
                        WorldPosition = m_vCurrentPathTarget;
                    }
                    else
                    {
                        if (bPhysicsDriven)
                        {
                            //Console.Out.WriteLine(Velocity);
                            Velocity += vAcceleration * fDelta / 2.0f;
                            m_vPreviousWorldPosition = m_vWorldPosition;
                            WorldPosition += Velocity * fDelta;
                            vAcceleration = vForce / fMass;
                            Velocity += vAcceleration * fDelta / 2.0f;
                            if (Velocity.Length() >= fTerminalVelocity)
                            {
                                //Console.Out.WriteLine("Normalizing");
                                Vector3 temp = Velocity;
                                temp.Normalize();
                                Velocity = temp;
                                Velocity *= fTerminalVelocity;
                            }
                            //Console.Out.WriteLine(Velocity);
                        }
                        else
                        {
                            WorldPosition = Velocity * fDelta;
                        }
                    }
                }

                //Console.Out.WriteLine(WorldPosition);
                //Console.Out.WriteLine(m_vCurrentPathTarget);
                //Console.Out.WriteLine(currentState);

                /****************************************************************
                 * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                 * AI LOGIC CODE
                 * REMINDER TO SELF: come up with a way to resolve or avoid 
                 * collisions in paths of the AI tanks.
                 * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                 ****************************************************************/
                if ((WorldPosition - m_vPlayerPosition).Length() < AUTOMATIC_DETECTION_RADIUS)
                {
                    //player has been automatically detected so set PlayerLastKnownPosition
                    //m_vPlayerLastKnownPosition = new Vector3(m_vPlayerPosition.X, m_vPlayerPosition.Y, m_vPlayerPosition.Z);

                    //change action flow based on current state and modify state
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
                    //reset turret
                    if (turretRotationValue != 0)
                    {
                        if (turretRotationValue + TURRET_ROTATION_SPEED * fDelta < 2 * Math.PI)
                        {
                            turretRotationValue += TURRET_ROTATION_SPEED * fDelta;
                        }
                        else
                        {
                            //finished or almost finished a full rotation of the turret so snap it to 0 so we can perform tasks
                            turretRotationValue = 0;
                        }
                    }
                    else
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
                            m_vCurrentPathTarget = (Vector3)path[1];    //target/world position is already the starting position of the path so get the next node
                        }
                        else
                        {
                            m_vCurrentPathTarget = (Vector3)path[path.IndexOf(m_vCurrentPathTarget) + 1];
                        }

                        //set the force vector to be in the right direction
                        UpdateForce();


                        currentState = AIStates.PATROL;
                    }
                }
                else if (currentState == AIStates.SCAN)
                {
                    //scan for the player
                    if (turretRotationValue + TURRET_ROTATION_SPEED * fDelta < 2 * Math.PI)
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
                        {
                            if (timer.GetNumberOfTimers() > 0)
                            {
                                //means we should be in pursuit mode
                                currentState = AIStates.PURSUE;
                            }
                            else
                            {
                                currentState = AIStates.NEED_PATROL;
                            }
                        }
                    }
                }
                else if (currentState == AIStates.NEED_PURSUE)
                {
                    //reset turret

                    if (turretRotationValue > fDelta * TURRET_ROTATION_SPEED)
                    {
                        if (turretRotationValue + TURRET_ROTATION_SPEED * fDelta < 2 * Math.PI)
                        {
                            turretRotationValue += TURRET_ROTATION_SPEED * fDelta;
                        }
                        else
                        {
                            //finished or almost finished a full rotation of the turret so snap it to 0 so we can perform tasks
                            turretRotationValue = 0;
                        }
                    }
                    else
                    {
                        turretRotationValue = 0;
                        timer.AddTimer("Stop Pursuit", PURSUIT_DURATION, StopPursuit, false);

                        m_vTarget = FindClosestNavNode(m_vPlayerLastKnownPosition);
                        path = navigation.GetPath(m_vCurrentPathTarget, m_vTarget);

                        currentState = AIStates.PURSUE;
                    }
                }
                else if (currentState == AIStates.PURSUE)
                {
                    //only scan for the player once we reach the navnode closest to its last known position
                    if (WorldPosition.Equals(m_vTarget))
                    {
                        currentState = AIStates.SCAN;

                        //pick a random place to look
                        m_vTarget = (Vector3)navNodes[rg.Next(navNodes.Count)];
                        path = navigation.GetPath(WorldPosition, m_vTarget);
                    }
                    else
                    {
                        if (WorldPosition.Equals(m_vCurrentPathTarget))
                        {
                            m_vCurrentPathTarget = (Vector3)path[path.IndexOf(m_vCurrentPathTarget) + 1];
                            UpdateForce();
                        }
                    }
                }
                else if (currentState == AIStates.ATTACK)
                {
                    if (CheckPlayerSighted())
                    {
                        //do attack
                        //Console.Out.WriteLine("Can see tank in ATTACK");
                        timer.RemoveTimer("Stop Pursuit");  //remove timer to prevent entering patrol mode too early
                    }
                    else
                    {
                        //lost sight, begin pursuit
                        currentState = AIStates.NEED_PURSUE;
                    }
                }

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
                else
                {
                    m_vWorldPosition += Vector3.Multiply(m_vVelocity, gameTime.ElapsedGameTime.Ticks / System.TimeSpan.TicksPerMillisecond / 1000.0f);

                    //TODO: Add World Bound check so the player doesn't fall off the world
                }*/

                //when the tank reaches its target, it performs a radial check for the player
                //if the player is within a minimum detection distance, the AI will instantly "discover" the player
                //AI tank has a 20 degree viewing angle for checking? maybe?
            }
            Console.Out.WriteLine("Current state: " + currentState);
        }

        //Helper functions to keep the Update method clean
        private bool CheckPlayerSighted()
        {
            Ray sightRay = new Ray(cannonBone.Transform.Translation, GetCannonFacing());
            //Console.Out.WriteLine(sightRay.Direction);
            bool seePlayer = false;
            
            foreach (Actor a in GameplayScreen.Instance.activeActors)
            {
                //Console.Out.WriteLine(GameplayScreen.Instance.activeActors.Count);
                if (a.COLLISION_IDENTIFIER != CollisionIdentifier.NONCOLLIDING)
                {
                    if (a.COLLISION_IDENTIFIER == CollisionIdentifier.BUILDING) 
                    {
                        Building b = (Building)a;
                        if (sightRay.Intersects(b.WorldBoundsBox) != null)
                            return false;
                    }
                    else if (sightRay.Intersects(a.WorldBounds) != null)
                    {
                        if (a.COLLISION_IDENTIFIER == CollisionIdentifier.PLAYER_TANK)
                        {
                            //Console.Out.WriteLine(a.WorldBounds);
                            //Console.Out.WriteLine("Can see player");
                            //m_vPlayerLastKnownPosition = new Vector3(a.WorldPosition.X, a.WorldPosition.Y, a.WorldPosition.Z);
                            seePlayer = true;
                        }
                        else
                            return false;
                    }
                }
            }
                    
            return seePlayer;
        }

        /// <summary>
        /// Finds the closest navigation node to the given position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns>Closest navigation node.</returns>
        private Vector3 FindClosestNavNode(Vector3 position)
        {
            Vector3 result = new Vector3(); //for compilation sake
            float shortestLength = float.MaxValue;
            foreach (Vector3 v in navNodes)
            {
                float length = (v - position).Length();
                if (length < shortestLength)
                {
                    shortestLength = length;
                    result = v;
                }
            }
            return result;
        }

        private void StopPursuit()
        {
            currentState = AIStates.NEED_PATROL;
        }

        public override Vector3 GetWorldFacing()
        {
            Vector3 result = worldTransform.Forward;
            result.X *= -1; //I don't even know
            return result;
        }

        public Vector3 GetCannonFacing()
        {
            Vector3 result = (worldTransform * Matrix.CreateRotationY(turretRotationValue)).Forward;
            result.X *= -1;
            result.Z *= -1;
            result.Normalize();
            return result;
        }
        /// <summary>
        /// Update the force vector to move the tank towards the new path target
        /// </summary>
        private void UpdateForce()
        {
            Force = (m_vCurrentPathTarget - WorldPosition);
            Vector3 tempForce = Force;
            //figure out how much we need to rotate by and whether the rotation should CW or CCW
            Vector3 Facing = GetWorldFacing();
            tempForce.Normalize();
            Facing.Normalize();
            //Console.Out.WriteLine(Force);
            //Console.Out.WriteLine(Facing);
            float deltaRotationValue = (float)Math.Acos((Double)Vector3.Dot(tempForce, Facing));
            //Console.Out.WriteLine(Vector3.Dot(tempForce, Facing));
            //Console.Out.WriteLine(deltaRotationValue);
            if (Vector3.Cross(Force, Facing).Y > 0.0f)
                targetTankRotationValue = RotAngle + deltaRotationValue;
            else
                targetTankRotationValue = RotAngle - deltaRotationValue;

            //set the magnitude of the Force vector
            Force = tempForce * fTerminalVelocity;

            //Console.Out.WriteLine("Force: " +Force);
        }

        /// <summary>
        /// Resolves collision based on defined behaviors.
        /// </summary>
        /// <param name="a">Actor with which it is currently colliding.</param>
        public override void collide(Actor a)
        {
            if (a is Projectile)
            {
                Projectile temp = (Projectile)a;
                CurrentHealth -= temp.Damage;
                if (CurrentHealth <= 0.0f)
                {
                    currentState = AIStates.DEAD;
                }
            }
        }

    }
}

