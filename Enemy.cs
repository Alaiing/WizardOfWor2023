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
        protected float SPEED_1 = 5f;
        protected float SPEED_2 = 10f;
        protected float SPEED_3 = 15f;
        protected float SPEED_4 = 25f;
        protected float SPEED_5 = 40f;

        private float INVISIBILITY_TIMER = 5f;
        private bool _canBecomeInvisible;
        private float _visibilityTimer;

        private float[] _thresholdSpeeds;

        protected int _preferredHorizontalDirection;
        public int PreferredHorizontalDirection => _preferredHorizontalDirection;

        private static Bullet _commonBullet;
        public static Bullet CommonBullet => _commonBullet;
        public Enemy(SpriteSheet spriteSheet, Color color, bool canBecomeInvisible) : base(spriteSheet, null)
        {
            _thresholdSpeeds = new float[5];
            _thresholdSpeeds[0] = SPEED_1;
            _thresholdSpeeds[1] = SPEED_2;
            _thresholdSpeeds[2] = SPEED_3;
            _thresholdSpeeds[3] = SPEED_4;
            _thresholdSpeeds[4] = SPEED_5;
            _canBecomeInvisible = canBecomeInvisible;
            Visible = !_canBecomeInvisible;
            SetSpeed(SPEED_1);
            SetAnimationSpeed(SPEED_1);
            SetColor(color);
            _preferredHorizontalDirection = 0;
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

        public void Update(float deltaTime)
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
            return player.Visible 
                && !IsAnyEnemyFiring() 
                && (MoveDirection.X != 0 && PixelPositionY == player.PixelPositionY && MathF.Sign(MoveDirection.X) == MathF.Sign(player.PixelPositionX - PixelPositionX)
    || MoveDirection.Y != 0 && PixelPositionX == player.PixelPositionX && MathF.Sign(MoveDirection.Y) == MathF.Sign(player.PixelPositionY - PixelPositionY));
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

        public virtual void SetThresholdSpeed(int threshold)
        {
            float newSpeed = _thresholdSpeeds[threshold];
            SetSpeed(newSpeed);
            SetAnimationSpeed(newSpeed);
        }
    }
}
