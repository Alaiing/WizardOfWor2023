using C3.MonoGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace WizardOfWor
{
    public class Bullet
    {
        public enum TargetTypes { Player, Any }

        private readonly Color _color;
        private readonly ShootingCharacter _origin;
        public ShootingCharacter Origin => _origin;
        private Vector2 _position;
        public int PixelPositionX => (int)MathF.Floor(_position.X);
        public int PixelPositionY => (int)MathF.Floor(_position.Y);
        private readonly Vector2 _velocity;
        private readonly TargetTypes _targetType;
        public TargetTypes TargetType => _targetType;

        public Bullet(ShootingCharacter origin, TargetTypes targetType, float speed)
        {
            _origin = origin;
            _position = origin.Position + origin.SpriteSheet.SpritePivot;
            _velocity = origin.MoveDirection * speed;
            _color = origin.Color;
            _targetType = targetType;
        }

        public void Update(float deltaTime)
        {
            _position += _velocity * deltaTime;
        }

        public bool TestHit(Character character)
        {
            int distanceX = (int)MathF.Abs(character.PixelPositionX - PixelPositionX + character.SpriteSheet.SpritePivot.X);
            int distanceY = (int)MathF.Abs(character.PixelPositionY - PixelPositionY + character.SpriteSheet.SpritePivot.Y);

            return distanceX <= character.SpriteSheet.SpritePivot.X && distanceY <= character.SpriteSheet.SpritePivot.Y;
        }

        public void Draw(SpriteBatch spriteBatch, int displayOffsetX = 0, int displayOffsetY = 0)
        {
            spriteBatch.PutPixel(PixelPositionX + displayOffsetX, PixelPositionY + displayOffsetY, _color);
        }
    }
}
