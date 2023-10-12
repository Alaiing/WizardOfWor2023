using C3.MonoGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.CompilerServices;


namespace WizardOfWor
{
    internal class Death
    {
        const float ANIMATION_SPEED = 10;

        private bool _enabled;
        public bool Enabled => _enabled;
        private SpriteSheet _spriteSheet;
        private int _positionX;
        private int _positionY;
        private float _orientation;
        private Vector2 _scale;
        private float _currentFrame;
        private Color _color;

        public Death(SpriteSheet spriteSheet, int x, int y, Color color, float orientation, Vector2 scale)
        {
            _spriteSheet = spriteSheet;
            _positionX = x;
            _positionY = y;
            _currentFrame = 0;
            _color = color;
            _enabled = true;
            _scale = scale;
            _orientation = orientation;
        }

        public void Update(float deltaTime)
        {
            _currentFrame = _currentFrame + deltaTime * ANIMATION_SPEED;
            if (_currentFrame > _spriteSheet.FrameCount)
            {
                _enabled = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch, int displayOffsetX = 0, int displayOffsetY = 0)
        {
            _spriteSheet.DrawFrame((int)MathF.Floor(_currentFrame), spriteBatch, new Vector2(_positionX + displayOffsetX, _positionY + displayOffsetY), _orientation, _scale, _color);
        }
    }
}
