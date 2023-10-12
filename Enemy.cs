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
        private static Bullet _commonBullet;
        public static Bullet CommonBullet => _commonBullet;
        public Enemy(SpriteSheet spriteSheet) : base(spriteSheet, null) { }

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
    }
}
