using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardOfWor
{
    public class Player : ShootingCharacter
    {
        private readonly int _maxLives;
        private int _remainingLives;
        public int RemainingLives => _remainingLives;

        public Player(SpriteSheet spriteSheet, int maxLives, SoundEffect shootSound = null) : base(spriteSheet, shootSound)
        {
            _maxLives = maxLives;
            _remainingLives = maxLives;
        }

        public void ResetLives()
        {
            _remainingLives = _maxLives;
        }

        public void LoseLife()
        {
            _remainingLives--;
        }

        public void GainLife()
        {
            _remainingLives++;
        }

        public bool HasLivesLeft()
        {
            return _remainingLives > 0;
        }

        public Bullet Fire()
        {
            return base.Fire(Bullet.TargetTypes.Any);
        }
    }
}
