#region File Description
//------------------------------------------------------------------------------
// BattlezoneGame.cs
//
// Copyright (C) Double XL, Graham Cracka, Old Jamison Irish Whiskey, & C-Cubed.
// All rights reserved.
//------------------------------------------------------------------------------
#endregion


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

namespace Battlezone
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class BattlezoneGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        ScreenManager screenManager;
        public Utils.FrameRateCounter m_kFrameRate;



        #region Properties
        public float fTargetMsPerFrame
        {
            get { return (float)TargetElapsedTime.Milliseconds; }
            set
            {
                if (value > 1.0f)
                    TargetElapsedTime = System.TimeSpan.FromMilliseconds(value);
                else
                    TargetElapsedTime = System.TimeSpan.FromMilliseconds(1.0f);
            }
        }
        #endregion

        public BattlezoneGame()
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;

            graphics.MinimumPixelShaderProfile = ShaderProfile.PS_2_0;

            // Create the screen manager component.
            screenManager = new ScreenManager(this);

            Components.Add(screenManager);

            // Activate the first screens.
            screenManager.AddScreen(new TitleBackgroundScreen());
            screenManager.AddScreen(new TitleMenuScreen());

            m_kFrameRate = new Utils.FrameRateCounter(this, new Vector2(750.0f, 600.0f));
            Components.Add(m_kFrameRate);

            // For testing purposes, let's disable fixed time step and vsync.
            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;


        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            IsFixedTimeStep = true;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
