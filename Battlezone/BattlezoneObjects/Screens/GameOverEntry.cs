﻿#region File Description
//-----------------------------------------------------------------------------
// MenuEntry.cs
//
// XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Battlezone
{
    /// <summary>
    /// Helper class represents a single entry in a MenuScreen. By default this
    /// just draws the entry text string, but it can be customized to display menu
    /// entries in different ways. This also provides an event that will be raised
    /// when the menu entry is selected.
    /// </summary>
    class GameOverEntry
    {
        #region Fields

        /// <summary>
        /// The text rendered for this entry.
        /// </summary>
        string text;
        public bool skip = false;

        /// <summary>
        /// Tracks a fading selection effect on the entry.
        /// </summary>
        /// <remarks>
        /// The entries transition out of the selection effect when they are deselected.
        /// </remarks>
        float selectionFade;

        #endregion

        #region Properties


        /// <summary>
        /// Gets or sets the text of this menu entry.
        /// </summary>
        public string Text
        {
            get { return text; }
            set { text = value; }
        }


        #endregion

        #region Events


        /// <summary>
        /// Event raised when the menu entry is selected.
        /// </summary>
        public event EventHandler<EventArgs> Selected;


        /// <summary>
        /// Method for raising the Selected event.
        /// </summary>
        protected internal virtual void OnSelectEntry()
        {
            if (Selected != null)
                Selected(this, EventArgs.Empty);
        }


        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new menu entry with the specified text.
        /// </summary>
        public GameOverEntry(string text)
        {
            this.text = text;
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the menu entry.
        /// </summary>
        public virtual void Update(GameOverScreen screen, bool isSelected,
                                                      GameTime gameTime)
        {
            // When the menu selection changes, entries gradually fade between
            // their selected and deselected appearance, rather than instantly
            // popping to the new state.
            float fadeSpeed = (float)gameTime.ElapsedGameTime.TotalSeconds * 4;

            if (isSelected)
                selectionFade = Math.Min(selectionFade + fadeSpeed, 1);
            else
                selectionFade = Math.Max(selectionFade - fadeSpeed, 0);
        }


        /// <summary>
        /// Draws the menu entry. This can be overridden to customize the appearance.
        /// </summary>
        public virtual void Draw(GameOverScreen screen, Vector2 position,
                                 bool isSelected, GameTime gameTime)
        {
            
            // Draw the selected entry in yellow, otherwise white.
            Color color = isSelected ? Color.Blue : Color.White;

            // Pulsate the size of the selected menu entry.
            double time = gameTime.TotalGameTime.TotalSeconds;

            float pulsate = (float)Math.Sin(time * 6) + 1;

            float scale = 1;

            // Modify the alpha to fade text out during transitions.
            color = new Color(color.R, color.G, color.B, screen.TransitionAlpha);

            // Draw text, centered on the middle of each line.
            ScreenManager screenManager = screen.ScreenManager;
            SpriteBatch spriteBatch = screenManager.SpriteBatch;
            SpriteFont font = screenManager.Font;

            spriteBatch.Begin();

            Vector2 origin = new Vector2(0, font.LineSpacing / 2);
            Vector2 message = new Vector2(50,50);
            if (GameOverMenuScreen.win)
            {
                spriteBatch.DrawString(font, "MISSION SUCCESS!", message, color, 0,
                     origin, scale, SpriteEffects.None, 0);
            }
            else
            {
                spriteBatch.DrawString(font, "MISSION FAILURE!", message, color, 0,
                origin, scale, SpriteEffects.None, 0);
            }

                spriteBatch.DrawString(font, text, position, color, 0,
                                   origin, scale, SpriteEffects.None, 0);

                spriteBatch.End();
        }


        /// <summary>
        /// Queries how much space this menu entry requires.
        /// </summary>
        public virtual int GetHeight(GameOverScreen screen)
        {
            return screen.ScreenManager.Font.LineSpacing;
        }


        #endregion
    }
}