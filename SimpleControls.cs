using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardOfWor
{
    public class SimpleControls
    {
        public enum PlayerNumber { Player1, Player2 }
        private static KeyboardState _keyboardState;
        private static GamePadState _gamePadStatePlayer1;
        private static GamePadState _gamePadStatePlayer2;

        public static void GetStates()
        {
            _keyboardState = Keyboard.GetState();
            _gamePadStatePlayer1 = GamePad.GetState(PlayerIndex.One);
            _gamePadStatePlayer2 = GamePad.GetState(PlayerIndex.Two);
        }

        public static bool IsAnyMoveKeyDown(PlayerNumber playerNumber)
        {
            return IsLeftDown(playerNumber) || IsRightDown(playerNumber) || IsUpDown(playerNumber) || IsDownDown(playerNumber);
        }

        public static bool IsLeftDown(PlayerNumber playerNumber)
        {
            if (playerNumber == PlayerNumber.Player1 && _keyboardState.IsKeyDown(Keys.Left)
                || playerNumber == PlayerNumber.Player2 && _keyboardState.IsKeyDown(Keys.G))
            {
                return true;
            }

            GamePadState currentGamepadState = playerNumber == PlayerNumber.Player1 ? _gamePadStatePlayer1 : _gamePadStatePlayer2;

            if (currentGamepadState.IsConnected)
            {
                if (currentGamepadState.DPad.Left == ButtonState.Pressed)
                {
                    return true;
                }

                if (currentGamepadState.ThumbSticks.Left.X < 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsRightDown(PlayerNumber playerNumber)
        {
            if (playerNumber == PlayerNumber.Player1 && _keyboardState.IsKeyDown(Keys.Right)
                || playerNumber == PlayerNumber.Player2 && _keyboardState.IsKeyDown(Keys.J))
            {
                return true;
            }

            GamePadState currentGamepadState = playerNumber == PlayerNumber.Player1 ? _gamePadStatePlayer1 : _gamePadStatePlayer2;

            if (currentGamepadState.IsConnected)
            {
                if (currentGamepadState.DPad.Right == ButtonState.Pressed)
                {
                    return true;
                }

                if (currentGamepadState.ThumbSticks.Left.X > 0)
                {
                    return true;
                }
            }

            return false;
        }
        public static bool IsUpDown(PlayerNumber playerNumber)
        {
            if (playerNumber == PlayerNumber.Player1 && _keyboardState.IsKeyDown(Keys.Up)
                || playerNumber == PlayerNumber.Player2 && _keyboardState.IsKeyDown(Keys.Y))
            {
                return true;
            }

            GamePadState currentGamepadState = playerNumber == PlayerNumber.Player1 ? _gamePadStatePlayer1 : _gamePadStatePlayer2;

            if (currentGamepadState.IsConnected)
            {
                if (_gamePadStatePlayer1.DPad.Up == ButtonState.Pressed)
                {
                    return true;
                }

                if (currentGamepadState.ThumbSticks.Left.Y > 0)
                {
                    return true;
                }
            }

            return false;
        }
        public static bool IsDownDown(PlayerNumber playerNumber)
        {
            if (playerNumber == PlayerNumber.Player1 && _keyboardState.IsKeyDown(Keys.Down)
                || playerNumber == PlayerNumber.Player2 && _keyboardState.IsKeyDown(Keys.H))
            {
                return true;
            }

            GamePadState currentGamepadState = playerNumber == PlayerNumber.Player1 ? _gamePadStatePlayer1 : _gamePadStatePlayer2;

            if (currentGamepadState.IsConnected)
            {
                if (currentGamepadState.DPad.Down == ButtonState.Pressed)
                {
                    return true;
                }

                if (currentGamepadState.ThumbSticks.Left.Y < 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsADown(PlayerNumber playerNumber)
        {
            if (playerNumber == PlayerNumber.Player1 && _keyboardState.IsKeyDown(Keys.Space)
                || playerNumber == PlayerNumber.Player2 && _keyboardState.IsKeyDown(Keys.F))
            {
                return true;
            }

            GamePadState currentGamepadState = playerNumber == PlayerNumber.Player1 ? _gamePadStatePlayer1 : _gamePadStatePlayer2;

            if (currentGamepadState.IsConnected)
            {
                if (currentGamepadState.Buttons.A == ButtonState.Pressed)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsBDown(PlayerNumber playerNumber)
        {
            if (playerNumber == PlayerNumber.Player1 && _keyboardState.IsKeyDown(Keys.Enter)
                || playerNumber == PlayerNumber.Player2 && _keyboardState.IsKeyDown(Keys.D))
            {
                return true;
            }

            GamePadState currentGamepadState = playerNumber == PlayerNumber.Player1 ? _gamePadStatePlayer1 : _gamePadStatePlayer2;

            if (currentGamepadState.IsConnected)
            {
                if (currentGamepadState.Buttons.B == ButtonState.Pressed)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsStartDown()
        {
            if (_keyboardState.IsKeyDown(Keys.F1))
            {
                return true;
            }

            if (_gamePadStatePlayer1.IsConnected)
            {
                if (_gamePadStatePlayer1.Buttons.Start == ButtonState.Pressed)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsSelectDown()
        {
            if (_keyboardState.IsKeyDown(Keys.F2))
            {
                return true;
            }

            if (_gamePadStatePlayer1.IsConnected)
            {
                if (_gamePadStatePlayer1.Buttons.Back == ButtonState.Pressed)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsEscapeDown()
        {
            return _keyboardState.IsKeyDown(Keys.Escape);
        }

        public static bool IsCheatKillDown()
        {
            return _keyboardState.IsKeyDown(Keys.F3);
        }

    }
}
