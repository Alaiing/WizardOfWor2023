using C3.MonoGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace WizardOfWor
{
    public class Character
    {
        private bool _enabled = true;
        public bool Visible { get; set; }
        public bool IsAlive => _enabled;
        private readonly SpriteSheet _spriteSheet;
        public SpriteSheet SpriteSheet => _spriteSheet;
        private Vector2 _position;
        public Vector2 Position => _position;
        public int PixelPositionX => (int)MathF.Floor(_position.X);
        public int PixelPositionY => (int)MathF.Floor(_position.Y);
        private float _currentFrame;
        private Color _color;
        public Color Color => _color;

        private Vector2 _orientation;
        private Vector2 _moveDirection;
        public Vector2 MoveDirection => _moveDirection;
        private float _currentRotation;
        public float CurrentRotation => _currentRotation;
        private Vector2 _currentScale;
        public Vector2 CurrentScale => _currentScale;
        private float _speed;
        private float _moveStep;
        private float _animationSpeed;

        public bool CanChangeDirection { get; set; }

        public Character(SpriteSheet spriteSheet)
        {
            _spriteSheet = spriteSheet;
            _currentScale = Vector2.One;
            _currentFrame = 0;
            _moveStep = 0;
            _color = Color.White;
            CanChangeDirection = true;
            LookTo(new Vector2(1, 0));
            Visible = true;
        }

        public void SetColor(Color color)
        {
            _color = color;
        }

        public void SetSpeed(float speed)
        {
            _speed = speed;
        }

        public void SetAnimationSpeed(float animationSpeed)
        {
            _animationSpeed = animationSpeed;
        }

        public void SetFrame(int frameIndex)
        {
            if (frameIndex > 0 && frameIndex < _spriteSheet.FrameCount)
            {
                _currentFrame = frameIndex;
            }
        }

        public void MoveTo(Vector2 position)
        {
            _position = position;
        }

        public void MoveBy(Vector2 translation)
        {
            _position += translation;
        }

        public void LookTo(Vector2 direction)
        {
            _moveDirection = direction;
            if (direction.X != 0)
            {
                _orientation.X = direction.X;
                _currentScale.X = direction.X;
            }
            if (direction.Y != 0)
            {
                _orientation.Y = direction.Y;
                _currentScale.Y = -_orientation.X * _orientation.Y;
            }
            else
            {
                _currentScale.Y = 1;
            }
            _orientation.Y = direction.Y;
            _currentRotation = _orientation.X * _orientation.Y * MathF.PI / 2;
        }

        public void Move(float deltaTime)
        {
            _moveStep += deltaTime * _speed;
            if (_moveStep >= 1)
            {
                _position += _moveDirection;
                _moveStep -= 1;
            }
        }

        public void Animate(float deltaTime)
        {
            _currentFrame = _currentFrame + deltaTime * _animationSpeed;
            if (_currentFrame > _spriteSheet.FrameCount)
            {
                _currentFrame = 0;
            }
        }

        public void Die()
        {
            _enabled = false;
        }

        public void Draw(SpriteBatch spriteBatch, int displayOffsetX = 0, int displayOffsetY = 0)
        {
            if(Visible)
            {
                _spriteSheet.DrawFrame((int)MathF.Floor(_currentFrame), spriteBatch, new Vector2(PixelPositionX + displayOffsetX, PixelPositionY + displayOffsetY), _currentRotation, _currentScale, _color);
            }
        }
    }
}
