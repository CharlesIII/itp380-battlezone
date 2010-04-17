#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Battlezone.Engine;
using Battlezone.BattlezoneObjects;
#endregion

namespace Battlezone
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields
        
        private static GameplayScreen instance;    //singleton design pattern
        public static GameplayScreen Instance
        {
            get
            {
                if (instance == null)
                    instance = new GameplayScreen();
                return instance;
            }
        }

        public static Matrix CameraMatrix = Matrix.CreateLookAt(new Vector3(0.0f,0.0f,2000.0f),Vector3.Zero,Vector3.UnitY);
        //public static Matrix ProjectionMatrix = Matrix.CreateOrthographic(1024,768,0.00001f,10000.0f);     //TODO: This needs to be Perspective
        public static Matrix ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(75.0f * (float)Math.PI / 180.0f, 4.0f / 3.0f, 0.1f, 10000.0f);

        public static Vector3 DiffuseColor = Color.Black.ToVector3();
        public static Vector3 DirLightDirection = new Vector3(1,-1,0);

        public PathFinder navPathFind;

        public static AudioEngine audioEngine;
        public static WaveBank waveBank;
        public static SoundBank soundBank;

        private float m_fTotalTime;

        private List<Actor> activeActors;   //list of active actors for collision checking
        private List<Actor> actorsToAdd;    //list of new actors to add to list of activeActors
        private List<Actor> actorsToRemove; //list of existing actors to be removed from list of activeActors;

        private BattlezoneObjects.PlayerTank m_kPlayer;

        ContentManager content;
        SpriteFont gameFont;

        Random random = new Random();

		Utils.Timer m_kTimer = new Utils.Timer();

        SpawnManager m_kSpawnManager;

        TimeSpan timeToNextProjectile = TimeSpan.Zero;

        // The explosions effect works by firing projectiles up into the
        // air, so we need to keep track of all the active projectiles.
        List<Projectile> projectiles = new List<Projectile>();

        private ParticleSystem explosionParticles;
        private ParticleSystem explosionSmokeParticles;
        private ParticleSystem projectileTrailParticles;
        private ParticleSystem smokePlumeParticles;
        private ParticleSystem fireParticles;


        #endregion

        #region Chased object properties (set externally each frame)

        /// <summary>
        /// Position of object being chased.
        /// </summary>
        public Vector3 ChasePosition
        {
            get { return chasePosition; }
            set { chasePosition = value; }
        }
        private Vector3 chasePosition;

        /// <summary>
        /// Direction the chased object is facing.
        /// </summary>
        public Vector3 ChaseDirection
        {
            get { return chaseDirection; }
            set { chaseDirection = value; }
        }
        private Vector3 chaseDirection;

        /// <summary>
        /// Chased object's Up vector.
        /// </summary>
        public Vector3 Up
        {
            get { return up; }
            set { up = value; }
        }
        private Vector3 up = Vector3.UnitY;

        #endregion

        #region Desired camera positioning (set when creating camera or changing view)

        /// <summary>
        /// Position of camera in world space.
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
        }
        private Vector3 position;

        /// <summary>
        /// Desired camera position in the chased object's coordinate system.
        /// </summary>
        public Vector3 DesiredPositionOffset
        {
            get { return desiredPositionOffset; }
            set { desiredPositionOffset = value; }
        }
        private Vector3 desiredPositionOffset = new Vector3(0.0f, 130.0f, 1200.0f);

        /// <summary>
        /// Desired camera position in world space.
        /// </summary>
        public Vector3 DesiredPosition
        {
            get
            {
                // Ensure correct value even if update has not been called this frame
                UpdateWorldPositions();
                return desiredPosition;
            }
        }
        private Vector3 desiredPosition;

        /// <summary>
        /// Look at point in the chased object's coordinate system.
        /// </summary>
        public Vector3 LookAtOffset
        {
            get { return lookAtOffset; }
            set { lookAtOffset = value; }
        }
        private Vector3 lookAtOffset = new Vector3(0.0f, 10.0f, -10.0f);

        /// <summary>
        /// Look at point in world space.
        /// </summary>
        public Vector3 LookAt
        {
            get
            {
                // Ensure correct value even if update has not been called this frame
                UpdateWorldPositions();

                return lookAt;
            }
        }
        private Vector3 lookAt;

        #endregion


        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            
            if (instance == null)
            {
                activeActors = new List<Actor>();
                actorsToAdd = new List<Actor>();
                actorsToRemove = new List<Actor>();
                TransitionOnTime = TimeSpan.FromSeconds(1.5);
                TransitionOffTime = TimeSpan.FromSeconds(0.5);
                try
                {
                    navPathFind = new PathFinder(@"..\..\..\BattlezoneObjects\Navigation Nodes.txt");
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine(e.ToString());
                    throw new Exception("Unable to load path finding system.",e);
                }
                instance = this;
            }
            else
                throw new Exception("There should only be one GameplayeScreen object in existence.");


        }

        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            gameFont = content.Load<SpriteFont>("gamefont");

            //load audio
            /*
            audioEngine = new AudioEngine("Content/Sounds.xgs");
            waveBank = new WaveBank(audioEngine, "Content/XNAsteroids Waves.xwb");
            soundBank = new SoundBank(audioEngine, "Content/XNAsteroids Cues.xsb");
             */

            //load spawn manager
            m_kSpawnManager = new SpawnManager(ScreenManager.Game);
            ScreenManager.Game.Components.Add(m_kSpawnManager);

            //Load Player Tank
            m_kPlayer = new PlayerTank(ScreenManager.Game);
            //ScreenManager.Game.Components.Add(m_kPlayer);
            /* Debug usage for AI Tank testing*/
            m_kPlayer.Initialize();

            Level l = new Level(ScreenManager.Game);
            ScreenManager.Game.Components.Add(l);

            AITank test = new AITank(ScreenManager.Game, navPathFind, new Vector3());
            ScreenManager.Game.Components.Add(test);

            /*
            // Construct our particle system components.
            explosionParticles = new ExplosionParticleSystem(ScreenManager.Game, content);
            explosionSmokeParticles = new ExplosionSmokeParticleSystem(ScreenManager.Game, content);
            projectileTrailParticles = new ProjectileTrailParticleSystem(ScreenManager.Game, content);
            smokePlumeParticles = new SmokePlumeParticleSystem(ScreenManager.Game, content);
            fireParticles = new FireParticleSystem(ScreenManager.Game, content);

            // Set the draw order so the explosions and fire
            // will appear over the top of the smoke.
            smokePlumeParticles.DrawOrder = 100;
            explosionSmokeParticles.DrawOrder = 200;
            projectileTrailParticles.DrawOrder = 300;
            explosionParticles.DrawOrder = 400;
            fireParticles.DrawOrder = 500;

            // Register the particle system components.
            ScreenManager.Game.Components.Add(explosionParticles);
            ScreenManager.Game.Components.Add(explosionSmokeParticles);
            ScreenManager.Game.Components.Add(projectileTrailParticles);
            ScreenManager.Game.Components.Add(smokePlumeParticles);
            ScreenManager.Game.Components.Add(fireParticles);
             * */

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {
                if (gameTime.ElapsedGameTime.Ticks != 0)
                {
                    float fDelta = (gameTime.ElapsedGameTime.Ticks / System.TimeSpan.TicksPerMillisecond) / 1000.0f;
                    m_fTotalTime = (m_fTotalTime + fDelta) % (float)(Math.PI * 2);
                    DirLightDirection.X = (float)Math.Cos(m_fTotalTime);
                    DirLightDirection.Y = (float)Math.Sin(m_fTotalTime);
                    
                    if (m_fTotalTime >= 5 && m_fTotalTime <= 5 + fDelta)
                    {
                        switch (random.Next(3))
                        {
                            case 0:
                                DiffuseColor.X = random.Next(0, 100);;
                                break;
                            case 1:
                                DiffuseColor.Y = random.Next(0, 100);
                                break;
                            case 2:
                                DiffuseColor.Z = random.Next(0, 100);
                                break;
                        }
                    }

                    //UpdateExplosions(gameTime);
                    //UpdateProjectiles(gameTime);

                    //perform activeActors maintenance
                    updateActors();

                    //check for collisions
                    checkCollision();

                    //Update Camera
                    UpdateWorldPositions();
                    ChasePosition = m_kPlayer.WorldPosition;
                    Matrix temp = m_kPlayer.worldTransform * m_kPlayer.turretBone.Transform;
                    ChaseDirection = (temp.Forward * -1);
                    Up = Vector3.UnitY;
                    CameraMatrix = Matrix.CreateLookAt(desiredPosition, LookAt, Up);
                }
            }
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
		public override void HandleInput(InputState input, GameTime gameTime)
        {

            float deltaTime = ((float)gameTime.ElapsedGameTime.Ticks / System.TimeSpan.TicksPerMillisecond) / 1000.0f;

            if (input == null)
                throw new ArgumentNullException("input");

            if (input.PauseGame)
            {
                // If they pressed pause, bring up the pause menu screen.
                ScreenManager.AddScreen(new PauseMenuScreen());
            }
            else
            {
                if (m_kPlayer != null)
                {
                    if (input.TurretLeft)
                    {
                        m_kPlayer.TurretRotation += (2 * deltaTime);
                        //projectiles.Add(new Projectile(explosionParticles, explosionSmokeParticles, projectileTrailParticles));
                        //CameraMatrix = Matrix.CreateLookAt(desiredPosition, LookAt, Up);
                    }
                    if (input.TurretRight)
                    {
                        m_kPlayer.TurretRotation -= (2 * deltaTime);
                        //projectiles.Add(new Projectile(explosionParticles, explosionSmokeParticles, projectileTrailParticles));
                        //CameraMatrix = Matrix.CreateLookAt(desiredPosition, LookAt, Up);
                    }

                    if (input.TurnLeft)
                    {
                        //m_kPlayer.TurretRotation += ((float)Math.PI / 5) * deltaTime;
                        //m_kPlayer.SteerRotation = (float)Math.PI / 5;
                        m_kPlayer.LWheelRotation -= (2 * deltaTime);
                        m_kPlayer.RWheelRotation += (2 * deltaTime);
                        m_kPlayer.Quat *= Quaternion.CreateFromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), ((float)Math.PI / 5) * deltaTime);
                    }

                    if (input.TurnRight)
                    {
                        //m_kPlayer.TurretRotation -= ((float)Math.PI / 5) * deltaTime;
                        //m_kPlayer.SteerRotation = -(float)Math.PI / 5;
                        m_kPlayer.LWheelRotation += (2 * deltaTime);
                        m_kPlayer.RWheelRotation -= (2 * deltaTime);
                        m_kPlayer.Quat *= Quaternion.CreateFromAxisAngle(new Vector3(0.0f, -1.0f, 0.0f), ((float)Math.PI / 5) * deltaTime);
                    }

                    if (input.Move)
                    {
                        m_kPlayer.LWheelRotation += (2 * deltaTime);
                        m_kPlayer.RWheelRotation += (2 * deltaTime);
                    }

                    if (input.Reverse)
                    {
                        m_kPlayer.LWheelRotation -= (2 * deltaTime);
                        m_kPlayer.RWheelRotation -= (2 * deltaTime);
                    }
                    if (!input.TurnLeft && !input.TurnRight)
                    {
                        m_kPlayer.SteerRotation = 0.0f;
                    }
                }
            }
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.Black, 0, 0);

            /*
            explosionParticles.SetCamera(CameraMatrix, ProjectionMatrix);
            explosionSmokeParticles.SetCamera(CameraMatrix, ProjectionMatrix);
            projectileTrailParticles.SetCamera(CameraMatrix, ProjectionMatrix);
            smokePlumeParticles.SetCamera(CameraMatrix, ProjectionMatrix);
            fireParticles.SetCamera(CameraMatrix, ProjectionMatrix);
            */
            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);
        }

        /// <summary>
        /// Rebuilds object space values in world space. Invoke before publicly
        /// returning or privately accessing world space values.
        /// </summary>
        private void UpdateWorldPositions()
        {
            // Construct a matrix to transform from object space to worldspace
            Matrix transform = Matrix.Identity;
            transform.Forward = ChaseDirection;
            transform.Up = Up;
            transform.Right = Vector3.Cross(Up, ChaseDirection);

            // Calculate desired camera properties in world space
            desiredPosition = ChasePosition +
                Vector3.TransformNormal(DesiredPositionOffset, transform);
            lookAt = ChasePosition +
                Vector3.TransformNormal(LookAtOffset, transform);
        }



        void UpdateExplosions(GameTime gameTime)
        {
            timeToNextProjectile -= gameTime.ElapsedGameTime;

            if (timeToNextProjectile <= TimeSpan.Zero)
            {
                // Create a new projectile once per second. The real work of moving
                // and creating particles is handled inside the Projectile class.
                projectiles.Add(new Projectile(explosionParticles,
                                               explosionSmokeParticles,
                                               projectileTrailParticles));

                timeToNextProjectile += TimeSpan.FromSeconds(3);
            }
        }


        /// <summary>
        /// Helper for updating the list of active projectiles.
        /// </summary>
        void UpdateProjectiles(GameTime gameTime)
        {
            int i = 0;

            while (i < projectiles.Count)
            {
                if (!projectiles[i].Update(gameTime))
                {
                    // Remove projectiles at the end of their life.
                    projectiles.RemoveAt(i);
                }
                else
                {
                    // Advance to the next projectile.
                    i++;
                }
            }
        }

        #endregion

        #region Collision Detection Logistics

        /// <summary>
        /// Adds a new actor to the list of actorsToAdd.
        /// </summary>
        /// <param name="a"></param>
        public void addActor(Actor a)
        {
            actorsToAdd.Add(a);
        }

        /// <summary>
        /// Adds an actor to the list of actorsToRemove.
        /// </summary>
        /// <param name="a"></param>
        public void removeActor(Actor a)
        {
            actorsToRemove.Remove(a);
        }

        /// <summary>
        /// Performs maintenance on activeActors.
        /// </summary>
        private void updateActors()
        {
            //remove all the actors that need to be removed
            foreach (Actor a in actorsToRemove)
            {
                activeActors.Remove(a);
            }
            actorsToRemove.Clear();

            //add in all the new actors
            foreach (Actor a in actorsToAdd)
            {
                activeActors.Add(a);
            }
            actorsToAdd.Clear();
        }



        /// <summary>
        /// Checks collisions between objects in game.
        /// </summary>
        private void checkCollision()
        {
            //terrible brute force method
            foreach (Actor a in activeActors)
            {
                foreach (Actor b in activeActors)
                {
                    //don't try to collide an object with itself and make sure they can collide
                    if (a != b 
                        && a.COLLISION_IDENTIFIER != CollisionIdentifier.NONCOLLIDING 
                        && b.COLLISION_IDENTIFIER != CollisionIdentifier.NONCOLLIDING)
                    {
                        if (a.WorldBounds.Intersects(b.WorldBounds))
                        {
                            a.collide(b);
                            b.collide(a);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
