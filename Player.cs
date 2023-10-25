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
        private readonly int _maxLives;
        private int _remainingLives;
        public int RemainingLives => _remainingLives;
        private int _currentScore;
        public int CurrentScore => _currentScore;
        public bool InCage;
        public float TimeInCage;
        public float TimeToCage;

        private int _cagePositionX;
        public int CagePositionX => _cagePositionX;
        private int _cagePositionY;
        public int CagePositionY => _cagePositionY;

        private SimpleControls.PlayerNumber _playerNumber;
        public SimpleControls.PlayerNumber PlayerNumber => _playerNumber;

        private int _extraLifeScore;

        public Player(SpriteSheet spriteSheet, int maxLives, SoundEffect shootSound, int cageX, int cageY, SimpleControls.PlayerNumber playerNumber) : base(spriteSheet, shootSound)
        {
            _maxLives = maxLives;
            _remainingLives = maxLives;
            _cagePositionX = cageX;
            _cagePositionY = cageY;
            _playerNumber = playerNumber;
            _extraLifeScore = ConfigManager.GetConfig(Constants.EXTRA_LIFE_SCORE, Constants.DEFAULT_EXTRA_LIFE_SCORE);
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
            return _remainingLives >= 0;
        }

        public Bullet Fire()
        {
            return base.Fire(Bullet.TargetTypes.Any, ConfigManager.GetConfig(Constants.PLAYER_BULLET_SPEED, Constants.DEFAULT_PLAYER_BULLET_SPEED));
        }

        public void IncreaseScore(int score)
        {
            if (_currentScore < _extraLifeScore && _currentScore + score >= _extraLifeScore)
                GainLife();
            _currentScore += score;
        }

        public void ResetScore() 
        { 
            _currentScore = 0; 
        }
    }
}
