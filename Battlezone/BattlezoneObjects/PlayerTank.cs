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
    /// Inherits from Actor. This is the tank controlled by the player. Should contain everything
    /// needed by a player.
    /// </summary>
    public class PlayerTank : Actor
    {

        #region Bone Fields


        const float TANK_VELOCITY = 550.0f;
        // The XNA framework Model object that we are going to display.
        Model tankModel;


        // Shortcut references to the bones that we are going to animate.
        // We could just look these up inside the Draw method, but it is more
        // efficient to do the lookups while loading and cache the results.
        ModelBone leftBackWheelBone;
        ModelBone rightBackWheelBone;
        ModelBone leftFrontWheelBone;
        ModelBone rightFrontWheelBone;
        ModelBone leftSteerBone;
        ModelBone rightSteerBone;
        public ModelBone turretBone;
        ModelBone cannonBone;
        ModelBone hatchBone;


        // Store the original transform matrix for each animating bone.
        Matrix leftBackWheelTransform;
        Matrix rightBackWheelTransform;
        Matrix leftFrontWheelTransform;
        Matrix rightFrontWheelTransform;
        Matrix leftSteerTransform;
        Matrix rightSteerTransform;
        public Matrix turretTransform;
        Matrix cannonTransform;
        Matrix hatchTransform;


        // Array holding all the bone transform matrices for the entire model.
        // We could just allocate this locally inside the Draw method, but it
        // is more efficient to reuse a single array, as this avoids creating
        // unnecessary garbage.
        //Matrix[] boneTransforms;      ----Using inherited matrix from Actor----


        // Current animation positions.
        float LwheelRotationValue;
        float RwheelRotationValue;
        float steerRotationValue;
        float turretRotationValue;
        float cannonRotationValue;
        float hatchRotationValue;
        public float turnDirection = 1.0f;
        public bool isColliding = false;

        public bool gamePlay = false;


        private Vector3 startingPos;
        String name;

        Cue soundCue;
        Cue treadsRollingCue;

        enum EngineState { IDLE, SPEEDUP, SLOWDOWN, MOVING };
        EngineState currentState;

        private ParticleSystem tankCannonPlumeParticleSystem;
        public ParticleSystem explosionParticles;
        public ParticleSystem explosionSmokeParticles;

        #endregion

        #region Bone Properties


        /// <summary>
        /// Gets or sets the wheel rotation amount.
        /// </summary>
        public float LWheelRotation
        {
            get { return LwheelRotationValue; }
            set { LwheelRotationValue = value; }
        }

        /// <summary>
        /// Gets or sets the wheel rotation amount.
        /// </summary>
        public float RWheelRotation
        {
            get { return RwheelRotationValue; }
            set { RwheelRotationValue = value; }
        }


        /// <summary>
        /// Gets or sets the steering rotation amount.
        /// </summary>
        public float SteerRotation
        {
            get { return steerRotationValue; }
            set { steerRotationValue = value; }
        }



        /// <summary>
        /// Gets or sets the turret rotation amount.
        /// </summary>
        public float TurretRotation
        {
            get { return turretRotationValue; }
            set { turretRotationValue = value; }
        }


        /// <summary>
        /// Gets or sets the cannon rotation amount.
        /// </summary>
        public float CannonRotation
        {
            get { return cannonRotationValue; }
            set { cannonRotationValue = value; }
        }


        /// <summary>
        /// Gets or sets the entry hatch rotation amount.
        /// </summary>
        public float HatchRotation
        {
            get { return hatchRotationValue; }
            set { hatchRotationValue = value; }
        }

        private bool m_bRotateLeft;
        public bool RotateLeft
        {
            get
            {
                return m_bRotateLeft;
            }
            set
            {
                if (m_bRotateRight)
                    m_bRotateRight = false;
                m_bRotateLeft = value;
            }
        }
        private bool m_bRotateRight;
        public bool RotateRight
        {
            get
            {
                return m_bRotateRight;
            }
            set
            {
                if (m_bRotateLeft)
                    m_bRotateLeft = false;
                m_bRotateRight = value;
            }
        }
        private bool m_bMove;
        public bool Move
        {
            get
            {
                return m_bMove;
            }
            set
            {
                if (m_bReverse)
                    m_bReverse = false;
                m_bMove = value;
            }
        }
        private bool m_bReverse;
        public bool Reverse
        {
            get
            {
                return m_bReverse;
            }
            set
            {
                if (m_bMove)
                    m_bMove = false;
                m_bReverse = value;
            }
        }

        #endregion

        float Delta = 0;

        /// <summary>
        /// Construtor for the Player Tank
        /// </summary>
        /// <param name="game">Reference to the Game</param>
        /// <param name="spawnPos">Spawning Position of the Tank</param>
        public PlayerTank(Game game, Vector3 spawnPos)
            : base(game)
        {
            sMeshToLoad = "playerTank";
            startingPos = spawnPos;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
            Scale = 0.30f;
            WorldPosition = startingPos;

            COLLISION_IDENTIFIER = CollisionIdentifier.PLAYER_TANK;

        }

        /// <summary>
        /// Loads the tank model.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            tankModel = ActorModel;  

            // Look up shortcut references to the bones we are going to animate.
            leftBackWheelBone = tankModel.Bones["l_back_wheel_geo"];
            rightBackWheelBone = tankModel.Bones["r_back_wheel_geo"];
            leftFrontWheelBone = tankModel.Bones["l_front_wheel_geo"];
            rightFrontWheelBone = tankModel.Bones["r_front_wheel_geo"];
            leftSteerBone = tankModel.Bones["l_steer_geo"];
            rightSteerBone = tankModel.Bones["r_steer_geo"];
            turretBone = tankModel.Bones["turret_geo"];
            cannonBone = tankModel.Bones["canon_geo"];
            hatchBone = tankModel.Bones["hatch_geo"];

            // Store the original transform matrix for each animating bone.
            leftBackWheelTransform = leftBackWheelBone.Transform;
            rightBackWheelTransform = rightBackWheelBone.Transform;
            leftFrontWheelTransform = leftFrontWheelBone.Transform;
            rightFrontWheelTransform = rightFrontWheelBone.Transform;
            leftSteerTransform = leftSteerBone.Transform;
            rightSteerTransform = rightSteerBone.Transform;
            turretTransform = turretBone.Transform;
            cannonTransform = cannonBone.Transform;
            hatchTransform = hatchBone.Transform;

            // Allocate the transform matrix array.
            boneTransforms = new Matrix[tankModel.Bones.Count];

            currentState = EngineState.IDLE;

            tankCannonPlumeParticleSystem = new TankCannonPlumeParticleSystem(Game, meshLoader);

            Game.Components.Add(this.tankCannonPlumeParticleSystem);

        }

        protected override void UnloadContent()
        {
            Console.Out.WriteLine("Player_tank is unloading content");
            base.UnloadContent();
        }

        public void ManualUnload()
        {
            UnloadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            if (explosionParticles != null && explosionSmokeParticles != null)
            {
                explosionParticles.SetCamera(GameplayScreen.CameraMatrix, GameplayScreen.ProjectionMatrix);
                explosionSmokeParticles.SetCamera(GameplayScreen.CameraMatrix, GameplayScreen.ProjectionMatrix);
            }
            if (dead)
                return;
            base.updateWorldTransform();

            // Set the world matrix as the root transform of the model.
            tankModel.Root.Transform = worldTransform;

            // Calculate matrices based on the current animation position.
            Matrix LwheelRotation = Matrix.CreateRotationX(LwheelRotationValue);
            Matrix RwheelRotation = Matrix.CreateRotationX(RwheelRotationValue);
            Matrix steerRotation = Matrix.CreateRotationY(steerRotationValue);
            Matrix turretRotation = Matrix.CreateRotationY(turretRotationValue);
            Matrix cannonRotation = Matrix.CreateRotationX(cannonRotationValue);
            Matrix hatchRotation = Matrix.CreateRotationX(hatchRotationValue);

            // Apply matrices to the relevant bones.
            leftBackWheelBone.Transform = LwheelRotation * leftBackWheelTransform;
            rightBackWheelBone.Transform = RwheelRotation * rightBackWheelTransform;
            leftFrontWheelBone.Transform = LwheelRotation * leftFrontWheelTransform;
            rightFrontWheelBone.Transform = RwheelRotation * rightFrontWheelTransform;
            leftSteerBone.Transform = steerRotation * leftSteerTransform;
            rightSteerBone.Transform = steerRotation * rightSteerTransform;
            turretBone.Transform = turretRotation * turretTransform;
            cannonBone.Transform = cannonRotation * cannonTransform;
            hatchBone.Transform = hatchRotation * hatchTransform;

            // Look up combined bone matrices for the entire model.
            tankModel.CopyAbsoluteBoneTransformsTo(boneTransforms);

            GraphicsDevice.RenderState.DepthBufferEnable = true;

            // Draw the model.
            foreach (ModelMesh mesh in tankModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index];
                    if (gamePlay)
                    {
                        effect.View = GameplayScreen.CameraMatrix;
                        effect.Projection = GameplayScreen.ProjectionMatrix;
                    }
                    else
                    {
                        effect.View = BackgroundScreen.CameraMatrix;
                        effect.Projection = BackgroundScreen.ProjectionMatrix;
                    }

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
            timer.Update(gameTime);
            //Console.Out.WriteLine(WorldPosition);
            //WorldBounds.Center = WorldPosition + new Vector3(0,0, turretTransform.Translation.Z);
            //Console.Out.WriteLine(WorldBounds.Center);
            //WorldBounds.Radius = ModelBounds.Radius * Scale;
            if (dead)
            {

                    soundCue.Stop(AudioStopOptions.Immediate);
                return;
            }

            Delta = gameTime.ElapsedGameTime.Ticks / System.TimeSpan.TicksPerMillisecond / 1000.0f;
            tankCannonPlumeParticleSystem.SetCamera(GameplayScreen.CameraMatrix, GameplayScreen.ProjectionMatrix);
            if (soundCue == null && gamePlay)
            {
                soundCue = ScreenManager.soundSoundBank.GetCue("TankIdle");

                soundCue.Apply3D(GameplayScreen.Instance.Camera.Listener, emitter);
                soundCue.Play();
            }
            else
            {

                if (gamePlay)
                {
                    if (dead) return;
                    if (Move && !isColliding)
                    {
                        Velocity = GetWorldFacing() * -TANK_VELOCITY * GameplayScreen.Instance.spdBoost;
                    }
                    else if (Reverse && !isColliding)
                    {
                        Velocity = GetWorldFacing() * TANK_VELOCITY * GameplayScreen.Instance.spdBoost;
                    }
                    //else if (Move && isColliding)
                    //{
                        //Velocity = GetWorldFacing() * -TANK_VELOCITY * GameplayScreen.Instance.spdBoost;
                    //}
                    //else if (Reverse && isColliding)
                    //{

                    //}
                    else
                    {
                        Velocity = Vector3.Zero;
                    }
                       
                    if (Velocity == Vector3.Zero)
                    {
                        if (soundCue.IsStopped && currentState == EngineState.SLOWDOWN)
                        {
                            soundCue = ScreenManager.soundSoundBank.GetCue("TankIdle");

                            soundCue.Apply3D(GameplayScreen.Instance.Camera.Listener, emitter);
                            soundCue.Play();
                            //soundCue.Stop(AudioStopOptions.AsAuthored);
                            //soundCue = GameplayScreen.Instance.audioManager.Play3DCue("TankIdle", this);
                            currentState = EngineState.IDLE;
                        }
                        else if (soundCue.IsStopped && currentState == EngineState.IDLE)
                        {
                            soundCue.Stop(AudioStopOptions.AsAuthored);
                            soundCue = ScreenManager.soundSoundBank.GetCue("TankIdle");

                            soundCue.Apply3D(GameplayScreen.Instance.Camera.Listener, emitter);
                            soundCue.Play();
                            //soundCue = GameplayScreen.Instance.audioManager.Play3DCue("TankIdle", this);
                            currentState = EngineState.IDLE;
                        }
                        else if (currentState == EngineState.MOVING)
                        {
                            soundCue.Stop(AudioStopOptions.Immediate);
                            soundCue = ScreenManager.soundSoundBank.GetCue("TankSlowDown");

                            soundCue.Apply3D(GameplayScreen.Instance.Camera.Listener, emitter);
                            soundCue.Play();
                            //soundCue = GameplayScreen.Instance.audioManager.Play3DCue("TankSlowDown", this);
                            currentState = EngineState.SLOWDOWN;
                        }
                        else if (currentState == EngineState.SPEEDUP)
                        {
                            soundCue.Stop(AudioStopOptions.Immediate);
                            soundCue = ScreenManager.soundSoundBank.GetCue("TankSlowDown");

                            soundCue.Apply3D(GameplayScreen.Instance.Camera.Listener, emitter);
                            soundCue.Play();
                            //soundCue = GameplayScreen.Instance.audioManager.Play3DCue("TankSlowDown", this);
                            currentState = EngineState.SLOWDOWN;
                        }
                    }
                    else
                    {
                        if (soundCue.IsStopped && currentState == EngineState.SPEEDUP)
                        {
                            soundCue = ScreenManager.soundSoundBank.GetCue("TankEngineMoving");

                            soundCue.Apply3D(GameplayScreen.Instance.Camera.Listener, emitter);
                            soundCue.Play();
                            
                            //soundCue = GameplayScreen.Instance.audioManager.Play3DCue("TankEngineMoving", this);
                            currentState = EngineState.MOVING;
                        }
                        else if (soundCue.IsStopped && currentState == EngineState.MOVING)
                        {
                            soundCue = ScreenManager.soundSoundBank.GetCue("TankEngineMoving");

                            soundCue.Apply3D(GameplayScreen.Instance.Camera.Listener, emitter);
                            soundCue.Play();
                            //soundCue = GameplayScreen.Instance.audioManager.Play3DCue("TankEngineMoving", this);
                            currentState = EngineState.MOVING;
                        }
                        else if (currentState == EngineState.IDLE)
                        {
                            soundCue.Stop(AudioStopOptions.Immediate);
                            soundCue = ScreenManager.soundSoundBank.GetCue("TankSpeedUp");

                            soundCue.Apply3D(GameplayScreen.Instance.Camera.Listener, emitter);
                            soundCue.Play();
                            //soundCue = GameplayScreen.Instance.audioManager.Play3DCue("TankSpeedUp", this);
                            currentState = EngineState.SPEEDUP;
                        }
                        else if (currentState == EngineState.SLOWDOWN)
                        {
                            soundCue.Stop(AudioStopOptions.Immediate);
                            soundCue = ScreenManager.soundSoundBank.GetCue("TankSpeedUp");

                            soundCue.Apply3D(GameplayScreen.Instance.Camera.Listener, emitter);
                            soundCue.Play();
                            //soundCue = GameplayScreen.Instance.audioManager.Play3DCue("TankSpeedUp", this);
                            currentState = EngineState.SPEEDUP;
                        }
                    }
                }
            }

            isColliding = false;
            base.Update(gameTime);
        }
        public void UpdateTankCannonSmoke()
        {
            // This is trivial: we just create one new smoke particle per frame.
            const int smokeParticlesPerFrame = 100;


            for (int i = 0; i < smokeParticlesPerFrame; i++)
            {
                tankCannonPlumeParticleSystem.AddParticle((turretBone.Transform * worldTransform).Translation, GetWorldFacing() * 500);
            }
        }

        /// <summary>
        /// Checks collision between this actor and the given actor.
        /// </summary>
        /// <param name="a">Actor to check collision with.</param>
        /// <returns>True if the two actors are colliding.</returns>
        public override bool checkCollision(Actor a)
        {
            if (a.COLLISION_IDENTIFIER == CollisionIdentifier.BUILDING)
            {
                Building b = (Building)a;
                if (buildingIntersect(b))
                {
                    System.Console.Out.WriteLine("Collision with Building Detected");
                    return true;
                }
                return false;
            }
            else if (a.COLLISION_IDENTIFIER == CollisionIdentifier.AI_TANK)
            {
                if (a.WorldBounds.Intersects(WorldBounds))
                    return true;
                return false;
            }
            else 
                return false;
        }

        /// <summary>
        /// Resolves collision based on defined behaviors.
        /// </summary>
        /// <param name="a">Actor with which it is currently colliding.</param>
        public override void collide(Actor a)
        {
            if (a.COLLISION_IDENTIFIER == CollisionIdentifier.AI_TANK)
            {
                isColliding = true;
                Velocity = new Vector3(0.0f, 0.0f, 0.0f);
                WorldPosition = m_vPreviousWorldPosition;
                //System.Console.Out.WriteLine("TANKS CRASHED!");
            }
            else if (a.COLLISION_IDENTIFIER == CollisionIdentifier.BUILDING)
            {
                isColliding = true;
                
                
                Building b = (Building)a;

                Plane wall = findIntersectingPlane(b);
                Console.Out.WriteLine(wall);
                Vector3 wallNormal = wall.Normal;
                wallNormal.Normalize();
                //perpVelComp = Perpendicular Velocity Component (perpendicular to plane)
                Vector3 perpVelComp = Vector3.Dot(Velocity, wallNormal * -1.0f) * wallNormal;
                
                //System.Console.Out.WriteLine("Pre: " + Velocity);
                Velocity += perpVelComp;
                WorldPosition = m_vPreviousWorldPosition;
                WorldPosition += Velocity * Delta;

                //WorldPosition += Velocity;
                //System.Console.Out.WriteLine("Post: " + Velocity + " diff: " + perpVelComp);
                //Console.Out.WriteLine(b.WorldPosition);
            }
            else if (a.COLLISION_IDENTIFIER == CollisionIdentifier.SHELL)
            {
                Projectile temp = (Projectile)a;
                if (a.dead)
                {
                    CurrentHealth -= temp.Damage;
                    System.Console.Out.WriteLine("Ouch");
                }
                if (this.CurrentHealth <= 0.0f)
                {
                    this.playerDeath();
                    timer.AddTimer("Respawn", 10.0f, new Utils.TimerDelegate(respawnPlayer), false);

                    explosionParticles = new ExplosionParticleSystemTank(Game, meshLoader);
                    explosionSmokeParticles = new ExplosionSmokeParticleSystemTank(Game, meshLoader);

                    // Set the draw order so the explosions and fire
                    // will appear over the top of the smoke.
                    explosionSmokeParticles.DrawOrder = 200;
                    explosionParticles.DrawOrder = 400;

                    // Register the particle system components.
                    Game.Components.Add(explosionParticles);
                    Game.Components.Add(explosionSmokeParticles);

                    for (int i = 0; i < 1000; i++)
                        explosionParticles.AddParticle(WorldPosition, new Vector3());

                    for (int i = 0; i < 1600; i++)
                        explosionSmokeParticles.AddParticle(WorldPosition, new Vector3());
                    Cue c = ScreenManager.soundSoundBank.GetCue("TankExplosion");

                    c.Apply3D(GameplayScreen.Instance.Camera.Listener, emitter);
                    c.Play();
                }

                
            }
        }

        /// <summary>
        /// Checks if the player is intersecting with a building.
        /// </summary>
        /// <param name="b">Building object to check intersection against.</param>
        /// <param name="wall">out variable of type Plane.</param>
        /// <returns></returns>
        public bool buildingIntersect(Building b)
        {
            float minX = Math.Min(b.WorldBoundsBox.Min.X, b.WorldBoundsBox.Max.X);
            float minZ = Math.Min(b.WorldBoundsBox.Min.Z, b.WorldBoundsBox.Max.Z);
            float maxX = Math.Max(b.WorldBoundsBox.Min.X, b.WorldBoundsBox.Max.X);
            float maxZ = Math.Max(b.WorldBoundsBox.Min.Z, b.WorldBoundsBox.Max.Z);
          

            if (WorldPosition.X >= (minX - WorldBounds.Radius) && WorldPosition.X <= (maxX + WorldBounds.Radius))
            {
                if (WorldPosition.Z >= (minZ - WorldBounds.Radius) && WorldPosition.Z <= (maxZ + WorldBounds.Radius))
                {
                    return true;
                }
            }

            return false;
        }

        public Plane findIntersectingPlane(Building b)
        {

            Vector3 firstClosest = new Vector3();
            float firstClosestDistance = float.MaxValue;
            Vector3 secondClosest = new Vector3();
            float secondClosestDistance = float.MaxValue;
            Vector3 thirdClosest = new Vector3();
            float thirdClosestDistance = float.MaxValue;

            Vector3[] corners = b.WorldBoundsBox.GetCorners();
            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 temp = corners[i];
                if (temp.Y > 100)
                    continue;

                float tempd = distanceSquared(new Vector2(temp.X, temp.Z), new Vector2(WorldPosition.X, WorldPosition.Z));

                if (tempd <= firstClosestDistance)
                {
                    thirdClosestDistance = secondClosestDistance;
                    thirdClosest = secondClosest;
                    secondClosestDistance = firstClosestDistance;
                    secondClosest = firstClosest;
                    firstClosestDistance = tempd;
                    firstClosest = temp;
                }
                else if (tempd <= secondClosestDistance)
                {
                    thirdClosestDistance = secondClosestDistance;
                    thirdClosest = secondClosest;
                    secondClosestDistance = tempd;
                    secondClosest = temp;
                }
                else if (tempd <= thirdClosestDistance)
                {
                    thirdClosestDistance = tempd;
                    thirdClosest = temp;
                }
            }

            //firstClosest.W = (float)Math.Sqrt(firstClosest.W);
            //secondClosest.W = (float)Math.Sqrt(secondClosest.W);
            Vector3 arbitrary = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 arbitrary2 = new Vector3(0.0f, 0.0f, 0.0f);
            Plane wall1;
            Plane wall2;

            Random rand = new Random();

            if (firstClosest.X == secondClosest.X)
            {
                arbitrary.X = firstClosest.X;
                arbitrary.Z = (float)rand.Next((int)Math.Min(firstClosest.Z, secondClosest.Z), (int)Math.Max(firstClosest.Z, secondClosest.Z));
                wall1 = new Plane(firstClosest, arbitrary, secondClosest);
            }
            else if (firstClosest.X == thirdClosest.X)
            {
                arbitrary.X = firstClosest.X;
                arbitrary.Z = (float)rand.Next((int)Math.Min(firstClosest.Z, thirdClosest.Z), (int)Math.Max(firstClosest.Z, thirdClosest.Z));
                wall1 = new Plane(firstClosest, arbitrary, thirdClosest);
            }
            else
            {
                arbitrary.X = secondClosest.X;
                arbitrary.Z = (float)rand.Next((int)Math.Min(secondClosest.Z, thirdClosest.Z), (int)Math.Max(secondClosest.Z, thirdClosest.Z));
                wall1 = new Plane(secondClosest, arbitrary, thirdClosest);
            }
            
            if (firstClosest.Z == secondClosest.Z)
            {
                arbitrary2.Z = firstClosest.Z;
                arbitrary2.X = (float)rand.Next((int)Math.Min(firstClosest.X, secondClosest.X), (int)Math.Min(firstClosest.X, secondClosest.X));
                wall2 = new Plane(firstClosest, arbitrary2, secondClosest);
            }
            else if (firstClosest.Z == thirdClosest.Z)
            {
                arbitrary2.Z = firstClosest.Z;
                arbitrary2.X = (float)rand.Next((int)Math.Min(firstClosest.X, thirdClosest.X), (int)Math.Min(firstClosest.X, thirdClosest.X));
                wall2 = new Plane(firstClosest, arbitrary2, thirdClosest);
            }
            
            else
            {
                arbitrary2.Z = thirdClosest.Z;
                arbitrary2.X = (float)rand.Next((int)Math.Min(secondClosest.X, thirdClosest.X), (int)Math.Min(secondClosest.X, thirdClosest.X));
                wall2 = new Plane(secondClosest, arbitrary2, thirdClosest);
            }

            
            float distance1 = Math.Abs(Vector3.Dot(Vector3.Normalize(wall1.Normal), arbitrary - WorldPosition));
            float distance2 = Math.Abs(Vector3.Dot(Vector3.Normalize(wall2.Normal), arbitrary2 - WorldPosition));

            if (distance1 < distance2)
            {
                return wall1;
            }
            else
            {
                return wall2;
            }

        }
        

        /// <summary>
        /// Computes the distance squared between two points.
        /// </summary>
        /// <param name="p1">Vector2 point 1.</param>
        /// <param name="p2">Vector2 point 2.</param>
        /// <returns></returns>
        public float distanceSquared(Vector2 p1, Vector2 p2)
        {
            return (p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y);
        }

        public void playerDeath()
        {
            dead = true;
            GameplayScreen.Instance.removeActor(this);
        }


        public void respawnPlayer()
        {
            GameplayScreen.Instance.m_kTimer.RemoveTimer("Respawn");
            CurrentHealth = 100.0f;
            dead = false;
            GameplayScreen.Instance.addActor(this);

        }
    }
}