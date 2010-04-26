#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
#endregion

namespace Battlezone
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class TitleMenuScreen : TitleScreen
    {
        #region Initialization

        public static bool skip = false;
        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public TitleMenuScreen()
            : base("")
        {
            // Create our menu entries.
            TitleEntry playGameMenuEntry = new TitleEntry("Press any key to continue");

            // Hook up menu event handlers.
            playGameMenuEntry.Selected += PlayGameMenuEntrySelected;

            // Add entries to the menu.
            TitleEntries.Add(playGameMenuEntry);

        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void PlayGameMenuEntrySelected(object sender, EventArgs e)
        {
            if (skip)
            {
                try
                {
                    LoadingScreen.Load(ScreenManager, true, new BackgroundScreen(), new MainMenuScreen());
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
            else
            {
                skip = true;
            }
        }

        public static void LoadMenu(ScreenManager screenManager)
        {
            // Tell all the current screens to transition off.
            foreach (GameScreen screen in screenManager.GetScreens())
                screen.ExitScreen();

            screenManager.AddScreen(new BackgroundScreen());
            screenManager.AddScreen(new MainMenuScreen());
        }


        #endregion
    }
}