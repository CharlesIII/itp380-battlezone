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
    class ControlsTitleScreen : MenuScreen
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
            MenuEntry controls = new MenuEntry("Controls:");
            MenuEntry W = new MenuEntry("W - Move Foward");
            MenuEntry S = new MenuEntry("S - Move Backward");
            MenuEntry A = new MenuEntry("A - Turn Left");
            MenuEntry D = new MenuEntry("D - Turd Right");
            MenuEntry Wep1 = new MenuEntry("1 - Turret Select");
            MenuEntry Wep2 = new MenuEntry("2 - Missile Select");
            MenuEntry Boost = new MenuEntry("G - Boost!");
            MenuEntry Fire = new MenuEntry("SpaceBar - Fire Weapon");


            MenuEntry backMenuEntry = new MenuEntry("Back");

            // Hook up menu event handlers.
            backMenuEntry.Selected += BackMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(controls);
            MenuEntries.Add(W);
            MenuEntries.Add(S);
            MenuEntries.Add(A);
            MenuEntries.Add(D);
            MenuEntries.Add(Wep1);
            MenuEntries.Add(Wep2);
            MenuEntries.Add(Boost);
            MenuEntries.Add(Fire);
            MenuEntries.Add(backMenuEntry);

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