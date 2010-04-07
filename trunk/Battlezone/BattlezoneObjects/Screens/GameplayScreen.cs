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
        public static Matrix ProjectionMatrix = Matrix.CreateOrthographic(1024,768,0.1f,10000.0f);

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

        ContentManager content;
        SpriteFont gameFont;

        Random random = new Random();

		Utils.Timer m_kTimer = new Utils.Timer();

        SpawnManager m_kSpawnManager;

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
                    navPathFind = new PathFinder("Navigation Nodes.txt");
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to load path finding system.");
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

                    //perform activeActors maintenance
                    updateActors();

                    //check for collisions
                    checkCollision();
                }
            }
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
		public override void HandleInput(InputState input, GameTime gameTime)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (input.PauseGame)
            {
                // If they pressed pause, bring up the pause menu screen.
                ScreenManager.AddScreen(new PauseMenuScreen());
            }
            else
            {
                //figure out where input should go
            }
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.Black, 0, 0);
            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);
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
