﻿#region File Description
//-----------------------------------------------------------------------------
// MenuScreen.cs
//
// XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Battlezone
{
    /// <summary>
    /// Base class for screens that contain a menu of options. The user can
    /// move up and down to select an entry, or cancel to back out of the screen.
    /// </summary>
    abstract class TitleScreen : GameScreen
    {
        #region Fields

        List<TitleEntry> titleEntries = new List<TitleEntry>();
        int selectedEntry = 0;
        string menuTitle;

        #endregion

        #region Properties


        /// <summary>
        /// Gets the list of menu entries, so derived classes can add
        /// or change the menu contents.
        /// </summary>
        protected IList<TitleEntry> TitleEntries
        {
            get { return titleEntries; }
        }


        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public TitleScreen(string menuTitle)
        {
            this.menuTitle = menuTitle;

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(1);
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the menu.
        /// </summary>
        public override void HandleInput(InputState input, GameTime gameTime)
        {
            // Move to the previous menu entry?
            if (input.MenuUp)
            {
                selectedEntry--;

                if (selectedEntry < 0)
                    selectedEntry = titleEntries.Count - 1;
            }

            // Move to the next menu entry?
            if (input.MenuDown)
            {
                selectedEntry++;

                if (selectedEntry >= titleEntries.Count)
                    selectedEntry = 0;
            }

            // Accept or cancel the menu?
            if (input.AnyKey)
            {
                OnSelectEntry(selectedEntry);
            }
            else if (input.MenuCancel)
            {
                OnCancel();
            }
        }


        /// <summary>
        /// Handler for when the user has chosen a menu entry.
        /// </summary>
        protected virtual void OnSelectEntry(int entryIndex)
        {
            titleEntries[selectedEntry].OnSelectEntry();
        }


        /// <summary>
        /// Handler for when the user has cancelled the menu.
        /// </summary>
        protected virtual void OnCancel()
        {
            ExitScreen();
        }


        /// <summary>
        /// Helper overload makes it easy to use OnCancel as a MenuEntry event handler.
        /// </summary>
        protected void OnCancel(object sender, EventArgs e)
        {
            OnCancel();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the menu.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Update each nested MenuEntry object.
            for (int i = 0; i < titleEntries.Count; i++)
            {
                bool isSelected = IsActive && (i == selectedEntry);

                titleEntries[i].Update(this, isSelected, gameTime);
            }
        }


        /// <summary>
        /// Draws the menu.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            Vector2 position = new Vector2(225, 576);

            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            if (ScreenState == ScreenState.TransitionOn)
                position.X -= transitionOffset * 256;
            else
                position.X += transitionOffset * 512;

            spriteBatch.Begin();

            // Draw each menu entry in turn.
            for (int i = 0; i < titleEntries.Count; i++)
            {
                TitleEntry titleEntry = titleEntries[i];

                bool isSelected = IsActive && (i == selectedEntry);

                titleEntry.Draw(this, position, isSelected, gameTime);

                position.Y += titleEntry.GetHeight(this);
            }

            // Draw the menu title.
            Vector2 titlePosition = new Vector2(512, 80);
            Vector2 titleOrigin = font.MeasureString(menuTitle) / 2;
            Color titleColor = new Color(192, 192, 192, TransitionAlpha);
            float titleScale = 1.25f;

            titlePosition.Y -= transitionOffset * 100;

            //spriteBatch.DrawString(font, menuTitle, titlePosition, titleColor, 0,
                                   //titleOrigin, titleScale, SpriteEffects.None, 0);

            spriteBatch.End();
            ScreenManager.GraphicsDevice.RenderState.DepthBufferEnable = true;
            ScreenManager.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            ScreenManager.GraphicsDevice.RenderState.AlphaTestEnable = false;
        }


        #endregion
    }
}

