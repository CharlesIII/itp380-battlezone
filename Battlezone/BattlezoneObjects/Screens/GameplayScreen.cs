#region File Description
//------------------------------------------------------------------------------
// GameplayScreen.cs
//
// Copyright (C) Double X.L., Graham Cracka, Old Jamison Irish Whiskey, & C-Cubed.
// All rights reserved.
//------------------------------------------------------------------------------
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
using System.Timers;
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
        public static Matrix ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(65.0f * (float)Math.PI / 180.0f, 4.0f / 3.0f, 0.1f, 10000.0f);

        public static Vector3 DiffuseColor = Color.Black.ToVector3();
        public static Vector3 DirLightDirection = new Vector3(1,-1,0);

        public PathFinder navPathFind;


        private float m_fTotalTime;
        
        public List<Actor> activeActors;   //list of active actors for collision checking
        private List<Actor> actorsToAdd;    //list of new actors to add to list of activeActors
        private List<Actor> actorsToRemove; //list of existing actors to be removed from list of activeActors;
        
        private PlayerTank m_kPlayer;

        private Camera e_Camera;
        public Camera Camera
        {
            get
            {
                return e_Camera;
            }
        }

        private SkyDome m_kSkyDome;

        ContentManager content;
        SpriteFont gameFont;

        Random random = new Random();

		Utils.Timer m_kTimer = new Utils.Timer();

        SpawnManager m_kSpawnManager;

        //HUD Objects
        BattlezoneObjects.HUD.HealthBar m_kHealthBar;
        BattlezoneObjects.HUD.Radar m_kRadar;
        public List<AITank> Enemies = new List<AITank>();
        BattlezoneObjects.HUD.WeaponSelect m_kWepSel;

        TimeSpan timeToNextProjectile = TimeSpan.Zero;

        //Weapon Selection: 1 Shell (defualt), 2 Missile
        private int selectedWeapon = 1;

        // The explosions effect works by firing projectiles up into the
        // air, so we need to keep track of all the active projectiles.
        List<Projectile> projectiles = new List<Projectile>();

        private ParticleSystem explosionParticles;
        private ParticleSystem explosionSmokeParticles;
        private ParticleSystem projectileTrailParticles;
        private ParticleSystem tankExaustPlumeParticles;
        private ParticleSystem fireParticles;
        private ParticleSystem TankCannonPlumeParticleSystem;

        //bool alphaBlendEnable;
        //Blend sourceBlend;
        //Blend destinationBlend;
        CullMode cullMode;
        bool depthBufferEnable;
        bool depthBufferWriteEnable;
        //bool alphaTestEnable;
        //TextureAddressMode addressU;
        //TextureAddressMode addressV;

        private float spdBoost = 1.0f;
        private Boolean spdBoostAvail = true;

        private bool justFired = false;
        private bool missileFired = false;

        private bool tankExaust = true;

        private bool tankExaustFire = true;

        public AudioManager audioManager;

        Cue cue;

        Cue soundCue;

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

            audioManager = new AudioManager(ScreenManager.Game);

            ScreenManager.Game.Components.Add(audioManager);

            //load audio
            /*
            audioEngine = new AudioEngine("Content/Sounds.xgs");
            waveBank = new WaveBank(audioEngine, "Content/XNAsteroids Waves.xwb");
            soundBank = new SoundBank(audioEngine, "Content/XNAsteroids Cues.xsb");
             */

            //load spawn manager
            m_kSpawnManager = new SpawnManager(ScreenManager.Game);
            ScreenManager.Game.Components.Add(m_kSpawnManager);

            //load camera
            e_Camera = new Camera(ScreenManager.Game);
            ScreenManager.Game.Components.Add(e_Camera);

            //Load Player Tank
            m_kPlayer = new PlayerTank(ScreenManager.Game, new Vector3(1330.0f, 0.0f, -1600.0f));
            ScreenManager.Game.Components.Add(m_kPlayer);
            /* Debug usage for AI Tank testing*/
            //m_kPlayer.Initialize();
            m_kPlayer.gamePlay = true;
            
            m_kSkyDome = new SkyDome(ScreenManager.Game, ScreenManager.GraphicsDevice);
            ScreenManager.Game.Components.Add(m_kSkyDome);

            Level l = new Level(ScreenManager.Game);
            ScreenManager.Game.Components.Add(l);

            for (int i = 0; i < 15; i++)
            {
                AITank temp = new AITank(ScreenManager.Game, navPathFind, m_kPlayer.WorldPosition);
                Enemies.Add(temp);
                ScreenManager.Game.Components.Add(temp);
            }
            

            //Load & Initialize HUD
            m_kHealthBar = new BattlezoneObjects.HUD.HealthBar(ScreenManager.Game);
            ScreenManager.Game.Components.Add(m_kHealthBar);
            m_kRadar = new Battlezone.BattlezoneObjects.HUD.Radar(ScreenManager.Game);
            ScreenManager.Game.Components.Add(m_kRadar);
            m_kWepSel = new Battlezone.BattlezoneObjects.HUD.WeaponSelect(ScreenManager.Game);
            ScreenManager.Game.Components.Add(m_kWepSel);
 

            tankExaustPlumeParticles = new SmokePlumeParticleSystemGameplay(ScreenManager.Game, content);
            explosionParticles = new ExplosionParticleSystem(ScreenManager.Game, content);
            explosionSmokeParticles = new ExplosionSmokeParticleSystemGameplay(ScreenManager.Game, content);
            projectileTrailParticles = new ProjectileTrailParticleSystemGameplay(ScreenManager.Game, content);
            fireParticles = new FireParticleSystemGameplay(ScreenManager.Game, content);

            // Set the draw order so the explosions and fire
            // will appear over the top of the smoke.
            tankExaustPlumeParticles.DrawOrder = 100;
            explosionSmokeParticles.DrawOrder = 200;
            projectileTrailParticles.DrawOrder = 300;
            explosionParticles.DrawOrder = 400;
            fireParticles.DrawOrder = 500;


            // Register the particle system components.
            ScreenManager.Game.Components.Add(explosionParticles);
            ScreenManager.Game.Components.Add(explosionSmokeParticles);
            ScreenManager.Game.Components.Add(projectileTrailParticles);
            ScreenManager.Game.Components.Add(tankExaustPlumeParticles);
            ScreenManager.Game.Components.Add(fireParticles);

            cue = ScreenManager.musicSoundBank.GetCue("BattlezoneGameplay");
            //cue.Play();

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
                    m_kTimer.Update(gameTime);
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
                    /*foreach (Actor a in activeActors)
                    {
                        Console.Out.WriteLine(a.COLLISION_IDENTIFIER);
                    }*/
                    //UpdateExplosions(gameTime);
                    //UpdateProjectiles(gameTime);
                    UpdateTankExaust();
                    ScreenManager.musicAudioEngine.Update();

                    if (spdBoost >= 2.5f)
                    {
                        UpdateFireExaust();
                    }

                    //perform activeActors maintenance
                    updateActors();

                    //check for collisions
                    checkCollision();

                    if (Enemies.Count == 0)
                    {
                        MainMenuScreen.LoadMenu(ScreenManager);
                    }
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
                    bool isTurning = false;

                    if (input.ShellSelect)
                    {
                        selectedWeapon = 1;
                        m_kWepSel.selectWeapon(selectedWeapon);
                    }
                    if (input.MissileSelect)
                    {
                        selectedWeapon = 2;
                        m_kWepSel.selectWeapon(selectedWeapon);
                    }
                    if (input.TurretLeft)
                    {
                        m_kPlayer.TurretRotation += (1.2f * deltaTime);
                    }
                    if (input.TurretRight)
                    {
                        m_kPlayer.TurretRotation -= (1.2f * deltaTime);
                    }

                    if (input.TurnLeft)
                    {
                        isTurning = true;
                        m_kPlayer.LWheelRotation -= (deltaTime);
                        m_kPlayer.RWheelRotation += (deltaTime);
                        m_kPlayer.TurretRotation -= ((float)Math.PI / 5) * deltaTime;
                        m_kPlayer.RotAngle -= ((float)Math.PI / 5) * deltaTime;
                        m_kPlayer.Quat *= Quaternion.CreateFromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), ((float)Math.PI / 5) * deltaTime);
                    }

                    if (input.TurnRight)
                    {
                        isTurning = true;
                        //m_kPlayer.TurretRotation -= ((float)Math.PI / 5) * deltaTime;
                        //m_kPlayer.SteerRotation = -(float)Math.PI / 5;
                        m_kPlayer.LWheelRotation += (deltaTime);
                        m_kPlayer.RWheelRotation -= (deltaTime);
                        m_kPlayer.TurretRotation += ((float)Math.PI / 5) * deltaTime;
                        m_kPlayer.RotAngle += ((float)Math.PI / 5) * deltaTime;
                        m_kPlayer.Quat *= Quaternion.CreateFromAxisAngle(new Vector3(0.0f, -1.0f, 0.0f), ((float)Math.PI / 5) * deltaTime);
                    }

                    if (input.Move)
                    {
                        if (!m_kPlayer.isColliding || (m_kPlayer.Velocity == new Vector3(0.0f, 0.0f, 0.0f)) || isTurning)
                        {
                            //m_kPlayer.Force = m_kPlayer.GetWorldFacing() * -1000.0f;
                            m_kPlayer.Velocity = m_kPlayer.GetWorldFacing() * -550.0f * spdBoost;
                        }
                        m_kPlayer.LWheelRotation += (4.0f * deltaTime * spdBoost);
                        m_kPlayer.RWheelRotation += (4.0f * deltaTime * spdBoost);
                        UpdateTankExaust();
                    }

                    if (!input.Move)
                    {
                        //m_kPlayer.Force = new Vector3();
                        m_kPlayer.Velocity = new Vector3(0.0f, 0.0f, 0.0f);

                    }

                    if (input.Boost)
                    {
                        if (spdBoostAvail) useBoost();
                    }

                    if (input.Reverse)
                    {
                        m_kPlayer.LWheelRotation -= (4.0f * deltaTime * spdBoost);
                        m_kPlayer.RWheelRotation -= (4.0f * deltaTime * spdBoost);

                        //m_kPlayer.Force = m_kPlayer.GetWorldFacing() * 500.0f;
                        
                        m_kPlayer.Velocity = m_kPlayer.GetWorldFacing() * 550.0f * spdBoost;
                        UpdateTankExaust();
                    }
                    if (!input.TurnLeft && !input.TurnRight)
                    {
                        m_kPlayer.SteerRotation = 0.0f;
                    }

                    if (input.Fire)
                    {
                        if (!justFired)
                        {
                            Matrix temp = m_kPlayer.worldTransform * m_kPlayer.turretBone.Transform;
                            Vector3 ChaseDirection = (temp.Forward * -1);
                            ChaseDirection.Normalize();
                            Vector3 offSet = new Vector3(0, 90, 0);
                            Projectile pro = null;
                            //Vector3 pos = m_kPlayer.WorldPosition + offSet;
                            Vector3 pos = (m_kPlayer.turretBone.Transform * m_kPlayer.worldTransform).Translation + ChaseDirection * 4;
                            switch (selectedWeapon)
                            {
                                case 1:
                                    pro = new Projectile(pos, ChaseDirection, ScreenManager.Game, Projectile.PROJECTILE_TYPE.SHELL, CollisionIdentifier.PLAYER_TANK);
                                    audioManager.Play3DCue("FireCannon",m_kPlayer);
                                    //m_kPlayer.UpdateTankCannonSmoke();
                                    break;
                                case 2:
                                    if (!missileFired)
                                    {
                                        pro = new Projectile(pos, ChaseDirection, ScreenManager.Game, Projectile.PROJECTILE_TYPE.MISSILE, CollisionIdentifier.PLAYER_TANK);
                                        audioManager.Play3DCue("FireMissile",m_kPlayer);
                                    }
                                    break;
                                default:
                                    System.Console.WriteLine("Hmm, Seems there's a bug a crawlin - selectedWeapon was neither 1 or 2.  Defaulted to Shell");
                                    pro = new Projectile(pos, ChaseDirection, ScreenManager.Game, Projectile.PROJECTILE_TYPE.SHELL, CollisionIdentifier.PLAYER_TANK);
                                    break;
                            }


                            if ((selectedWeapon == 1) || !missileFired)
                            {
                                ScreenManager.Game.Components.Add(pro);
                                pro.Initialize(400.0f, 250, 190, 100.0f, 100.0f, 0.0f);
                                m_kTimer.AddTimer("FireTimer", 3.0f, new Utils.TimerDelegate(FireEvent), false);
                                justFired = true;
                            }
                            if (selectedWeapon == 2)
                            {
                                m_kTimer.AddTimer("MissileTimer", 10.0f, new Utils.TimerDelegate(MissileEvent), false);
                                missileFired = true;
                            }
                            //activeActors.Add(new Projectile(explosionParticles, explosionSmokeParticles, projectileTrailParticles,pos,temp,2,ScreenManager.Game));
                        }
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
            
            tankExaustPlumeParticles.SetCamera(CameraMatrix, ProjectionMatrix);
            fireParticles.SetCamera(CameraMatrix, ProjectionMatrix);
            /*
            explosionParticles.SetCamera(CameraMatrix, ProjectionMatrix);
            explosionSmokeParticles.SetCamera(CameraMatrix, ProjectionMatrix);
            projectileTrailParticles.SetCamera(CameraMatrix, ProjectionMatrix);
            */
            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);

            ScreenManager.GraphicsDevice.RenderState.DepthBufferEnable = true;
            ScreenManager.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            ScreenManager.GraphicsDevice.RenderState.AlphaTestEnable = false;
        }

        


        /// <summary>
        /// Helper for updating the list of active projectiles.
        /// </summary>

        #endregion
        #region Particle Effects and Related Functions
        void UpdateTankExaust()
        {
            // This is trivial: we just create one new smoke particle per frame.


            if (tankExaust)
            {
                Vector3 temp = new Vector3(103 * (float)Math.Cos(m_kPlayer.RotAngle - (Math.PI / 3)), 85, 103 * (float)Math.Sin(m_kPlayer.RotAngle - (Math.PI / 3)));
                temp = m_kPlayer.WorldPosition + temp;
                tankExaustPlumeParticles.AddParticle(temp, Vector3.Zero);
                tankExaust = false;
            }
            else
            {
                Vector3 temp = new Vector3(103 * (float)Math.Cos(m_kPlayer.RotAngle - 2*(Math.PI / 3)), 85, 103 * (float)Math.Sin(m_kPlayer.RotAngle - 2*(Math.PI / 3)));
                temp = m_kPlayer.WorldPosition + temp;
                tankExaustPlumeParticles.AddParticle(temp, Vector3.Zero);
                tankExaust = true;
            }
        }

        void UpdateFireExaust()
        {
            // This is trivial: we just create one new smoke particle per frame.
            const int fireParticlesPerFrame = 20;

            if (tankExaustFire)
            {
                Vector3 temp = new Vector3(103 * (float)Math.Cos(m_kPlayer.RotAngle - (Math.PI / 3)), 85, 103 * (float)Math.Sin(m_kPlayer.RotAngle - (Math.PI / 3)));
                temp = m_kPlayer.WorldPosition + temp;
                for (int i = 0; i < fireParticlesPerFrame; i++)
                {
                    fireParticles.AddParticle(temp, Vector3.Zero);
                }
                tankExaustFire = false;
            }
            else
            {
                Vector3 temp = new Vector3(103 * (float)Math.Cos(m_kPlayer.RotAngle - 2 * (Math.PI / 3)), 85, 103 * (float)Math.Sin(m_kPlayer.RotAngle - 2 * (Math.PI / 3)));
                temp = m_kPlayer.WorldPosition + temp;
                for (int i = 0; i < fireParticlesPerFrame; i++)
                {
                    fireParticles.AddParticle(temp, Vector3.Zero);
                }
                tankExaustFire = true;
            }
        }

        private void SaveGraphicsDeviceState()
        {
            //alphaBlendEnable = device.RenderState.AlphaBlendEnable;
            //sourceBlend = device.RenderState.SourceBlend;
            //destinationBlend = device.RenderState.DestinationBlend;
            cullMode = ScreenManager.GraphicsDevice.RenderState.CullMode;
            depthBufferEnable = ScreenManager.GraphicsDevice.RenderState.DepthBufferEnable;
            depthBufferWriteEnable = ScreenManager.GraphicsDevice.RenderState.DepthBufferWriteEnable;
            //alphaTestEnable = device.RenderState.AlphaTestEnable;
            //addressU = device.SamplerStates[0].AddressU;
            //addressV = device.SamplerStates[0].AddressV;
        }

        private void RestoreGraphicsDeviceState()
        {
            //device.RenderState.AlphaBlendEnable = alphaBlendEnable;
            //device.RenderState.SourceBlend = sourceBlend;
            //device.RenderState.DestinationBlend = destinationBlend;
            ScreenManager.GraphicsDevice.RenderState.CullMode = cullMode;
            ScreenManager.GraphicsDevice.RenderState.DepthBufferEnable = depthBufferEnable;
            ScreenManager.GraphicsDevice.RenderState.DepthBufferWriteEnable = depthBufferWriteEnable;
            //device.RenderState.AlphaTestEnable = alphaTestEnable;
            //device.SamplerStates[0].AddressU = addressU;
            //device.SamplerStates[0].AddressV = addressV;
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
            actorsToRemove.Add(a);
        }

        /// <summary>
        /// Performs maintenance on activeActors.
        /// </summary>
        private void updateActors()
        {
            //add in all the new actors
            foreach (Actor a in actorsToAdd)
            {
                activeActors.Add(a);
            }
            actorsToAdd.Clear();

            //remove all the actors that need to be removed
            foreach (Actor a in actorsToRemove)
            {
                activeActors.Remove(a);
                if (a.COLLISION_IDENTIFIER == CollisionIdentifier.AI_TANK)
                {
                    Enemies.Remove((AITank)a);
                }
            }
            actorsToRemove.Clear();
        }

        /// <summary>
        /// Checks collisions between objects in game.
        /// </summary>
        private void checkCollision()
        {
            //terrible brute force method
            foreach (Actor a in activeActors)
            {
                if (a.COLLISION_IDENTIFIER != CollisionIdentifier.NONCOLLIDING)
                {
                    foreach (Actor b in activeActors)
                    {
                        //don't try to collide an object with itself and make sure they can collide
                        if (a != b && b.COLLISION_IDENTIFIER != CollisionIdentifier.NONCOLLIDING)
                        {
                            if (a.checkCollision(b))
                            {
                                a.collide(b);
                                b.collide(a);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Timer Functions

        public void useBoost()
        {
            spdBoost = 2.5f;
            spdBoostAvail = false;
            m_kTimer.AddTimer("Boost", 5.0f, BoostOver, false);
        }

        public void BoostOver()
        {
            spdBoost = 1.0f;
            m_kTimer.RemoveTimer("Boost");
            m_kTimer.AddTimer("BoostCD", 1.0f, BoostReady, false);
        }

        public void BoostReady()
        {
            spdBoostAvail = true;
        }

        public void FireEvent()
        {
            justFired = false;
        }

        public void MissileEvent()
        {
            missileFired = false;
            m_kTimer.RemoveTimer("MissileTimer");
        }


        #endregion

        #region misc methods

        public PlayerTank getPlayer()
        {
            return m_kPlayer;
        }

        #endregion
    }
}
