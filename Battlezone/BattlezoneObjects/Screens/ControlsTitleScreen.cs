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
    class ControlsTitleScreen : TitleScreen
    {
        #region Initialization

        public static bool win = false;
        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public ControlsTitleScreen()
            : base("")
        {
            // Create our menu entries.
            // Create our menu entries.
            TitleEntry controls = new TitleEntry("Controls:");
            TitleEntry W = new TitleEntry("W - Move Foward");
            TitleEntry S = new TitleEntry("S - Move Backward");
            TitleEntry A = new TitleEntry("A - Turn Left");
            TitleEntry D = new TitleEntry("D - Turd Right");
            TitleEntry Wep1 = new TitleEntry("1 - Turret Select");
            TitleEntry Wep2 = new TitleEntry("2 - Missile Select");
            TitleEntry Boost = new TitleEntry("G - Boost!");
            TitleEntry Fire = new TitleEntry("SpaceBar - Fire Weapon");


            TitleEntry backMenuEntry = new TitleEntry("Back");

            // Hook up menu event handlers.
            backMenuEntry.Selected += BackMenuEntrySelected;

            // Add entries to the menu.
            TitleEntries.Add(controls);
            TitleEntries.Add(W);
            TitleEntries.Add(S);
            TitleEntries.Add(A);
            TitleEntries.Add(D);
            TitleEntries.Add(Wep1);
            TitleEntries.Add(Wep2);
            TitleEntries.Add(Boost);
            TitleEntries.Add(Fire);
            TitleEntries.Add(backMenuEntry);

        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void BackMenuEntrySelected(object sender, EventArgs e)
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

        public static void LoadMenu(ScreenManager screenManager, bool winner)
        {
            // Tell all the current screens to transition off.
            foreach (GameScreen screen in screenManager.GetScreens())
                screen.ExitScreen();

        }


        #endregion
    }
}