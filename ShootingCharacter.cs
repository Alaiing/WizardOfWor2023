using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardOfWor
{
    public class ShootingCharacter : Character
    {
        private Bullet _bullet;
        public Bullet Bullet => _bullet;
        private readonly SoundEffect _shootSound;
        private SoundEffectInstance _currentShootSound;

        public ShootingCharacter(SpriteSheet spriteSheet, SoundEffect shootSound = null) : base(spriteSheet) 
        {
            _shootSound = shootSound;
        }

        public virtual Bullet Fire(Bullet.TargetTypes targetType)
        {
            _bullet = new Bullet(this, targetType);
            if (_shootSound != null)
            {
                _currentShootSound = _shootSound.CreateInstance();
                _currentShootSound.Play();
            }

            return _bullet;
        }

        public virtual void KillBullet()
        {
            _bullet = null;
            if (_currentShootSound != null)
            {
                _currentShootSound.Stop(immediate: true);
                _currentShootSound.Dispose();
                _currentShootSound = null;
            }
        }

        public virtual bool IsOwnedBullet(Bullet bullet)
        {
            return bullet != null && bullet == _bullet;
        }

        public virtual bool IsFiring()
        {
            return _bullet != null;
        }
    }
}
