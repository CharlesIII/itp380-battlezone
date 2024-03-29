#region File Description
//-----------------------------------------------------------------------------
// InputState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Battlezone
{
    /// <summary>
    /// Helper for reading input from keyboard and gamepad. This class tracks both
    /// the current and previous state of both input devices, and implements query
    /// properties for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    public class InputState
    {
        #region Fields

        public const int MaxInputs = 4;

        public readonly KeyboardState[] CurrentKeyboardStates;
        public readonly GamePadState[] CurrentGamePadStates;

        public readonly KeyboardState[] LastKeyboardStates;
        public readonly GamePadState[] LastGamePadStates;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new input state.
        /// </summary>
        public InputState()
        {
            CurrentKeyboardStates = new KeyboardState[MaxInputs];
            CurrentGamePadStates = new GamePadState[MaxInputs];

            LastKeyboardStates = new KeyboardState[MaxInputs];
            LastGamePadStates = new GamePadState[MaxInputs];
        }


        #endregion

        #region Properties


        /// <summary>
        /// Checks for a "menu up" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool MenuUp
        {
            get
            {
                return IsNewKeyPress(Keys.Up) ||
                       IsNewButtonPress(Buttons.DPadUp) ||
                       IsNewButtonPress(Buttons.LeftThumbstickUp);
            }
        }


        /// <summary>
        /// Checks for a "menu down" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool MenuDown
        {
            get
            {
                return IsNewKeyPress(Keys.Down) ||
                       IsNewButtonPress(Buttons.DPadDown) ||
                       IsNewButtonPress(Buttons.LeftThumbstickDown);
            }
        }


        /// <summary>
        /// Checks for a "menu select" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool MenuSelect
        {
            get
            {
                return IsNewKeyPress(Keys.Space) ||
                       IsNewKeyPress(Keys.Enter) ||
                       IsNewButtonPress(Buttons.A) ||
                       IsNewButtonPress(Buttons.Start);
            }
        }


        /// <summary>
        /// Checks for a "menu cancel" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool MenuCancel
        {
            get
            {
                return IsNewKeyPress(Keys.Escape) ||
                       IsNewButtonPress(Buttons.B) ||
                       IsNewButtonPress(Buttons.Back);
            }
        }


        /// <summary>
        /// Checks for a "pause the game" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool PauseGame
        {
            get
            {
                return IsNewKeyPress(Keys.Escape) ||
                       IsNewButtonPress(Buttons.Back) ||
                       IsNewButtonPress(Buttons.Start);
            }
        }

        /*
         * Goes pew pew pew
         */
        public bool Fire
        {
            get
            {
                return IsNewKeyPress(Keys.Space) ||
                       IsNewButtonPress(Buttons.RightTrigger) ||
                       IsKeyHeld(Keys.Space) ||
                       IsButtonHeld(Buttons.RightTrigger);
            }
        }

        /*
         * Check out mah missile
         */
        public bool MissileSelect
        {
            get
            {
                return IsNewKeyPress(Keys.D2) ||
                       IsNewButtonPress(Buttons.LeftShoulder) ||
                       IsKeyHeld(Keys.D2) ||
                       IsButtonHeld(Buttons.LeftShoulder);
            }
        }

        /*
         * The ladies say I have a big gun
         */
        public bool ShellSelect
        {
            get
            {
                return IsNewKeyPress(Keys.D1) ||
                       IsNewButtonPress(Buttons.RightShoulder) ||
                       IsKeyHeld(Keys.D1) ||
                       IsButtonHeld(Buttons.RightShoulder);
            }
        }


        /*
         * Goes vroom vroom
         */
        public bool Boost
        {
            get
            {
                return IsNewKeyPress(Keys.G) ||
                       IsNewButtonPress(Buttons.DPadLeft) ||
                       IsKeyHeld(Keys.G) ||
                       IsButtonHeld(Buttons.DPadLeft);
            }
        }


        /*
         * Turns the tank left
         */
        public bool TurnLeft
        {
            get
            {
                return IsNewKeyPress(Keys.A) ||
                       IsKeyHeld(Keys.A);
            }
        }

        /*
         * Turns the tank left
         */
        public bool TurretLeft
        {
            get
            {
                return IsNewKeyPress(Keys.Left) ||
                       IsKeyHeld(Keys.Left);
            }
        }

        /*
         * Turns the tank left
         */
        public bool TurretRight
        {
            get
            {
                return IsNewKeyPress(Keys.Right) ||
                       IsKeyHeld(Keys.Right);
            }
        }


        /*
         * Turns the tank right
         */
        public bool TurnRight
        {
            get
            {
                return IsNewKeyPress(Keys.D) ||
                       IsKeyHeld(Keys.D);
            }
        }

        /*
         * Moves the tank forward
         */
        public bool Move
        {
            get
            {
                return IsNewKeyPress(Keys.W) ||
                       IsKeyHeld(Keys.W);
            }
        }

        /*
         * Moves the tank backward
         */
        public bool Reverse
        {
            get
            {
                return IsNewKeyPress(Keys.S) ||
                       IsKeyHeld(Keys.S);
            }
        }

        public bool AnyKey
        {
            get
            {
                return Keyboard.GetState().GetPressedKeys().Length > 0;
            }
        }


        #endregion

        #region Methods


        /// <summary>
        /// Reads the latest state of the keyboard and gamepad.
        /// </summary>
        public void Update()
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                LastKeyboardStates[i] = CurrentKeyboardStates[i];
                LastGamePadStates[i] = CurrentGamePadStates[i];

                CurrentKeyboardStates[i] = Keyboard.GetState((PlayerIndex)i);
                CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);
            }
        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update,
        /// by any player.
        /// </summary>
        public bool IsNewKeyPress(Keys key)
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                if (IsNewKeyPress(key, (PlayerIndex)i))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update,
        /// by the specified player.
        /// </summary>
        public bool IsNewKeyPress(Keys key, PlayerIndex playerIndex)
        {
            return (CurrentKeyboardStates[(int)playerIndex].IsKeyDown(key) &&
                    LastKeyboardStates[(int)playerIndex].IsKeyUp(key));
        }

        //Helper function for cheking if a key is being held.
        public bool IsKeyHeld(Keys key)
        {
            return (CurrentKeyboardStates[0].IsKeyDown(key) &&
                    LastKeyboardStates[0].IsKeyDown(key));
        }
        //Helper function for cheking if a key is being held.
        public bool IsKeyHeld(Keys key, PlayerIndex playerIndex)
        {
            return (CurrentKeyboardStates[(int)playerIndex].IsKeyDown(key) &&
                    LastKeyboardStates[(int)playerIndex].IsKeyDown(key));
        }

        /// <summary>
        /// Helper for checking if a button was newly pressed during this update,
        /// by any player.
        /// </summary>
        public bool IsNewButtonPress(Buttons button)
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                if (IsNewButtonPress(button, (PlayerIndex)i))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Helper for checking if a button was newly pressed during this update,
        /// by the specified player.
        /// </summary>
        public bool IsNewButtonPress(Buttons button, PlayerIndex playerIndex)
        {
            return (CurrentGamePadStates[(int)playerIndex].IsButtonDown(button) &&
                    LastGamePadStates[(int)playerIndex].IsButtonUp(button));
        }

        //Helper function for checking if a button has been held during this update.
        public bool IsButtonHeld(Buttons button)
        {
            return (CurrentGamePadStates[0].IsButtonDown(button) &&
                    LastGamePadStates[0].IsButtonDown(button));
        }

        //Helper function for checking if a button has been held during this update.
        public bool IsButtonHeld(Buttons button, PlayerIndex playerIndex)
        {
            return (CurrentGamePadStates[(int)playerIndex].IsButtonDown(button) &&
                    LastGamePadStates[(int)playerIndex].IsButtonDown(button));
        }

        /// <summary>
        /// Checks for a "menu select" input action from the specified player.
        /// </summary>
        public bool IsMenuSelect(PlayerIndex playerIndex)
        {
            return IsNewKeyPress(Keys.Space, playerIndex) ||
                   IsNewKeyPress(Keys.Enter, playerIndex) ||
                   IsNewButtonPress(Buttons.A, playerIndex) ||
                   IsNewButtonPress(Buttons.Start, playerIndex);
        }


        /// <summary>
        /// Checks for a "menu cancel" input action from the specified player.
        /// </summary>
        public bool IsMenuCancel(PlayerIndex playerIndex)
        {
            return IsNewKeyPress(Keys.Escape, playerIndex) ||
                   IsNewButtonPress(Buttons.B, playerIndex) ||
                   IsNewButtonPress(Buttons.Back, playerIndex);
        }


        #endregion
    }
}
