using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardOfWor
{
    internal class Enemy : ShootingCharacter
    {
        private float SPEED_1 = 8f;
        private float SPEED_2 = 16f;
        private float SPEED_3 = 24f;
        private float SPEED_4 = 32f;
        private float[] _thresholdSpeeds;

        private static Bullet _commonBullet;
        public static Bullet CommonBullet => _commonBullet;
        public Enemy(SpriteSheet spriteSheet) : base(spriteSheet, null) 
        {
            _thresholdSpeeds = new float[4];
            _thresholdSpeeds[0] = SPEED_1;
            _thresholdSpeeds[1] = SPEED_2;
            _thresholdSpeeds[2] = SPEED_3;
            _thresholdSpeeds[3] = SPEED_4;
        }

        public Bullet Fire()
        {
            _commonBullet = base.Fire(Bullet.TargetTypes.Player);

            return _commonBullet;
        }

        public static bool IsEnemyBullet(Bullet bullet)
        {
            return bullet != null && bullet == _commonBullet;
        }

        public static void KillEnemyBullet() 
        { 
            _commonBullet = null; 
        }

        public override void KillBullet()
        {
            base.KillBullet();
            KillEnemyBullet();
        }

        public static bool IsAnyEnemyFiring()
        {
            return _commonBullet != null;
        }

        public void SetThresholdSpeed(int threshold)
        {
            float newSpeed = _thresholdSpeeds[threshold];
            SetSpeed(newSpeed);
            SetAnimationSpeed(newSpeed);
        }
    }
}
