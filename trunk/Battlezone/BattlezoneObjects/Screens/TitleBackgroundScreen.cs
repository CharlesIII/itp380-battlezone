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
    class TitleBackgroundScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        Texture2D backgroundTexture;


        public static Matrix CameraMatrix = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, -100), Vector3.Zero, Vector3.UnitY);
        public static Matrix ProjectionMatrix = Matrix.CreateOrthographic(1024,768,-100.0f,1000);     //TODO: This needs to be Perspective
        //public static Matrix ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(75.0f * (float)Math.PI / 180.0f, 4.0f / 3.0f, 0.1f, 10000.0f);

        // The explosions effect works by firing projectiles up into the
        // air, so we need to keep track of all the active projectiles.
        List<Projectile> projectiles = new List<Projectile>();

        private ParticleSystem explosionParticles;
        private ParticleSystem explosionSmokeParticles;
        private ParticleSystem projectileTrailParticles;
        private ParticleSystem fireParticles;

        TimeSpan timeToNextProjectile = TimeSpan.Zero;

        SoundEffect Background;
        SoundEffectInstance instance;

        private bool SmokePlume = true;
        private bool fired = true;
        private bool letter = true;
        Vector2 letterPosition;
        double angle;
        int letterCount;
        Vector2 temp;
        Vector2 startPosition;
        Vector2 firePosition;
        String text;
        String name;
        float scale;
        Cue c;

        Utils.Timer m_kTimer = new Utils.Timer();

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
        public TitleBackgroundScreen()
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
        
            explosionParticles = new ExplosionParticleSystemTitleBackground(ScreenManager.Game, content);
            explosionSmokeParticles = new ExplosionSmokeParticleSystemTitleBackground(ScreenManager.Game, content);
            projectileTrailParticles = new ProjectileTrailParticleSystemTitleBackground(ScreenManager.Game, content);
            fireParticles = new FireParticleSystemTitleBackground(ScreenManager.Game, content);


            // Set the draw order so the explosions and fire
            // will appear over the top of the smoke.
            explosionSmokeParticles.DrawOrder = 200;
            projectileTrailParticles.DrawOrder = 300;
            explosionParticles.DrawOrder = 400;
            fireParticles.DrawOrder = 500;

            // Register the particle system components.
            ScreenManager.Game.Components.Add(explosionParticles);
            ScreenManager.Game.Components.Add(explosionSmokeParticles);
            ScreenManager.Game.Components.Add(projectileTrailParticles);
            //ScreenManager.Game.Components.Add(smokePlumeParticles);
            ScreenManager.Game.Components.Add(fireParticles);

            letterPosition = new Vector2(0, 0);
            angle = 90;
            letterCount = 0;

            temp = new Vector2(0, 0);
            startPosition = new Vector2(0, 0);
            firePosition = new Vector2(0, 0);
            text = "";
            name = "";
            scale = 5;

            ScreenManager.Game.ResetElapsedTime();

        }


        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
            ScreenManager.Game.Components.Remove(explosionParticles);
            ScreenManager.Game.Components.Remove(explosionSmokeParticles);
            ScreenManager.Game.Components.Remove(projectileTrailParticles);
            ScreenManager.Game.Components.Remove(fireParticles);
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
                fireParticles.SetCamera(CameraMatrix, ProjectionMatrix);

                UpdateExplosions(gameTime);
                UpdateProjectiles(gameTime);
                if (!letter)
                {
                    UpdateFireExaust();
                }

                m_kTimer.Update(gameTime);


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

            // Draw the selected entry in yellow, otherwise white.
            Color color = Color.Blue;

            float deltaTime = ((float)gameTime.ElapsedGameTime.Ticks / System.TimeSpan.TicksPerMillisecond) / 1000.0f;
            // Pulsate the size of the selected menu entry.
            double time = gameTime.TotalGameTime.TotalSeconds;

            float pulsate = (float)Math.Sin(time * 6) + 1;

            float scale = 1; //+pulsate * 0.05f;
            ScreenManager screenManager = this.ScreenManager;
            SpriteBatch sprite = screenManager.SpriteBatch;
            // Modify the alpha to fade text out during transitions.
            color = new Color(color.R, color.G, color.B);
            Vector2 origin = Vector2.Zero;

            SpriteFont font = ScreenManager.gameTitleFont;

            // Draw text, centered on the middle of each line.
            //SpriteFont font = screenManager.gameTitleFont;
            if (letterCount == 0 && !TitleMenuScreen.skip)
            {
                text = "B";
                name = "B";

                float X = -70-100*(float)Math.Cos((180-angle)*(Math.PI/180));
                float Y = 100-100*(float)Math.Sin((180-angle)*(Math.PI/180));
                letterPosition = new Vector2(X, Y);
                temp = letterPosition;
                angle = angle - deltaTime * 200;
                sprite.Begin();
                sprite.DrawString(font, text, letterPosition, color, 0,
                                       origin, scale, SpriteEffects.None, 0);
                sprite.End();
                if (angle <= 0)
                {
                    letterCount++;
                    startPosition.X = -100;
                    Projectile pro = new Projectile(explosionParticles, explosionSmokeParticles, projectileTrailParticles, new Vector3(-temp.X + 460, temp.Y + 100, 0), ChaseDirection, 0, ScreenManager.Game);
                    pro.Initialize(100, 90, 50, 250, 250, 0);
                    projectiles.Add(pro);
                    ScreenManager.soundSoundBank.PlayCue("MenuExplosion");

                }
              
            }
            else if (letterCount == 1 && !TitleMenuScreen.skip)
            {
                text = "A";
                name = "B";
                //letterPosition = new Vector2(X, Y);
                startPosition.Y = temp.Y;
                startPosition.X = startPosition.X + deltaTime * 500;

                sprite.Begin();
                sprite.DrawString(font, text, startPosition, color, 0,
                                       origin, scale, SpriteEffects.None, 0);
                sprite.End();
                if (startPosition.X >= 120)
                {
                    Projectile pro = new Projectile(explosionParticles, explosionSmokeParticles, projectileTrailParticles, new Vector3(-startPosition.X + 442, startPosition.Y + 100, 0), ChaseDirection, 0, ScreenManager.Game);
                    pro.Initialize(100, 90, 50, 250, 250, 0);
                    projectiles.Add(pro);
                    ScreenManager.soundSoundBank.PlayCue("MenuExplosion");
                    letterCount++;
                    startPosition.X = -100;
                    //letter = false;
                }

            }
            else if (letterCount == 2 && !TitleMenuScreen.skip)
            {
                text = "T";
                name = "BA";
                //letterPosition = new Vector2(X, Y);
                startPosition.X = startPosition.X + deltaTime * 550;

                sprite.Begin();
                sprite.DrawString(font, text, startPosition, color, 0,
                                       origin, scale, SpriteEffects.None, 0);
                sprite.End();
                if (startPosition.X >= 230)
                {
                    Projectile pro = new Projectile(explosionParticles, explosionSmokeParticles, projectileTrailParticles, new Vector3(-startPosition.X + 460, startPosition.Y + 100, 0), ChaseDirection, 0, ScreenManager.Game);
                    pro.Initialize(100, 90, 50, 250, 250, 0);
                    projectiles.Add(pro);
                    ScreenManager.soundSoundBank.PlayCue("MenuExplosion");
                    letterCount++;
                    startPosition.X = -100;
                    //letter = false;
                }

            }
            else if (letterCount == 3 && !TitleMenuScreen.skip)
            {
                text = "T";
                name = "BAT";
                //letterPosition = new Vector2(X, Y);
                startPosition.X = startPosition.X + deltaTime * 600;
                sprite.Begin();
                sprite.DrawString(font, text, startPosition, color, 0,
                                       origin, scale, SpriteEffects.None, 0);
                sprite.End();
                if (startPosition.X >= 320)
                {
                    Projectile pro = new Projectile(explosionParticles, explosionSmokeParticles, projectileTrailParticles, new Vector3(-startPosition.X + 460, startPosition.Y + 100, 0), ChaseDirection, 0, ScreenManager.Game);
                    pro.Initialize(100, 90, 50, 250, 250, 0);
                    projectiles.Add(pro);
                    ScreenManager.soundSoundBank.PlayCue("MenuExplosion");
                    letterCount++;
                    startPosition.X = -100;
                    //letter = false;
                }

            }
            else if (letterCount == 4 && !TitleMenuScreen.skip)
            {
                text = "L";
                name = "BATT";
                //letterPosition = new Vector2(X, Y);
                startPosition.X = startPosition.X + deltaTime * 750;

                sprite.Begin();
                sprite.DrawString(font, text, startPosition, color, 0,
                                       origin, scale, SpriteEffects.None, 0);
                sprite.End();
                if (startPosition.X >= 420)
                {
                    Projectile pro = new Projectile(explosionParticles, explosionSmokeParticles, projectileTrailParticles, new Vector3(-startPosition.X + 470, startPosition.Y + 100, 0), ChaseDirection, 0, ScreenManager.Game);
                    pro.Initialize(100, 90, 50, 250, 250, 0);
                    projectiles.Add(pro);
                    ScreenManager.soundSoundBank.PlayCue("MenuExplosion");
                    letterCount++;
                    startPosition.X = -100;
                    //letter = false;
                }

            }
            else if (letterCount == 5 && !TitleMenuScreen.skip)
            {
                text = "E";
                name = "BATTL";
                //letterPosition = new Vector2(X, Y);
                startPosition.X = startPosition.X + deltaTime * 800;
                sprite.Begin();
                sprite.DrawString(font, text, startPosition, color, 0,
                                       origin, scale, SpriteEffects.None, 0);

                sprite.End();
                if (startPosition.X >= 500)
                {
                    Projectile pro = new Projectile(explosionParticles, explosionSmokeParticles, projectileTrailParticles, new Vector3(-startPosition.X + 460, startPosition.Y + 100, 0), ChaseDirection, 0, ScreenManager.Game);
                    pro.Initialize(100, 90, 50, 250, 250, 0);
                    projectiles.Add(pro);
                    ScreenManager.soundSoundBank.PlayCue("MenuExplosion");
                    letterCount++;
                    startPosition.X = -100;
                    //letter = false;
                }

            }
            else if (letterCount == 6 && !TitleMenuScreen.skip)
            {
                text = "Z";
                name = "BATTLE";
                //letterPosition = new Vector2(X, Y);
                startPosition.X = startPosition.X + deltaTime * 900;

                sprite.Begin();
                sprite.DrawString(font, text, startPosition, color, 0,
                                       origin, scale, SpriteEffects.None, 0);
                sprite.End();
                if (startPosition.X >= 580)
                {
                    Projectile pro = new Projectile(explosionParticles, explosionSmokeParticles, projectileTrailParticles, new Vector3(-startPosition.X + 442, startPosition.Y + 100, 0), ChaseDirection, 0, ScreenManager.Game);
                    pro.Initialize(100, 90, 50, 250, 250, 0);
                    projectiles.Add(pro);
                    ScreenManager.soundSoundBank.PlayCue("MenuExplosion");
                    letterCount++;
                    startPosition.X = -100;
                    //letter = false;
                }

            }
            else if (letterCount == 7 && !TitleMenuScreen.skip)
            {
                text = "O";
                name = "BATTLEZ";
                //letterPosition = new Vector2(X, Y);
                startPosition.X = startPosition.X + deltaTime * 1000;

                sprite.Begin();
                sprite.DrawString(font, text, startPosition, color, 0,
                                       origin, scale, SpriteEffects.None, 0);
                sprite.End();
                if (startPosition.X >= 680)
                {
                    Projectile pro = new Projectile(explosionParticles, explosionSmokeParticles, projectileTrailParticles, new Vector3(-startPosition.X + 460, startPosition.Y + 100, 0), ChaseDirection, 0, ScreenManager.Game);
                    pro.Initialize(100, 90, 50, 250, 250, 0);
                    projectiles.Add(pro);
                    ScreenManager.soundSoundBank.PlayCue("MenuExplosion");
                    letterCount++;
                    startPosition.X = -100;
                    //letter = false;
                }

            }
            else if (letterCount == 8 && !TitleMenuScreen.skip)
            {
                text = "N";
                name = "BATTLEZO";
                //letterPosition = new Vector2(X, Y);
                startPosition.X = startPosition.X + deltaTime * 1100;

                sprite.Begin();
                sprite.DrawString(font, text, startPosition, color, 0,
                                       origin, scale, SpriteEffects.None, 0);

                sprite.End();
                if (startPosition.X >= 780)
                {
                    Projectile pro = new Projectile(explosionParticles, explosionSmokeParticles, projectileTrailParticles, new Vector3(-startPosition.X + 460, startPosition.Y + 100, 0), ChaseDirection, 0, ScreenManager.Game);
                    pro.Initialize(100, 90, 50, 250, 250, 0);
                    projectiles.Add(pro);
                    ScreenManager.soundSoundBank.PlayCue("MenuExplosion");
                    letterCount++;
                    startPosition.X = -100;
                    //letter = false;
                }

            }
            else if (letterCount == 9 && !TitleMenuScreen.skip)
            {
                text = "E";
                name = "BATTLEZON";
                startPosition.X = startPosition.X + deltaTime * 1200;
                //letterPosition = new Vector2(X, Y);
                sprite.Begin();
                sprite.DrawString(font, text, startPosition, color, 0,
                                       origin, scale, SpriteEffects.None, 0);
                sprite.End();
                if (startPosition.X >= 880)
                {
                    Projectile pro = new Projectile(explosionParticles, explosionSmokeParticles, projectileTrailParticles, new Vector3(-startPosition.X + 470, startPosition.Y + 100, 0), ChaseDirection, 0, ScreenManager.Game);
                    pro.Initialize(100, 90, 50, 250, 250, 0);
                    projectiles.Add(pro);
                    ScreenManager.soundSoundBank.PlayCue("MenuExplosion");
                    letterCount++;
                    startPosition.X = -100;
                }

            }
            else if (letterCount == 10 && !TitleMenuScreen.skip)
            {
                name = "BATTLEZONE";
                //letterPosition = new Vector2(X, Y);
                //Projectile pro = new Projectile(explosionParticles, explosionSmokeParticles, projectileTrailParticles, new Vector3(-temp.X + 442, temp.Y + 100, 0), ChaseDirection, 0, ScreenManager.Game);
                //pro.Initialize(100, 50, 50, 250, 250, 0);
                //projectiles.Add(pro);
                letter = false;
                TitleMenuScreen.skip = true;
                if (startPosition.X >= 570)
                {
                    letterCount++;
                    startPosition.X = -100;
                    letter = false;
                    //letter = false;
                }

            }

            if (TitleMenuScreen.skip)
            {
                name = "BATTLEZONE";
            }
            sprite.Begin();
            sprite.DrawString(font, name, temp, color, 0,
                                   origin, 1, SpriteEffects.None, 0);
            sprite.End();
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

                ChaseDirection = new Vector3(0, 0, 1);

                Vector3 temp1;
                temp1 = Vector3.Zero;

                Random random = new Random();

                int num = random.Next(0, 100);
                if (num < 25)
                {
                    temp1 = new Vector3(random.Next(0, 1024/2), random.Next(0, 768/2), 0);
                    //temp1 = LookAt + temp1;
                    fired = false;
                }
                else if (num >=25 && num <50)
                {
                    temp1 = new Vector3(random.Next(0, 1024 / 2), -1 * random.Next(0, 768 / 2), 0);
                    //temp1 = LookAt - temp1;
                    fired = true;
                }
                else if (num >= 50 && num < 75)
                {
                    temp1 = new Vector3(-1 * random.Next(0, 1024 / 2), random.Next(0, 768 / 2), 0);
                    //temp1 = LookAt - temp1;
                    fired = true;
                }
                else if (num >= 75 && num <= 100)
                {
                    temp1 = new Vector3(-1 * random.Next(0, 1024 / 2), -1 * random.Next(0, 768 / 2), 0);
                    //temp1 = LookAt - temp1;
                    fired = true;
                }

                if (!letter)
                {
                    Projectile pro = new Projectile(explosionParticles, explosionSmokeParticles, projectileTrailParticles, temp1, ChaseDirection, 0, ScreenManager.Game);
                    pro.Initialize(100, 50, 30, 250, 250, 0);
                    projectiles.Add(pro);
                }

                timeToNextProjectile += TimeSpan.FromSeconds(random.Next(2,4));
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
                if (!projectiles[i].Update(gameTime,0))
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
        void UpdateFireExaust()
        {
            // This is trivial: we just create one new smoke particle per frame.
            const int fireParticlesPerFrame = 40;

            firePosition.X = -startPosition.X;
            firePosition.Y = startPosition.Y;

            Random random = new Random();

            for (int i = 0; i < fireParticlesPerFrame; i++)
            {
                fireParticles.AddParticle(new Vector3(-firePosition.X + random.Next(-350, 550), firePosition.Y + 150, -50), Vector3.Zero);
            }
        }

        #endregion
    }
}