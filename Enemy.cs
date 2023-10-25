using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardOfWor
{
    public class Enemy : ShootingCharacter
    {
        private float INVISIBILITY_TIMER = 5f;
        private bool _canBecomeInvisible;
        private float _visibilityTimer;

        private float[] _thresholdSpeeds;

        protected int _preferredHorizontalDirection;
        public int PreferredHorizontalDirection => _preferredHorizontalDirection;

        protected int _scorePoints;
        public int ScorePoints => _scorePoints;

        private static Bullet _commonBullet;
        public static Bullet CommonBullet => _commonBullet;
        public Enemy(SpriteSheet spriteSheet, Color color, bool canBecomeInvisible, int scorePoints) : base(spriteSheet, null)
        {
            _thresholdSpeeds = new float[5];
            _thresholdSpeeds[0] = ConfigManager.GetConfig(Constants.ENEMY_SPEED_1, Constants.DEFAULT_ENEMY_SPEED_1);
            _thresholdSpeeds[1] = ConfigManager.GetConfig(Constants.ENEMY_SPEED_2, Constants.DEFAULT_ENEMY_SPEED_2);
            _thresholdSpeeds[2] = ConfigManager.GetConfig(Constants.ENEMY_SPEED_3, Constants.DEFAULT_ENEMY_SPEED_3);
            _thresholdSpeeds[3] = ConfigManager.GetConfig(Constants.ENEMY_SPEED_4, Constants.DEFAULT_ENEMY_SPEED_4);
            _thresholdSpeeds[4] = ConfigManager.GetConfig(Constants.ENEMY_SPEED_5, Constants.DEFAULT_ENEMY_SPEED_5);
            _canBecomeInvisible = canBecomeInvisible;
            Visible = !_canBecomeInvisible;
            SetSpeed(_thresholdSpeeds[0]);
            SetAnimationSpeed(_thresholdSpeeds[0]);
            SetColor(color);
            _preferredHorizontalDirection = 0;
            _scorePoints = scorePoints;
        }

        public override bool Visible
        {
            get => base.Visible;
            set
            {
                if (value)
                {
                    _visibilityTimer = 0;
                }
                base.Visible = value;
            }
        }

        public virtual void Update(float deltaTime)
        {
            if (_canBecomeInvisible && Visible)
            {
                _visibilityTimer += deltaTime;
                if (_visibilityTimer >= INVISIBILITY_TIMER) 
                { 
                    Visible = false;
                }
            }
        }

        public virtual bool CanFireAtPlayer(Player player)
        {
            return player!= null && !player.InCage && player.Visible 
                && !IsAnyEnemyFiring() 
                && (MoveDirection.X != 0 && PixelPositionY == player.PixelPositionY && MathF.Sign(MoveDirection.X) == MathF.Sign(player.PixelPositionX - PixelPositionX)
    || MoveDirection.Y != 0 && PixelPositionX == player.PixelPositionX && MathF.Sign(MoveDirection.Y) == MathF.Sign(player.PixelPositionY - PixelPositionY));
        }

        public Bullet Fire()
        {
            _commonBullet = base.Fire(Bullet.TargetTypes.Player, ConfigManager.GetConfig(Constants.ENEMY_BULLET_SPEED, Constants.DEFAULT_ENEMY_BULLET_SPEED));

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

        public virtual void SetThresholdSpeed(int threshold, float modificator)
        {
            float newSpeed = _thresholdSpeeds[threshold] * modificator;
            SetSpeed(newSpeed);
            SetAnimationSpeed(newSpeed);
        }

        public void UpdateVisible(Player player)
        {
            if (player!= null & !Visible && (PixelPositionY == player.PixelPositionY || PixelPositionX == player.PixelPositionX))
            {
                Visible = true;
            }
        }
    }
}
