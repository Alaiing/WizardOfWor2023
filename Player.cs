using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardOfWor
{
    public class Player : ShootingCharacter
    {
        private const int EXTRA_LIFE_SCORE = 10000;
        private readonly int _maxLives;
        private int _remainingLives;
        public int RemainingLives => _remainingLives;
        private int _currentScore;
        public int CurrentScore => _currentScore;
        public bool InCage { get; set; }

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

        public void IncreaseScore(int score)
        {
            if (_currentScore < EXTRA_LIFE_SCORE && _currentScore + score >= EXTRA_LIFE_SCORE)
                GainLife();
            _currentScore += score;
            Debug.WriteLine($"Player score: {_currentScore}");
        }

        public void ResetScore() 
        { 
            _currentScore = 0; 
        }
    }
}
