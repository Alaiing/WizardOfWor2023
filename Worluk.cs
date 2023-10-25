using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardOfWor
{
    public class Worluk : Enemy
    {
        public Worluk(SpriteSheet spriteSheet, Color color, int preferredDirection, int score) : base(spriteSheet, color, false, score)
        {
            SetSpeed(ConfigManager.GetConfig(Constants.WORLUK_SPEED, Constants.DEFAULT_WORLUK_SPEED));
            SetAnimationSpeed(20);
            _preferredHorizontalDirection = preferredDirection;
        }

        public override bool CanFireAtPlayer(Player player)
        {
            return false;
        }

        public override void Draw(SpriteBatch spriteBatch, int displayOffsetX = 0, int displayOffsetY = 0)
        {
            _currentScale = Vector2.One;
            _currentRotation = 0;
            base.Draw(spriteBatch, displayOffsetX, displayOffsetY);
        }

        public override void SetThresholdSpeed(int threshold, float modificator)
        {
            // DO NOTHING
        }
    }
}
