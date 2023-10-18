using C3.MonoGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace WizardOfWor
{
    public class WizardOfWor : Game
    {
        private const int SCREEN_WIDTH = 160;
        private const int SCREEN_HEIGHT = 114;
        private const int SCREEN_SCALE = 8;
        private const int DISPLAY_OFFSET_X = 4;
        private const int DISPLAY_OFFSET_Y = 7;

        private const int PLAYER_MAX_LIVES = 2;
        private const int BURWOR_AMOUNT = 6;
        private readonly Color PLAYER1_COLOR = new Color(0.863f, 0.690f, 0.286f, 1f);
        private readonly Color BURWOR_COLOR = new Color(0.416f, 0.459f, 0.933f, 1f);
        private readonly Color GARWOR_COLOR = new Color(0.863f, 0.690f, 0.286f, 1f);
        private readonly Color THORWOR_COLOR = new Color(0.694f, 0.157f, 0.153f, 1f);

        private readonly Color LEVEL_DEFAULT_COLOR = new Color(0.286f, 0.318f, 0.820f, 1);
        private readonly Color LEVEL_DOUBLE_SCORE_COLOR = new Color(0.863f, 0.690f, 0.286f, 1f);

        private readonly int BURWOR_SCORE = 100;
        private readonly int GARWOR_SCORE = 200;
        private readonly int THORWOR_SCORE = 500;
        private readonly int WORLUK_SCORE = 1000;
        private readonly int WIZARD_SCORE = 1000;


        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private RenderTarget2D _renderTarget;

        private Level _currentLevel;
        private Level[] _levels;

        private Player _player;
        private SpriteSheet _playerSheet;
        private bool _inCage;
        private int _scoreModifier;
        private bool _applyDoubleScore;

        private List<Enemy> _enemies = new();
        private SpriteSheet _burworSheet;
        private SpriteSheet _thorworSheet;
        private SpriteSheet _worlukSheet;
        private SpriteSheet _wizardSheet;

        private List<Death> _deaths = new();
        private SpriteSheet _enemyDeathSheet;
        private SpriteSheet _playerDeathSheet;

        private SpriteSheet _numbersSheet;

        private readonly List<Bullet> _bullets = new();

        private Random _random;

        private bool _gameStarted = false;
        private int _currentStage = 0;
        private int _levelState;
        private int _garworToSpawn;
        private int _thorworToSpawn;
        private int _killCount;
        private int _enemiesToKill;

        private SoundEffect _playerShootSound;
        private SoundEffect _levelIntroSound;
        private SoundEffect _playerDeathSound;

        private MusicManager _musicManager;

        public WizardOfWor()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = SCREEN_WIDTH * SCREEN_SCALE;
            _graphics.PreferredBackBufferHeight = SCREEN_HEIGHT * SCREEN_SCALE;
            _graphics.ApplyChanges();

            _random = new Random();

            _musicManager = new MusicManager();
        }

        protected override void Initialize()
        {
            _renderTarget = new RenderTarget2D(GraphicsDevice, SCREEN_WIDTH, SCREEN_HEIGHT);
            _scoreModifier = 1;
            _applyDoubleScore = false;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _playerSheet = new SpriteSheet(Content, "player", 8, 8, 4, 4);
            _burworSheet = new SpriteSheet(Content, "burwor", 8, 8, 4, 4);
            _thorworSheet = new SpriteSheet(Content, "thorwor", 8, 8, 4, 4);
            _worlukSheet = new SpriteSheet(Content, "worluk", 8, 8, 4, 4);
            _wizardSheet = new SpriteSheet(Content, "wizardofwor", 8, 8, 4, 4);

            _enemyDeathSheet = new SpriteSheet(Content, "monster-death", 8, 8, 4, 4);
            _playerDeathSheet = new SpriteSheet(Content, "player-death", 8, 8, 4, 4);

            _numbersSheet = new SpriteSheet(Content, "numbers", 14, 7, 0, 0);

            _levels = new Level[2];
            _levels[0] = new Level("Level1.txt", 12, 10, GraphicsDevice, _spriteBatch, _random);
            _levels[1] = new Level("Level2.txt", 12, 10, GraphicsDevice, _spriteBatch, _random);
            _currentLevel = _levels[0];

            _playerShootSound = Content.Load<SoundEffect>("piou-bouche");
            _levelIntroSound = Content.Load<SoundEffect>("intro-bouche");
            _playerDeathSound = Content.Load<SoundEffect>("death-bouche");

            _musicManager.LoadMusicSounds(Content);
        }

        private float _levelStartTimer;
        private bool _levelStarting = false;

        private void InitLevel()
        {
            _currentLevel = _levels[_currentStage % 2];
            _currentLevel.Reset();
            if (_player == null)
            {
                SpawnPlayer();
            }
            else
            {
                _player.ResetLives();
            }
            ToCage();

            _scoreModifier = _applyDoubleScore ? 2 : 1;
            _applyDoubleScore = false;

            _garworToSpawn = _thorworToSpawn = _currentStage + 1;
            _killCount = 0;
            _enemiesToKill = BURWOR_AMOUNT + _garworToSpawn + _thorworToSpawn;
            _levelIntroSound.Play();
            _levelStartTimer = (float)_levelIntroSound.Duration.TotalSeconds;
            _levelStarting = true;
            _levelState = 0;
        }

        private void StartLevel()
        {
            for (int i = 0; i < BURWOR_AMOUNT; i++)
            {
                SpawnEnemy(_burworSheet, BURWOR_COLOR, canBecomeInvisible: false, BURWOR_SCORE);
            }

            _levelStarting = false;
            _gameStarted = true;
            _musicManager.StartMusic(30);
        }

        private void NextLevelPhase()
        {
            switch (_levelState)
            {
                case 0:
                    if (_currentStage > 0)
                    {
                        _levelState++;
                        SpawnWorluk();
                    }
                    else
                    {
                        NextLevel();
                    }
                    break;
                case 1:
                    _levelState++;
                    if (_random.Next(2) == 0)
                        SpawnWizard();
                    else
                        NextLevel();
                    break;
                case 2:
                    NextLevel();
                    break;
            }
        }

        private void NextLevel()
        {
            _currentStage++;
            ClearLevel();
            _musicManager.StopMusic();
            InitLevel();
        }

        private void EndGame()
        {
            ClearLevel();
            _gameStarted = false;
            _currentStage = 0;
            _scoreModifier = 1;
            _player.ResetScore();
            _musicManager.StopMusic();
        }

        private void ClearLevel()
        {
            _currentLevel.Reset();
            _enemies.Clear();
            _bullets.Clear();
            Enemy.KillEnemyBullet();
            _player.KillBullet();
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;


            if (_levelStarting)
            {
                _levelStartTimer -= deltaTime;
                if (_levelStartTimer <= 0)
                {
                    StartLevel();
                }
            }

            SimpleControls.GetStates();

            if (_gameStarted)
            {
                _currentLevel.Update(deltaTime);

                if (_toCageTimer > 0)
                {
                    _toCageTimer -= deltaTime;
                    if (_toCageTimer <= 0)
                    {
                        ToCage();
                    }
                }

                if (SimpleControls.IsEscapeDown())
                    Exit();


                if (_inCage)
                {
                    _inCageTimer += deltaTime;
                    if ((SimpleControls.IsAnyMoveKeyDown() && !_levelStarting) || _inCageTimer >= 10f)
                    {
                        LeaveCage();
                    }
                }
                else
                {
                    if (_player.Visible)
                    {
                        ProcessPlayerInput(deltaTime);
                    }
                }

                UpdateEnemies(deltaTime);

                CheckPlayerDeath();

                UpdateBullets(deltaTime);

                UpdateDeaths(deltaTime);

                _musicManager.Update(deltaTime, _currentLevel.CurrentThreshold);
            }
            else
            {
                if (SimpleControls.IsStartDown() && !_levelStarting)
                {
                    InitLevel();
                }
            }
            base.Update(gameTime);
        }

        private void LeaveCage()
        {
            _inCage = false;
            _player.MoveTo(_currentLevel.GetCellPosition(11, 6));
        }

        private void UpdateDeaths(float deltaTime)
        {
            for (int i = 0; i < _deaths.Count; i++)
            {
                _deaths[i].Update(deltaTime);
                if (!_deaths[i].Enabled)
                {
                    _deaths.RemoveAt(i);
                }
            }
        }

        private void UpdateBullets(float deltaTime)
        {
            _bullets.Clear();
            if (_player.IsFiring())
            {
                _bullets.Add(_player.Bullet);
            }
            if (Enemy.IsAnyEnemyFiring())
            {
                _bullets.Add(Enemy.CommonBullet);
            }

            for (int i = 0; i < _bullets.Count; i++)
            {
                Bullet bullet = _bullets[i];
                bullet.Update(deltaTime);

                // Test whether the bullet hits a wall (non empty pixel)
                if (_currentLevel.HasPixel(bullet.PixelPositionX, bullet.PixelPositionY))
                {
                    if (_player.IsOwnedBullet(bullet))
                    {
                        _player.KillBullet();
                    }
                    else if (Enemy.IsAnyEnemyFiring())
                    {
                        Enemy.KillEnemyBullet();
                    }
                    continue;
                }

                // Test if the player's bullet hits an enemy
                int distanceX, distanceY;
                if (bullet.TargetType == Bullet.TargetTypes.Any)
                {
                    // TODO: include players

                    for (int j = 0; j < _enemies.Count; j++)
                    {
                        Enemy enemy = _enemies[j];
                        distanceX = (int)MathF.Abs(enemy.PixelPositionX - bullet.PixelPositionX + enemy.SpriteSheet.SpritePivot.X);
                        distanceY = (int)MathF.Abs(enemy.PixelPositionY - bullet.PixelPositionY + enemy.SpriteSheet.SpritePivot.Y);

                        if (distanceX <= enemy.SpriteSheet.SpritePivot.X && distanceY <= enemy.SpriteSheet.SpritePivot.Y)
                        {
                            _player.IncreaseScore(enemy.ScorePoints * _scoreModifier);
                            enemy.Die();
                            if (enemy.IsFiring())
                            {
                                enemy.KillBullet();
                            }
                            _enemies.Remove(enemy);
                            bullet.Origin.KillBullet();

                            _deaths.Add(new Death(_enemyDeathSheet, enemy.PixelPositionX, enemy.PixelPositionY, enemy.Color, 0, Vector2.One));

                            _killCount++;
                            UpdateEnemiesSpawn();
                        }
                    }
                }

                // Test if an enemy's bullet hits the player
                if (bullet.TargetType == Bullet.TargetTypes.Player)
                {
                    // TODO: test both players

                    if (_player.Visible)
                    {
                        distanceX = (int)MathF.Abs(_player.PixelPositionX - bullet.PixelPositionX + _player.SpriteSheet.SpritePivot.X);
                        distanceY = (int)MathF.Abs(_player.PixelPositionY - bullet.PixelPositionY + _player.SpriteSheet.SpritePivot.Y);

                        if (distanceX <= _player.SpriteSheet.SpritePivot.X && distanceY <= _player.SpriteSheet.SpritePivot.Y)
                        {
                            KillPlayer(_player);
                            bullet.Origin.KillBullet();
                        }
                    }
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            // TODO: Add your drawing code here
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            Color levelColor = _scoreModifier > 1 ? LEVEL_DOUBLE_SCORE_COLOR : LEVEL_DEFAULT_COLOR;
            _spriteBatch.Draw(_currentLevel.RenderTarget2D, new Rectangle(DISPLAY_OFFSET_X, DISPLAY_OFFSET_Y, _currentLevel.PixelWidth, _currentLevel.PixelHeight), levelColor);
            _currentLevel.DrawTunnels(levelColor, DISPLAY_OFFSET_X, DISPLAY_OFFSET_Y);

            if (_gameStarted)
            {
                if (_player.IsAlive)
                {
                    _player.Draw(_spriteBatch, DISPLAY_OFFSET_X, DISPLAY_OFFSET_Y);
                }

                foreach (Character enemy in _enemies)
                {
                    enemy.Draw(_spriteBatch, DISPLAY_OFFSET_X, DISPLAY_OFFSET_Y);
                }

                foreach (Bullet bullet in _bullets)
                {
                    bullet.Draw(_spriteBatch, DISPLAY_OFFSET_X, DISPLAY_OFFSET_Y);
                }

                foreach (Death death in _deaths)
                {
                    death.Draw(_spriteBatch, DISPLAY_OFFSET_X, DISPLAY_OFFSET_Y);
                }

                DrawRemainingLives(_spriteBatch);
                _currentLevel.DrawRadar(_enemies);
            }
            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            _spriteBatch.Draw(_renderTarget, new Rectangle(0, 0, SCREEN_WIDTH * SCREEN_SCALE, SCREEN_HEIGHT * SCREEN_SCALE), Color.White);
            DrawScore(_player, _spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawScore(Player player, SpriteBatch spriteBatch)
        {
            int score = player != null ? player.CurrentScore : 0;
            Color color = player != null ? player.Color : PLAYER1_COLOR;
            Vector2 position = new Vector2(280 * SCREEN_SCALE / 2, DISPLAY_OFFSET_Y * SCREEN_SCALE);
            Vector2 scale = new Vector2(SCREEN_SCALE / 2, SCREEN_SCALE / 2);
            Vector2 positionDelta = new Vector2(-16 * SCREEN_SCALE / 2, 0);
            for(int i = 0; i<5;i++)
            {
                int number = score % 10;

                _numbersSheet.DrawFrame(number, spriteBatch, position, 0, scale, color);

                score = score / 10;
                position += positionDelta;
            }
        }

        #region Player
        private void SpawnPlayer()
        {
            _player = new Player(_playerSheet, PLAYER_MAX_LIVES, _playerShootSound);
            _player.SetSpeed(20);
            _player.SetAnimationSpeed(10);
            _player.SetColor(PLAYER1_COLOR);
        }

        private void ProcessPlayerInput(float deltaTime)
        {
            bool isMoving = false;
            Level.CanMoveData canMove = _currentLevel.CanMove(_player.PixelPositionX, _player.PixelPositionY, out int tunnel);
            Vector2 lookTo = Vector2.Zero;
            bool isBetweenCells = !_currentLevel.IsOnGridCell(_player.PixelPositionX, _player.PixelPositionY);

            if (isBetweenCells &&
                (SimpleControls.IsLeftDown() && !canMove.Left
                    || SimpleControls.IsRightDown() && !canMove.Right
                    || SimpleControls.IsUpDown() && !canMove.Up
                    || SimpleControls.IsDownDown() && !canMove.Down))
            {
                lookTo = _player.MoveDirection;
                isMoving = true;
            }
            else
            {
                if (SimpleControls.IsLeftDown() && (canMove.Left || tunnel == Level.TUNNEL_LEFT))
                {
                    lookTo.X = -1;
                    if (tunnel == Level.TUNNEL_LEFT)
                    {
                        isMoving = false;
                        _player.LookTo(lookTo);
                        TunnelTeleport(_player, tunnel);
                    }
                    else
                    {
                        isMoving = true;
                    }
                }
                else if (SimpleControls.IsRightDown() && (canMove.Right || tunnel == Level.TUNNEL_RIGHT))
                {
                    lookTo.X = 1;
                    if (tunnel == Level.TUNNEL_RIGHT)
                    {
                        isMoving = false;
                        _player.LookTo(lookTo);
                        TunnelTeleport(_player, tunnel);
                    }
                    else
                    {
                        isMoving = true;
                    }
                }
                else if (SimpleControls.IsDownDown() && canMove.Down)
                {
                    lookTo.Y = 1;
                    isMoving = true;
                }
                else if (SimpleControls.IsUpDown() && canMove.Up)
                {
                    lookTo.Y = -1;
                    isMoving = true;
                }
            }
            if (SimpleControls.IsADown() && !_player.IsFiring())
            {
                _player.Fire(Bullet.TargetTypes.Any);

            }

            if (isMoving)
            {
                _player.LookTo(lookTo);
                _player.Move(deltaTime);
                _player.Animate(deltaTime);
            }
        }

        private void TunnelTeleport(Character character, int tunnel)
        {
            if (tunnel != Level.NO_TUNNEL)
            {
                character.MoveTo(_currentLevel.GetTunnelPosition(3 - tunnel));
            }
        }

        private void CheckPlayerDeath()
        {
            if (_player.Visible)
            {
                for (int i = 0; i < _enemies.Count; i++)
                {
                    Character enemy = _enemies[i];
                    int distanceX = (int)MathF.Abs(enemy.PixelPositionX - _player.PixelPositionX);
                    int distanceY = (int)MathF.Abs(enemy.PixelPositionY - _player.PixelPositionY);
                    if (distanceX <= 2 && distanceY <= 2)
                    {
                        KillPlayer(_player);
                    }
                }
            }
        }

        private void KillPlayer(Player player)
        {
            if (player.IsFiring())
            {
                player.KillBullet();
            }

            if (_player.HasLivesLeft())
            {
                _player.LoseLife();
                _deaths.Add(new Death(_playerDeathSheet, player.PixelPositionX, player.PixelPositionY, Color.White, _player.CurrentRotation, _player.CurrentScale));

                _playerDeathSound.Play();
                if (_levelState == 2)
                {
                    NextLevel();
                }
                else
                {
                    ToCage((float)_playerDeathSound.Duration.TotalSeconds);
                }
            }
            else
            {
                EndGame();
            }

        }

        private float _toCageTimer;
        private float _inCageTimer;
        private void ToCage(float timer)
        {
            _toCageTimer = timer;
            _player.Visible = false;
        }

        private void ToCage()
        {
            _player.MoveTo(_currentLevel.GetCellPosition(11, 7));
            _player.LookTo(new Vector2(-1, 0));
            _player.SetFrame(1);
            _player.Visible = true;
            _inCage = true;
            _inCageTimer = 0;
        }

        private void DrawRemainingLives(SpriteBatch spriteBatch)
        {
            int offset = _inCage ? 1 : 0;
            int remainingLives = _player.RemainingLives;
            if (_toCageTimer > 0)
            {
                remainingLives++;
            }
            Vector2 startPosition = _currentLevel.GetCellPosition(11, 7);
            for (int i = 0; i < MathF.Min(2 + 1 - offset, remainingLives); i++)
            {
                _playerSheet.DrawFrame(1, spriteBatch, startPosition + new Vector2(-1, 0) * (i + offset) * 16 + new Vector2(DISPLAY_OFFSET_X, DISPLAY_OFFSET_Y), 0, new Vector2(-1, 1), _player.Color);
            }
        }
        #endregion

        #region Enemies

        private void SpawnEnemy(SpriteSheet enemySheet, Color color, bool canBecomeInvisible, int score)
        {
            Enemy enemy = new Enemy(enemySheet, color, canBecomeInvisible, score);

            enemy.MoveTo(_currentLevel.GetRandomPosition());
            enemy.LookTo(_currentLevel.PickPossibleDirection(enemy, out int _));

            _enemies.Add(enemy);
        }

        private void SpawnWorluk()
        {
            Worluk enemy = new Worluk(_worlukSheet, GARWOR_COLOR, _random.Next(0, 2) * 2 - 1, WORLUK_SCORE);

            enemy.MoveTo(_currentLevel.GetRandomPosition());
            enemy.LookTo(_currentLevel.PickPossibleDirection(enemy, out int _));

            _enemies.Add(enemy);
        }

        private void SpawnWizard()
        {
            Wizard enemy = new Wizard(_wizardSheet, _currentLevel, WIZARD_SCORE);

            enemy.MoveTo(_currentLevel.GetRandomPosition());
            enemy.LookTo(_currentLevel.PickPossibleDirection(enemy, out int _));

            _enemies.Add(enemy);
        }

        private void UpdateEnemies(float deltaTime)
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                Enemy enemy = _enemies[i];
                enemy.Update(deltaTime);

                enemy.SetThresholdSpeed(_currentLevel.CurrentThreshold);
                Vector2 newMoveDirection;
                int tunnel = Level.NO_TUNNEL;
                if (enemy is Wizard)
                {
                    enemy.CanChangeDirection = true;
                    newMoveDirection = enemy.MoveDirection;
                }
                else
                {
                    newMoveDirection = _currentLevel.PickPossibleDirection(enemy, out tunnel);
                    enemy.LookTo(newMoveDirection);
                }

                if (newMoveDirection.X > 0 && tunnel == Level.TUNNEL_RIGHT
                    || newMoveDirection.X < 0 && tunnel == Level.TUNNEL_LEFT)
                {
                    if (enemy is Worluk)
                    {
                        enemy.Die();
                        _enemies.Remove(enemy);
                        //NextLevelPhase();
                        NextLevel();
                    }
                    else
                    {
                        TunnelTeleport(enemy, tunnel);
                    }
                }
                else
                {
                    enemy.Move(deltaTime);
                }
                enemy.Animate(deltaTime);

                // Test for player in line of sight
                if (!_inCage && enemy.CanFireAtPlayer(_player))
                {
                    enemy.Fire();
                }

                if (!enemy.Visible && (enemy.PixelPositionY == _player.PixelPositionY || enemy.PixelPositionX == _player.PixelPositionX))
                {
                    enemy.Visible = true;
                }
            }
        }

        #endregion

        #region Level
        private void UpdateEnemiesSpawn()
        {
            if (_levelState > 0)
            {
                _applyDoubleScore = true;
                NextLevelPhase();
            }
            else
            {
                if (_killCount == _enemiesToKill)
                {
                    // end of level or spawn worluk
                    NextLevelPhase();
                }
                else
                {
                    if (_killCount >= 6 - _currentStage)
                    {
                        if (_garworToSpawn >= _thorworToSpawn)
                        {
                            if (_garworToSpawn > 0)
                            {
                                // spawn a garwor
                                SpawnEnemy(_burworSheet, GARWOR_COLOR, canBecomeInvisible: true, GARWOR_SCORE);
                                _garworToSpawn--;
                            }
                        }
                        else
                        {
                            if (_thorworToSpawn > 0)
                            {
                                // spawn a thorwor
                                SpawnEnemy(_thorworSheet, THORWOR_COLOR, canBecomeInvisible: true, THORWOR_SCORE);
                                _thorworToSpawn--;
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}