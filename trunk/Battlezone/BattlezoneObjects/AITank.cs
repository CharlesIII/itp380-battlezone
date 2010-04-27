using System;
using System.Collections;
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

        const string CANNON_FIRE = "FireCannon";
        const string CANNON_RELOAD = "TankShellReload";
        const string TANK_IDLE = "TankIdle";
        const string TANK_ENGINE_MOVE = "TankEngineMoving";
        const string TANK_TREAD_MOVE = "TankTreadRolling";

        Cue TankEngineIdleCue;
        Cue TankEngineMovingCue;
        Cue TankTreadRollingCue;
        Cue TankCannonFireCue;
        Cue TankCannonReloadCue;

        float DistanceFromCamera;

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

        enum AIStates {NEED_PURSUE, PURSUE, PATROL, NEED_PATROL, SCAN, ATTACK, DEAD, STOP};
        AIStates currentState;
        AIStates previousState;

        float turretRotationValue;
        float turretTargetRotationValue;

        float targetTankRotationValue;

        bool canFire;
        bool canRotate;

        List<AITank> collidingAITanks;
        
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

            Scale = 59.0f;

            canFire = true;
            canRotate = true;
            turretTargetRotationValue = 0;

            collidingAITanks = new List<AITank>(10);

            TankEngineIdleCue = GameplayScreen.Instance.audioManager.Play3DCue(TANK_IDLE,this);
            TankTreadRollingCue = GameplayScreen.Instance.audioManager.Play3DCue(TANK_TREAD_MOVE,this);
            TankEngineMovingCue = GameplayScreen.Instance.audioManager.Play3DCue(TANK_ENGINE_MOVE,this);
            TankCannonFireCue = GameplayScreen.Instance.audioManager.Play3DCue(CANNON_FIRE,this);
            TankCannonReloadCue = GameplayScreen.Instance.audioManager.Play3DCue(CANNON_RELOAD,this);

            DistanceFromCamera = (GameplayScreen.CameraMatrix.Translation - WorldPosition).Length();

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

            DistanceFromCamera = (GameplayScreen.CameraMatrix.Translation - WorldPosition).Length();
            
            if (currentState == AIStates.STOP || currentState == AIStates.DEAD)
                return;

            if ((WorldPosition - m_vPlayerPosition).Length() < AUTOMATIC_DETECTION_RADIUS)
            {
                //player has been automatically detected so set PlayerLastKnownPosition
                m_vPlayerLastKnownPosition = new Vector3(m_vPlayerPosition.X, m_vPlayerPosition.Y, m_vPlayerPosition.Z);

                //change action flow based on current state and modify state
                //perform quick instant vision check
                //if visible, set turretRotationTarget and stop moving and set state to attack
                //if not visisble, find the closest nav node to the player position and navigate to it and set state to pursue
                /*
                if (currentState == AIStates.STOP)
                {
                    previousState = AIStates.NEED_PURSUE;
                }
                 */
            }

            //make sure we're facing the right direction before we start moving
            if (targetTankRotationValue > 0)
            {
                //we need to rotate CCW
                RotAngle += TANK_ROTATION_SPEED * fDelta;
                targetTankRotationValue -= TANK_ROTATION_SPEED * fDelta;
                //clamp RotAngle to target if close enough
                if (Math.Abs(targetTankRotationValue) <= 0.10f)
                    targetTankRotationValue = 0;
                Quat = Quaternion.CreateFromAxisAngle(Vector3.UnitY, RotAngle);
                Console.Out.WriteLine(GetWorldFacing());
            }
            else if (targetTankRotationValue < 0)
            {
                //we need to rotate CW
                RotAngle -= TANK_ROTATION_SPEED * fDelta;
                targetTankRotationValue += TANK_ROTATION_SPEED * fDelta;

                //clamp RotAngle to target if close enough
                if (Math.Abs(targetTankRotationValue) <= 0.10f)
                    targetTankRotationValue = 0;
                Quat = Quaternion.CreateFromAxisAngle(Vector3.UnitY, RotAngle);
                Console.Out.WriteLine(GetWorldFacing());
            }
            else
            {
                //we're facing the right direction do movement logic
                if ((m_vCurrentPathTarget - WorldPosition).Length() < 5.0f)
                {
                    //we're close enough so stop moving and snap position
                    Velocity = new Vector3(0.0f);
                    WorldPosition = m_vCurrentPathTarget;
                    if (TankEngineIdleCue.IsStopped)
                        TankEngineIdleCue = GameplayScreen.Instance.audioManager.Play3DCue(TANK_IDLE,this);
                    if (TankEngineIdleCue.IsPrepared)
                    {
                        TankEngineIdleCue.Play();
                    }
                    if (TankEngineMovingCue.IsPlaying)
                        TankEngineMovingCue.Stop(AudioStopOptions.Immediate);
                    if (TankTreadRollingCue.IsPlaying)
                        TankTreadRollingCue.Stop(AudioStopOptions.Immediate);
                }
                else
                {
                    if (bPhysicsDriven)
                    {
                        Velocity += vAcceleration * fDelta / 2.0f;
                        m_vPreviousWorldPosition = m_vWorldPosition;
                        WorldPosition += Velocity * fDelta;
                        vAcceleration = vForce / fMass;
                        Velocity += vAcceleration * fDelta / 2.0f;
                        if (Velocity.Length() >= fTerminalVelocity)
                        {
                            Vector3 temp = Velocity;
                            temp.Normalize();
                            Velocity = temp;
                            Velocity *= fTerminalVelocity;
                        }
                        if (TankEngineMovingCue.IsStopped)
                            TankEngineMovingCue = GameplayScreen.Instance.audioManager.Play3DCue(TANK_ENGINE_MOVE,this);
                        if (TankTreadRollingCue.IsStopped)
                            TankTreadRollingCue = GameplayScreen.Instance.audioManager.Play3DCue(TANK_TREAD_MOVE,this);

                        if (TankEngineIdleCue.IsPlaying)
                            TankEngineIdleCue.Stop(AudioStopOptions.Immediate);
                        if (TankEngineMovingCue.IsPrepared)
                        {
                            TankEngineMovingCue.SetVariable("Distance", DistanceFromCamera);
                            TankEngineMovingCue.Play();
                        }
                        if (TankTreadRollingCue.IsPrepared)
                        {
                            TankTreadRollingCue.SetVariable("Distance", DistanceFromCamera);
                            TankTreadRollingCue.Play();
                        }
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
                        //currentState = AIStates.ATTACK;
                    }
                }
                else
                {
                    //finished or almost finished a full rotation of the turret so snap it to 0 and perform once last scan
                    turretRotationValue = 0;

                    if (CheckPlayerSighted())
                    {
                        //currentState = AIStates.ATTACK;
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
                if (Math.Abs(turretTargetRotationValue) > 0)
                {
                    if (canRotate)
                    {
                        if (turretTargetRotationValue < 0)
                        {
                            if (turretTargetRotationValue < 0.15)
                                turretRotationValue -= fDelta * TURRET_ROTATION_SPEED / 2;
                            else
                                turretRotationValue -= fDelta * TURRET_ROTATION_SPEED;
                            turretTargetRotationValue += fDelta * TURRET_ROTATION_SPEED;
                            if (turretTargetRotationValue >= 0)
                                turretTargetRotationValue = 0;
                        }
                        else
                        {
                            if (turretTargetRotationValue < 0.15)
                                turretRotationValue += fDelta * TURRET_ROTATION_SPEED / 2;
                            else
                                turretRotationValue += fDelta * TURRET_ROTATION_SPEED;
                            turretTargetRotationValue -= fDelta * TURRET_ROTATION_SPEED;
                            if (turretTargetRotationValue <= 0)
                                turretTargetRotationValue = 0;
                        }
                    }
                }
                else if (CheckPlayerSighted())
                {
                    //do attack
                    //Console.Out.WriteLine("Can see tank in ATTACK");
                    if (canFire)
                    {
                        Vector3 pos = WorldPosition + new Vector3(0, 95, 0) + (turretTransform * cannonTransform).Translation;
                        Projectile pro = new Projectile(pos, GetCannonFacing(), Game, Projectile.PROJECTILE_TYPE.SHELL, CollisionIdentifier.AI_TANK);
                        pro.Initialize(400.0f, 250, 190, 100.0f, 100.0f, 0.0f);
                        Game.Components.Add(pro);
                        
                        if (TankCannonFireCue.IsStopped)
                            TankCannonFireCue = GameplayScreen.Instance.audioManager.Play3DCue(CANNON_FIRE,this);
                        TankCannonFireCue.Play();
                        TankCannonReloadCue = GameplayScreen.Instance.audioManager.Play3DCue(CANNON_RELOAD,this);

                        canFire = false;
                        canRotate = false;
                        timer.AddTimer("Enable Cannon", 3, AllowFire, false);
                        timer.AddTimer("Enable Rotation", 0.75f, AllowRotate, false);
                    }
                    timer.RemoveTimer("Stop Pursuit");  //remove timer to prevent entering patrol mode too early
                }
                else
                {
                    //lost sight, begin pursuit
                    currentState = AIStates.NEED_PURSUE;
                }
            }
            if (canFire == false && TankCannonFireCue.IsStopped && TankCannonReloadCue.IsPrepared)
            {
                //cannon was fired and cannon fire sound is over
                TankCannonReloadCue.SetVariable("Distance", DistanceFromCamera);
                TankCannonReloadCue.Play();
            }

            //check to see if we had collided with any AITanks previously and notify them if we're far away enough to avoid collision
            if (collidingAITanks.Count > 0)
            {
                List<AITank> tanksToRemove = new List<AITank>(10);
                foreach (AITank tank in collidingAITanks)
                {
                    Vector3 distance = WorldPosition - tank.WorldPosition;
                    float requiredDistanceApart = WorldBounds.Radius + tank.WorldBounds.Radius + 40;
                    if (distance.Length() >= requiredDistanceApart)
                    {
                        tank.msgCollisionResolved(this);
                        tanksToRemove.Add(tank);
                    }
                }

                //remove any tanks we have resolved collision with
                foreach (AITank removedTank in tanksToRemove)
                {
                    collidingAITanks.Remove(removedTank);
                }
            }

            //TODO: Add World Bound check so the player doesn't fall off the world

            //when the tank reaches its target, it performs a radial check for the player
            //if the player is within a minimum detection distance, the AI will instantly "discover" the player
            //AI tank has a 20 degree viewing angle for checking? maybe?
            //Console.Out.WriteLine(WorldBounds);
            //Console.Out.WriteLine("Current state: " + currentState);
        }

        //Helper functions to keep the Update method clean
        private bool CheckPlayerSighted()
        {
            return CheckPlayerSighted(null);
        }

        private bool CheckPlayerSighted(Vector3? direction)
        {
            Ray sightRay;
            if (direction == null)
                sightRay = new Ray(WorldPosition, GetCannonFacing());
            else
                sightRay = new Ray(WorldPosition, (Vector3)direction);

            bool seePlayer = false;
            Actor player = null;

            //find out what the distance to the player is if we can see them and compare it to the distance we can see some other object
            float? distanceToPlayer = null;
            float? distanceToAnotherObject = null;
            foreach (Actor a in GameplayScreen.Instance.activeActors)
            {
                if (a.COLLISION_IDENTIFIER != CollisionIdentifier.NONCOLLIDING && a.COLLISION_IDENTIFIER != CollisionIdentifier.AI_TANK)
                {
                    if (a.COLLISION_IDENTIFIER == CollisionIdentifier.BUILDING) 
                    {
                        Building b = (Building)a;
                        distanceToAnotherObject = sightRay.Intersects(b.WorldBoundsBox);
                    }
                    else if (a.COLLISION_IDENTIFIER == CollisionIdentifier.PLAYER_TANK)
                    {
                        distanceToPlayer = sightRay.Intersects(a.WorldBounds);
                        player = a;
                    }
                    else
                    {
                        float? temp = sightRay.Intersects(a.WorldBounds);
                        if (temp < distanceToAnotherObject || distanceToAnotherObject == null)
                        {
                            distanceToAnotherObject = temp;
                        }
                    }
                }
            }
            //Console.Out.WriteLine("Player distance: " + distanceToPlayer);
            //Console.Out.WriteLine("Object distance: " + distanceToAnotherObject);
            //if the player is closer than the closest object we can see, then we can see the player
            if (distanceToAnotherObject == null && distanceToPlayer != null)
            {
                //can't see any other objects, but the player
                seePlayer = true;
            }
            else if (distanceToPlayer == null)
            {
                //can't see the player
                return false;
            }
            else if (distanceToPlayer < distanceToAnotherObject)
                seePlayer = true;
            
            if (seePlayer == true)
            {
                Vector3 correctFacing = player.WorldPosition - WorldPosition;
                correctFacing.Normalize();
                turretTargetRotationValue = (float)Math.Acos((double)Vector3.Dot(correctFacing, GetCannonFacing()));
                if (turretTargetRotationValue < 0.01f)
                    turretTargetRotationValue = 0;
                Vector3 cross = Vector3.Cross(GetCannonFacing(), correctFacing);
                if (cross.Y < 0)
                    turretTargetRotationValue *= -1;

                m_vPlayerLastKnownPosition = new Vector3(m_vPlayerPosition.X, m_vPlayerPosition.Y, m_vPlayerPosition.Z);
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

        private void AllowFire()
        {
            canFire = true;
        }

        private void AllowRotate()
        {
            canRotate = true;
        }

        public override Vector3 GetWorldFacing()
        {
            Vector3 result = worldTransform.Forward;
            result.X *= -1; //I don't even know
            result.Z *= -1;
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
            Vector3 tempForce = (m_vCurrentPathTarget - WorldPosition);
            Vector3 Facing = GetWorldFacing();
            
            tempForce.Normalize();
            Facing.Normalize();

            float deltaRotationValue = (float)Math.Acos((Double)Vector3.Dot(tempForce, Facing));

            //figure out how much we need to rotate by and whether the rotation should CW or CCW
            if (Vector3.Cross(Facing, tempForce).Y > 0.0f)
                targetTankRotationValue = deltaRotationValue;
            else
                targetTankRotationValue = - deltaRotationValue;
            
            //set the magnitude of the Force vector
            Force = tempForce * fTerminalVelocity;
        }

        public void msgWeAreColliding(AITank tank)
        {
            collidingAITanks.Add(tank);
            previousState = currentState;
            currentState = AIStates.STOP;
            WorldPosition = m_vPreviousWorldPosition;
        }

        public void msgCollisionResolved(AITank tank)
        {
            collidingAITanks.Remove(tank);
            currentState = previousState;
        }

        /// <summary>
        /// Resolves collision based on defined behaviors.
        /// </summary>
        /// <param name="a">Actor with which it is currently colliding.</param>
        public override void collide(Actor a)
        {
            if (a.COLLISION_IDENTIFIER == CollisionIdentifier.SHELL)
            {
                Projectile temp = (Projectile)a;
                CurrentHealth -= temp.Damage;
                if (CurrentHealth <= 0.0f)
                {
                    currentState = AIStates.DEAD;
                    GameplayScreen.Instance.Enemies.Remove(this);
                }
            }
            else if (a.COLLISION_IDENTIFIER == CollisionIdentifier.AI_TANK)
            {
                /*
                 * When two tanks collide, one of them will have their collide method called first. This check 
                 * prevents the later tank from causing the earlier tank to stop.
                 */
                AITank ai = (AITank)a;
                if (!collidingAITanks.Contains(ai))
                {
                    collidingAITanks.Add(ai);
                    ai.msgWeAreColliding(this);
                }
            }
            else if (a.COLLISION_IDENTIFIER == CollisionIdentifier.PLAYER_TANK)
            {
                //this really shouldn't happen since AITank should have discovered the player tank due to minimum-detection-radius check
                //I'm not sure if we really need to handle this case
            }
            else if (a.COLLISION_IDENTIFIER == CollisionIdentifier.BUILDING)
            {
                //Console.Out.WriteLine("Colliding with a building, resolution undefined.");
                currentState = AIStates.STOP;
            }
        }

        public override bool checkCollision(Actor a)
        {
            if (a.COLLISION_IDENTIFIER == CollisionIdentifier.BUILDING)
            {
                Building b = (Building)a;
                return WorldBounds.Intersects(b.WorldBoundsBox);
            }
            else if (a.COLLISION_IDENTIFIER == CollisionIdentifier.PLAYER_TANK || a.COLLISION_IDENTIFIER == CollisionIdentifier.AI_TANK)
            {
                return WorldBounds.Intersects(a.WorldBounds);
            }
            else
            {
                return false;
            }
        }

    }
}

