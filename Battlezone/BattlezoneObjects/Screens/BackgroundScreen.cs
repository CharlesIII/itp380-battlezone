#region File Description
//-----------------------------------------------------------------------------
// BackgroundScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Battlezone.Engine;
using Microsoft.Xna.Framework.Audio;
using Battlezone.BattlezoneObjects;
#endregion

namespace Battlezone
{
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary>
    class BackgroundScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        Texture2D backgroundTexture;

        private BattlezoneObjects.PlayerTank tank;

        public static Matrix CameraMatrix = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 2000.0f), Vector3.Zero, Vector3.UnitY);
        //public static Matrix ProjectionMatrix = Matrix.CreateOrthographic(1024,768,0.00001f,10000.0f);     //TODO: This needs to be Perspective
        public static Matrix ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(75.0f * (float)Math.PI / 180.0f, 4.0f / 3.0f, 0.1f, 10000.0f);

        // The explosions effect works by firing projectiles up into the
        // air, so we need to keep track of all the active projectiles.
        List<Projectile> projectiles = new List<Projectile>();

        private ParticleSystem explosionParticles;
        private ParticleSystem explosionSmokeParticles;
        private ParticleSystem projectileTrailParticles;
        private ParticleSystem smokePlumeParticles;
        private ParticleSystem fireParticles;

        TimeSpan timeToNextProjectile = TimeSpan.Zero;

        SoundEffect Background;
        SoundEffectInstance instance;

        public static AudioEngine audioEngine;
        public static WaveBank waveBank;
        public static SoundBank soundBank;

        private bool SmokePlume = true;
        private bool fired = true;

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
        private Vector3 lookAtOffset = new Vector3(0.0f, 50.0f, -10.0f);

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
        public BackgroundScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        /// <summary>
        /// Loads graphics content for this screen. The background texture is quite
        /// big, so we use our own local ContentManager to load it. This allows us
        /// to unload before going from the menus into the game itself, wheras if we
        /// used the shared ContentManager provided by the Game class, the content
        /// would remain loaded forever.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            //backgroundTexture = content.Load<Texture2D>("background");
            tank = new PlayerTank(ScreenManager.Game);
            tank.gamePlay = false;

            ScreenManager.Game.Components.Add(tank);
            /* Debug usage for AI Tank testing*/
            tank.Initialize();

            smokePlumeParticles = new SmokePlumeParticleSystem(ScreenManager.Game, content);
            ScreenManager.Game.Components.Add(smokePlumeParticles);

            explosionParticles = new ExplosionParticleSystem(ScreenManager.Game, content);
            explosionSmokeParticles = new ExplosionSmokeParticleSystem(ScreenManager.Game, content);
            projectileTrailParticles = new ProjectileTrailParticleSystem(ScreenManager.Game, content);

            // Set the draw order so the explosions and fire
            // will appear over the top of the smoke.
            //smokePlumeParticles.DrawOrder = 100;
            explosionSmokeParticles.DrawOrder = 200;
            projectileTrailParticles.DrawOrder = 300;
            explosionParticles.DrawOrder = 400;
            //fireParticles.DrawOrder = 500;

            // Register the particle system components.
            ScreenManager.Game.Components.Add(explosionParticles);
            ScreenManager.Game.Components.Add(explosionSmokeParticles);
            ScreenManager.Game.Components.Add(projectileTrailParticles);
            //ScreenManager.Game.Components.Add(smokePlumeParticles);
            //ScreenManager.Game.Components.Add(fireParticles);

            //Background = content.Load<SoundEffect>("TheEcstacyOfGold"); //Background in .wav format

            float volume = 1.0f;
            //Background.Play(volume, 0.0f, 0.0f); // volume is a float between 0.0f and 1.0f
            //instance = Background.CreateInstance();
            //instance.IsLooped = true;
            //instance.Play();

            audioEngine = new AudioEngine("Content/BackgroundSound.xgs");
            waveBank = new WaveBank(audioEngine, "Content/BackgroundSound.xwb");
            soundBank = new SoundBank(audioEngine, "Content/BackgroundSound.xsb");

            audioEngine.Update();
            soundBank.PlayCue("TheEcstacyOfGold");

            ScreenManager.Game.ResetElapsedTime();

        }


        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
            ScreenManager.Game.Components.Remove(tank);
            ScreenManager.Game.Components.Remove(explosionParticles);
            ScreenManager.Game.Components.Remove(explosionSmokeParticles);
            ScreenManager.Game.Components.Remove(projectileTrailParticles);
            ScreenManager.Game.Components.Remove(smokePlumeParticles);
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the background screen. Unlike most screens, this should not
        /// transition off even if it has been covered by another screen: it is
        /// supposed to be covered, after all! This overload forces the
        /// coveredByOtherScreen parameter to false in order to stop the base
        /// Update method wanting to transition off.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            if (gameTime.ElapsedGameTime.Ticks != 0)
            {

                float deltaTime = ((float)gameTime.ElapsedGameTime.Ticks / System.TimeSpan.TicksPerMillisecond) / 1000.0f;
                explosionParticles.SetCamera(CameraMatrix, ProjectionMatrix);
                explosionSmokeParticles.SetCamera(CameraMatrix, ProjectionMatrix);
                projectileTrailParticles.SetCamera(CameraMatrix, ProjectionMatrix);
                smokePlumeParticles.SetCamera(CameraMatrix, ProjectionMatrix);

                UpdateExplosions(gameTime);
                UpdateProjectiles(gameTime);
                UpdateSmokePlume();

                //tank.Scale = 1.5f;
                //tank.WorldPosition = new Vector3(0, -50000, 0);
                tank.LWheelRotation += (2 * deltaTime);
                tank.RWheelRotation += (2 * deltaTime);

                //tank.TurretRotation += (0.50f * deltaTime);

                UpdateWorldPositions();
                ChasePosition = tank.WorldPosition;
                Matrix temp = tank.worldTransform * tank.turretBone.Transform;
                ChaseDirection = (temp.Forward * 1f);
                Up = Vector3.UnitY;
                CameraMatrix = Matrix.CreateLookAt(desiredPosition, LookAt, Up);

            }
             
        }


        /// <summary>
        /// Draws the background screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                   Color.Black, 0, 0);
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);
            byte fade = TransitionAlpha;
            ScreenManager.GraphicsDevice.RenderState.DepthBufferEnable = true;
            ScreenManager.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            ScreenManager.GraphicsDevice.RenderState.AlphaTestEnable = false;

        }
        
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

                Matrix temp = tank.worldTransform * tank.turretBone.Transform;
                ChaseDirection = (temp.Forward * 1f);

                Vector3 temp1;

                Random random = new Random();

                int num = random.Next(0,50);
                if (fired)
                {
                    temp1 = new Vector3(random.Next(50, 100), 0, 200);
                    temp1 = LookAt + temp1;
                    fired = false;
                }
                else
                {
                    temp1 = new Vector3(random.Next(50, 100), 0, -200);
                    temp1 = LookAt - temp1;
                    fired = true;
                }

                projectiles.Add(new Projectile(explosionParticles,
                                               explosionSmokeParticles,
                                               projectileTrailParticles,
                                               temp1,
                                               ChaseDirection,1));

                timeToNextProjectile += TimeSpan.FromSeconds(random.Next(4, 5));
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
        void UpdateSmokePlume()
        {
            // This is trivial: we just create one new smoke particle per frame.
            if (SmokePlume)
            {
                Vector3 temp = new Vector3(27, 40, -45);
                temp = tank.WorldPosition + temp;
                smokePlumeParticles.AddParticle(temp, Vector3.Zero);
                SmokePlume = false;
            }
            else
            {
                Vector3 temp = new Vector3(-27, 40, -45);
                temp = tank.WorldPosition + temp;
                smokePlumeParticles.AddParticle(temp, Vector3.Zero);
                SmokePlume = true;
            }
        }

        

        #endregion
    }
}
