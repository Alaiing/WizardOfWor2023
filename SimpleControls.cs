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
        private static KeyboardState _keyboardState;
        private static GamePadState _gamePadState;

        public static void GetStates()
        {
            _keyboardState = Keyboard.GetState();
            _gamePadState = GamePad.GetState(PlayerIndex.One);
        }

        public static bool IsAnyMoveKeyDown()
        {
            return IsLeftDown() || IsRightDown() || IsUpDown() || IsDownDown();
        }

        public static bool IsLeftDown()
        {
            if (_keyboardState.IsKeyDown(Keys.Left))
            {
                return true;
            }

            if (_gamePadState.IsConnected)
            {
                if (_gamePadState.DPad.Left == ButtonState.Pressed)
                {
                    return true;
                }

                if (_gamePadState.ThumbSticks.Left.X < 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsRightDown()
        {
            if (_keyboardState.IsKeyDown(Keys.Right))
            {
                return true;
            }

            if (_gamePadState.IsConnected)
            {
                if (_gamePadState.DPad.Right == ButtonState.Pressed)
                {
                    return true;
                }

                if (_gamePadState.ThumbSticks.Left.X > 0)
                {
                    return true;
                }
            }

            return false;
        }
        public static bool IsUpDown()
        {
            if (_keyboardState.IsKeyDown(Keys.Up))
            {
                return true;
            }

            if (_gamePadState.IsConnected)
            {
                if (_gamePadState.DPad.Up == ButtonState.Pressed)
                {
                    return true;
                }

                if (_gamePadState.ThumbSticks.Left.Y > 0)
                {
                    return true;
                }
            }

            return false;
        }
        public static bool IsDownDown()
        {
            if (_keyboardState.IsKeyDown(Keys.Down))
            {
                return true;
            }

            if (_gamePadState.IsConnected)
            {
                if (_gamePadState.DPad.Down == ButtonState.Pressed)
                {
                    return true;
                }

                if (_gamePadState.ThumbSticks.Left.Y < 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsADown()
        {
            if (_keyboardState.IsKeyDown(Keys.Space))
            {
                return true;
            }

            if (_gamePadState.IsConnected)
            {
                if (_gamePadState.Buttons.A == ButtonState.Pressed)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsBDown()
        {
            if (_keyboardState.IsKeyDown(Keys.Enter))
            {
                return true;
            }

            if (_gamePadState.IsConnected)
            {
                if (_gamePadState.Buttons.B == ButtonState.Pressed)
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

            if (_gamePadState.IsConnected)
            {
                if (_gamePadState.Buttons.Start == ButtonState.Pressed)
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

            if (_gamePadState.IsConnected)
            {
                if (_gamePadState.Buttons.Back == ButtonState.Pressed)
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

    }
}
