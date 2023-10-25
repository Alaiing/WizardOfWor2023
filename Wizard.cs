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
        private readonly Level _level;

        private float _teleportTimer;
        private float _teleportCooldown;

        public Wizard(SpriteSheet spriteSheet, Level level, int score) : base(spriteSheet, Color.White, canBecomeInvisible:false, score) 
        {
            SetSpeed(ConfigManager.GetConfig(Constants.WIZARD_SPEED, Constants.DEFAULT_WIZARD_SPEED));
            SetAnimationSpeed(10);
            _preferredHorizontalDirection = 0;
            _teleportCooldown = ConfigManager.GetConfig(Constants.WIZARD_TELEPORT_COOLDOWN, Constants.DEFAULT_WIZARD_TELEPORT_COOLDOWN);
            _level = level;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            _teleportTimer += deltaTime;
            if (_teleportTimer > _teleportCooldown) 
            { 
                _teleportTimer -= _teleportCooldown;
                MoveTo(_level.GetRandomPosition());
                LookTo(_level.PickPossibleDirection(this, out int _));
                if (!IsAnyEnemyFiring())
                {
                    Fire();
                }
                CameraShake.Shake(1, 100, 0.2f);
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
