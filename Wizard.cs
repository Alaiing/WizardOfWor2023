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
    internal class Wizard : Enemy
    {
        public const float TELEPORT_COOLDOWN = 1f;
        private readonly Level _level;

        private float _teleportTimer;

        public Wizard(SpriteSheet spriteSheet, Level level, int score) : base(spriteSheet, Color.White, canBecomeInvisible:false, score) 
        {
            SetSpeed(SPEED_2);
            SetAnimationSpeed(10);
            _preferredHorizontalDirection = 0;
            _level = level;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            _teleportTimer += deltaTime;
            if (_teleportTimer > TELEPORT_COOLDOWN) 
            { 
                _teleportTimer -= TELEPORT_COOLDOWN;
                MoveTo(_level.GetRandomPosition());
                LookTo(_level.PickPossibleDirection(this, out int _));
                if (!IsAnyEnemyFiring())
                {
                    Fire();
                }
            }
        }

        public override bool CanFireAtPlayer(Player player)
        {
            return false;
        }

        public override void SetThresholdSpeed(int threshold, float modificator)
        {
            // DO NOTHING
        }
    }
}
