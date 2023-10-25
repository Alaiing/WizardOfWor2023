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

        private readonly Color PLAYER1_COLOR = new Color(0.863f, 0.690f, 0.286f, 1f);
        private readonly Color PLAYER2_COLOR = new Color(0.416f, 0.459f, 0.933f, 1f);
        private readonly Color BURWOR_COLOR = new Color(0.416f, 0.459f, 0.933f, 1f);
        private readonly Color GARWOR_COLOR = new Color(0.863f, 0.690f, 0.286f, 1f);
        private readonly Color THORWOR_COLOR = new Color(0.694f, 0.157f, 0.153f, 1f);

        private readonly Color LEVEL_DEFAULT_COLOR = new Color(0.286f, 0.318f, 0.820f, 1);
        private readonly Color LEVEL_DOUBLE_SCORE_COLOR = new Color(0.863f, 0.690f, 0.286f, 1f);
        private readonly Color LEVEL_WORLUK_COLOR = new Color(0.690f, 0.153f, 0.149f, 1f);

        private const float WORLUK_DEATH_DURATION = 5f;
        private const float WORLUK_DEATH_COLOR_DURATION = 0.25f;
        private const float WIZARD_DEATH_COLOR_DURATION = 0.5f;
        private readonly Color[] WORLUK_DEATH_COLOR = { Color.Pink, Color.Cyan, Color.Yellow, Color.Purple, Color.Green };

        private const int LEVEL_KILL_ENEMIES = 0;
        private const int LEVEL_WORLUK = 1;
        private const int LEVEL_WIZARD = 2;
        private const int LEVEL_WORLUK_DEATH = 3;
        private const int LEVEL_WIZARD_DEATH = 4;
        private const int LEVEL_WORLUK_ESCAPE = 5;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private RenderTarget2D _renderTarget;

        private Level _currentLevel;
        private Level[] _levels;

        private Player _player1;
        private Player _player2;
        private bool IsMultiPlayer => _player2 != null;

        private SpriteSheet _playerSheet;
        private int _scoreModifier;
        private bool _applyDoubleScore;

        private readonly List<Enemy> _enemies = new();
        private SpriteSheet _burworSheet;
        private SpriteSheet _thorworSheet;
        private SpriteSheet _worlukSheet;
        private SpriteSheet _wizardSheet;

        private readonly List<Death> _deaths = new();
        private SpriteSheet _enemyDeathSheet;
        private SpriteSheet _playerDeathSheet;

        private SpriteSheet _numbersSheet;

        private readonly List<Bullet> _bullets = new();

        private readonly Random _random;

        private bool _gameStarted = false;
        private int _currentStage = 0;
        private int _levelState;
        private Color _levelColor;
        private Color _levelBackgroundColor;
        private float _levelStateTimer;
        private int _garworToSpawn;
        private int _thorworToSpawn;
        private int _killCount;
        private int _enemiesToKill;

        private SoundEffect _playerShootSound;
        private SoundEffect _levelIntroSound;
        private SoundEffect _playerDeathSound;
        private SoundEffect _worlukEscapeSound;
        private SoundEffect _worlukDeathSound;
        private SoundEffect _wizardDeathSound;

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
            ConfigManager.LoadConfig("config.ini");

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
            _levelColor = LEVEL_DEFAULT_COLOR;

            _playerShootSound = Content.Load<SoundEffect>("piou");
            _levelIntroSound = Content.Load<SoundEffect>("intro");
            _playerDeathSound = Content.Load<SoundEffect>("death");
            _worlukEscapeSound = Content.Load<SoundEffect>("worluk-escape");
            _worlukDeathSound = Content.Load<SoundEffect>("worluk-kill");
            _wizardDeathSound = Content.Load<SoundEffect>("wizard-kill");

            CameraShake.Enabled = ConfigManager.GetConfig(Constants.CAMERA_SHAKE, Constants.DEFAULT_CAMERA_SHAKE);

            _musicManager.LoadMusicSounds(Content);

            _player1 = SpawnPlayer(SimpleControls.PlayerNumber.Player1, PLAYER1_COLOR, 11, 7);
        }

        private float _levelStartTimer;
        private bool _levelStarting = false;

        private void StartGame(bool multiplayer)
        {
            _player1 = SpawnPlayer(SimpleControls.PlayerNumber.Player1, PLAYER1_COLOR, 11, 7);
            if (multiplayer)
            {
                _player2 = SpawnPlayer(SimpleControls.PlayerNumber.Player2, PLAYER2_COLOR, 1, 7);
            }
            else
            {
                _player2 = null;
            }
            _currentStage = 0;
            _applyDoubleScore = false;
            _levelStarting = false;
            InitLevel();
        }

        private void InitLevel()
        {
            _currentLevel = _levels[_currentStage % 2];
            _levelState = 0;
            _currentLevel.Reset(_currentStage);
            ToCage(_player1);
            ToCage(_player2);

            _garworToSpawn = _currentStage * ConfigManager.GetConfig(Constants.GARWORS_PER_LEVEL, Constants.DEFAULT_GARWORS_PER_LEVEL) + 1;
            _thorworToSpawn = _currentStage * ConfigManager.GetConfig(Constants.THORWORS_PER_LEVEL, Constants.DEFAULT_THORWORS_PER_LEVEL) + 1;
            _killCount = 0;
            _enemiesToKill = ConfigManager.GetConfig(Constants.BURWARS, Constants.DEFAULT_BURWARS) + _garworToSpawn + _thorworToSpawn;
            _levelIntroSound.Play();
            _levelStartTimer = (float)_levelIntroSound.Duration.TotalSeconds;
            _levelStarting = true;
            _levelBackgroundColor = Color.Black;

            if (_applyDoubleScore)
            {
                _scoreModifier = 2;
                _levelColor = LEVEL_DOUBLE_SCORE_COLOR;
            }
            else
            {
                _scoreModifier = 1;
                _levelColor = LEVEL_DEFAULT_COLOR;
            }
            _applyDoubleScore = false;
        }

        private void StartLevel()
        {
            for (int i = 0; i < ConfigManager.GetConfig(Constants.BURWARS, Constants.DEFAULT_BURWARS); i++)
            {
                SpawnEnemy(_burworSheet, BURWOR_COLOR, canBecomeInvisible: false, ConfigManager.GetConfig(Constants.BURWOR_SCORE, Constants.DEFAULT_BURWOR_SCORE));
            }

            _levelStarting = false;
            _gameStarted = true;
            _musicManager.StartMusic(30);
        }

        private void NextLevelPhase()
        {
            switch (_levelState)
            {
                case LEVEL_KILL_ENEMIES:
                    if (_currentStage > 0)
                    {
                        _levelState = LEVEL_WORLUK;
                        SpawnWorluk();
                        _musicManager.StopMusic();
                        _musicManager.StartBossMusic();

                        _levelColor = LEVEL_WORLUK_COLOR;
                    }
                    else
                    {
                        NextLevel();
                    }
                    break;
                case LEVEL_WORLUK:
                    if (_random.Next(2) == 0)
                    {
                        _levelState = LEVEL_WIZARD;
                        SpawnWizard();
                    }
                    else
                    {
                        _levelState = LEVEL_WORLUK_DEATH;
                        _musicManager.StopMusic();
                        _worlukDeathSound.Play();
                        CameraShake.Shake(4, 50, (float)_worlukDeathSound.Duration.TotalSeconds);
                        _levelStateTimer = 0;
                        KillPlayerBullets();
                        Enemy.KillEnemyBullet();
                    }
                    break;

                case LEVEL_WIZARD:
                    _levelState = LEVEL_WIZARD_DEATH;
                    _musicManager.StopMusic();
                    _wizardDeathSound.Play();
                    CameraShake.Shake(4, 50, (float)_wizardDeathSound.Duration.TotalSeconds);
                    _levelStateTimer = 0;
                    KillPlayerBullets();
                    Enemy.KillEnemyBullet();
                    break;

                case LEVEL_WORLUK_DEATH:
                    NextLevel();
                    break;

                case LEVEL_WIZARD_DEATH:
                    NextLevel();
                    break;
            }
        }

        private void WorlukEscape()
        {
            _musicManager.StopMusic();
            _worlukEscapeSound.Play();
            _levelStateTimer = 0;
            _levelState = LEVEL_WORLUK_ESCAPE;
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
            _musicManager.StopMusic();
        }

        private void ClearLevel()
        {
            _currentLevel.Reset(_currentStage);
            _enemies.Clear();
            _bullets.Clear();
            KillPlayerBullets();
            Enemy.KillEnemyBullet();
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            CameraShake.Update(deltaTime);

            switch (_levelState)
            {
                case LEVEL_WORLUK_DEATH:
                    _levelStateTimer += deltaTime;
                    int colorIndex = ((int)Math.Floor(_levelStateTimer / WORLUK_DEATH_COLOR_DURATION)) % WORLUK_DEATH_COLOR.Length;
                    Color backgroundColor = WORLUK_DEATH_COLOR[colorIndex];
                    float colorAlpha = _levelStateTimer / WORLUK_DEATH_COLOR_DURATION - MathF.Floor(_levelStateTimer / WORLUK_DEATH_COLOR_DURATION);
                    backgroundColor.R = (byte)(backgroundColor.R * colorAlpha);
                    backgroundColor.G = (byte)(backgroundColor.G * colorAlpha);
                    backgroundColor.B = (byte)(backgroundColor.B * colorAlpha);
                    _levelBackgroundColor = backgroundColor;
                    if (_levelStateTimer > _worlukDeathSound.Duration.TotalSeconds)
                    {
                        NextLevelPhase();
                    }
                    break;

                case LEVEL_WIZARD_DEATH:
                    _levelStateTimer += deltaTime;
                    float colorValue = _levelStateTimer / WIZARD_DEATH_COLOR_DURATION - MathF.Floor(_levelStateTimer / WIZARD_DEATH_COLOR_DURATION);
                    _levelColor = new Color(colorValue, colorValue, colorValue, 1f);
                    if (_levelStateTimer > _wizardDeathSound.Duration.TotalSeconds)
                    {
                        NextLevelPhase();
                    }
                    break;

                case LEVEL_WORLUK_ESCAPE:
                    _levelStateTimer += deltaTime;
                    if (_levelStateTimer > _worlukEscapeSound.Duration.TotalSeconds)
                    {
                        NextLevel();
                    }
                    break;

                default:
                    if (_levelStarting)
                    {
                        _levelStartTimer -= deltaTime;
                        if (_levelStartTimer <= 0)
                        {
                            StartLevel();
                        }
                    }
                    else
                    {
                        if (SimpleControls.IsStartDown())
                        {
                            if (_gameStarted)
                            {
                                EndGame();
                            }
                            StartGame(multiplayer: false);
                        }
                        else if (SimpleControls.IsSelectDown())
                        {
                            if (_gameStarted)
                            {
                                EndGame();
                            }
                            StartGame(multiplayer: true);
                        }
                    }


                    SimpleControls.GetStates();

                    if (_gameStarted)
                    {
                        _currentLevel.Update(deltaTime);

                        UpdatePlayerToCage(_player1, deltaTime);
                        UpdatePlayerToCage(_player2, deltaTime);

                        if (SimpleControls.IsEscapeDown())
                            Exit();

                        if (SimpleControls.IsCheatKillDown())
                        {
                            if (_enemies.Count > 0)
                            {
                                KillEnemy(_enemies[0]);
                                _killCount++;
                            }
                        }

                        UpdatePlayerInCage(_player1, deltaTime);
                        UpdatePlayerInCage(_player2, deltaTime);

                        UpdateEnemies(deltaTime);

                        CheckPlayerDeath(_player1);
                        CheckPlayerDeath(_player2);

                        UpdateBullets(deltaTime);

                        _musicManager.Update(deltaTime, _currentLevel.CurrentThreshold);
                    }
                    break;
            }

            UpdateDeaths(deltaTime);

            base.Update(gameTime);
        }

        private void UpdatePlayerToCage(Player player, float deltaTime)
        {

            if (player != null && player.TimeToCage > 0)
            {
                player.TimeToCage -= deltaTime;
                if (player.TimeToCage <= 0)
                {
                    ToCage(player);
                }
            }
        }

        private void UpdatePlayerInCage(Player player, float deltaTime)
        {
            if (player != null)
            {
                if (player.InCage)
                {
                    player.TimeInCage += deltaTime;
                    if ((SimpleControls.IsAnyMoveKeyDown(player.PlayerNumber) && !_levelStarting) || player.TimeInCage >= ConfigManager.GetConfig(Constants.PLAYER_TIME_IN_CAGE, Constants.DEFAULT_PLAYER_TIME_IN_CAGE))
                    {
                        LeaveCage(player);
                    }
                }
                else
                {
                    if (player.Visible)
                    {
                        ProcessPlayerInput(player, deltaTime);
                    }
                }
            }
        }

        private void LeaveCage(Player player)
        {
            player.InCage = false;
            player.MoveTo(_currentLevel.GetCellPosition(player.CagePositionX, player.CagePositionY - 1));
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
            if (_player1.IsFiring())
            {
                _bullets.Add(_player1.Bullet);
            }
            if (IsMultiPlayer && _player2.IsFiring())
            {
                _bullets.Add(_player2.Bullet);
            }

            if (Enemy.IsAnyEnemyFiring())
            {
                _bullets.Add(Enemy.CommonBullet);
            }

            for (int i = 0; i < _bullets.Count; i++)
            {
                Bullet bullet = _bullets[i];
                bullet.Update(deltaTime);

                if (!_currentLevel.IsInsideWalls(bullet.PixelPositionX, bullet.PixelPositionY)
                    || _currentLevel.HasPixel(bullet.PixelPositionX, bullet.PixelPositionY)) // hits a wall
                {
                    bullet.Origin.KillBullet();
                    continue;
                }

                // Test if the player's bullet hits an enemy
                if (bullet.TargetType == Bullet.TargetTypes.Any)
                {
                    for (int j = 0; j < _enemies.Count; j++)
                    {
                        Enemy enemy = _enemies[j];

                        if (bullet.TestHit(enemy))
                        {
                            if (bullet.Origin is Player player)
                            {
                                player.IncreaseScore(enemy.ScorePoints * _scoreModifier);
                                player.KillBullet();
                            }
                            KillEnemy(enemy);

                            _deaths.Add(new Death(_enemyDeathSheet, enemy.PixelPositionX, enemy.PixelPositionY, enemy.Color, 0, Vector2.One));

                            _killCount++;
                            UpdateEnemiesSpawn();
                        }
                    }
                }

                TestBulletKillsPlayer(bullet, _player1);
                TestBulletKillsPlayer(bullet, _player2);
            }
        }

        private bool TestBulletKillsPlayer(Bullet bullet, Player player)
        {
            if (player != null && player.Visible && bullet.Origin != player)
            {
                if (bullet.TestHit(player))
                {
                    KillPlayer(player);
                    if (bullet.Origin is Player otherPlayer)
                    {
                        otherPlayer.IncreaseScore(ConfigManager.GetConfig(Constants.OTHER_PLAYER_SCORE, Constants.DEFAULT_OTHER_PLAYER_SCORE));
                    }
                    bullet.Origin.KillBullet();
                    return true;
                }
            }

            return false;
        }

        private void KillEnemy(Enemy enemy)
        {
            enemy.Die();
            if (enemy.IsFiring())
            {
                enemy.KillBullet();
            }
            _enemies.Remove(enemy);
        }

        protected override void Draw(GameTime gameTime)
        {
            // TODO: Add your drawing code here
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(_levelBackgroundColor);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _spriteBatch.Draw(_currentLevel.RenderTarget2D, new Rectangle(DISPLAY_OFFSET_X, DISPLAY_OFFSET_Y, _currentLevel.PixelWidth, _currentLevel.PixelHeight), _levelColor);
            _currentLevel.DrawTunnels(_levelColor, DISPLAY_OFFSET_X, DISPLAY_OFFSET_Y);

            if (_gameStarted)
            {
                if (_player1.IsAlive)
                {
                    _player1.Draw(_spriteBatch, DISPLAY_OFFSET_X, DISPLAY_OFFSET_Y);
                }
                if (IsMultiPlayer && _player2.IsAlive)
                {
                    _player2.Draw(_spriteBatch, DISPLAY_OFFSET_X, DISPLAY_OFFSET_Y);
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

                DrawRemainingLives(_player1, _spriteBatch);
                DrawRemainingLives(_player2, _spriteBatch);
                _currentLevel.DrawRadar(_enemies);
            }
            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            _spriteBatch.Draw(_renderTarget, new Rectangle((int)MathF.Floor(CameraShake.ShakeOffset.X), (int)MathF.Floor(CameraShake.ShakeOffset.Y), SCREEN_WIDTH * SCREEN_SCALE, SCREEN_HEIGHT * SCREEN_SCALE), Color.White);
            DrawScore(_spriteBatch, _player1, 280);
            DrawScore(_spriteBatch, _player2, 89);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawScore(SpriteBatch spriteBatch, Player player, int positionX)
        {
            if (player != null)
            {
                int score = player != null ? player.CurrentScore : 0;
                Color color = player != null ? player.Color : PLAYER1_COLOR;
                Vector2 position = new Vector2(positionX * SCREEN_SCALE / 2, DISPLAY_OFFSET_Y * SCREEN_SCALE);
                Vector2 scale = new Vector2(SCREEN_SCALE / 2, SCREEN_SCALE / 2);
                Vector2 positionDelta = new Vector2(-16 * SCREEN_SCALE / 2, 0);
                for (int i = 0; i < 5; i++)
                {
                    int number = score % 10;

                    _numbersSheet.DrawFrame(number, spriteBatch, position, 0, scale, color);

                    score = score / 10;
                    position += positionDelta;
                }
            }
        }

        #region Player
        private Player SpawnPlayer(SimpleControls.PlayerNumber playerNumber, Color color, int cageX, int cageY)
        {
            int maxlives = ConfigManager.GetConfig(Constants.PLAYER_MAX_LIVES, Constants.DEFAULT_PLAYER_MAX_LIVES);
            Player player = new Player(_playerSheet, maxlives, _playerShootSound, cageX, cageY, playerNumber);
            player.SetSpeed(ConfigManager.GetConfig(Constants.PLAYER_SPEED, Constants.DEFAULT_PLAYER_SPEED));
            player.SetAnimationSpeed(ConfigManager.GetConfig(Constants.PLAYER_ANIMATION_SPEED, Constants.DEFAULT_PLAYER_ANIMATION_SPEED));
            player.SetColor(color);

            return player;
        }

        private void ProcessPlayerInput(Player player, float deltaTime)
        {
            bool isMoving = false;
            Level.CanMoveData canMove = _currentLevel.CanMove(player.PixelPositionX, player.PixelPositionY, out int tunnel);
            Vector2 lookTo = Vector2.Zero;
            bool isBetweenCells = !_currentLevel.IsOnGridCell(player.PixelPositionX, player.PixelPositionY);

            if (isBetweenCells &&
                (SimpleControls.IsLeftDown(player.PlayerNumber) && !canMove.Left
                    || SimpleControls.IsRightDown(player.PlayerNumber) && !canMove.Right
                    || SimpleControls.IsUpDown(player.PlayerNumber) && !canMove.Up
                    || SimpleControls.IsDownDown(player.PlayerNumber) && !canMove.Down))
            {
                lookTo = player.MoveDirection;
                isMoving = true;
            }
            else
            {
                if (SimpleControls.IsLeftDown(player.PlayerNumber) && (canMove.Left || tunnel == Level.TUNNEL_LEFT))
                {
                    lookTo.X = -1;
                    if (tunnel == Level.TUNNEL_LEFT)
                    {
                        isMoving = false;
                        player.LookTo(lookTo);
                        TunnelTeleport(player, tunnel);
                    }
                    else
                    {
                        isMoving = true;
                    }
                }
                else if (SimpleControls.IsRightDown(player.PlayerNumber) && (canMove.Right || tunnel == Level.TUNNEL_RIGHT))
                {
                    lookTo.X = 1;
                    if (tunnel == Level.TUNNEL_RIGHT)
                    {
                        isMoving = false;
                        player.LookTo(lookTo);
                        TunnelTeleport(player, tunnel);
                    }
                    else
                    {
                        isMoving = true;
                    }
                }
                else if (SimpleControls.IsDownDown(player.PlayerNumber) && canMove.Down)
                {
                    lookTo.Y = 1;
                    isMoving = true;
                }
                else if (SimpleControls.IsUpDown(player.PlayerNumber) && canMove.Up)
                {
                    lookTo.Y = -1;
                    isMoving = true;
                }
            }
            if (SimpleControls.IsADown(player.PlayerNumber) && !player.IsFiring())
            {
                player.Fire();
                CameraShake.Shake(2, 50, 0.1f);
            }

            if (isMoving)
            {
                player.LookTo(lookTo);
                player.Move(deltaTime);
                player.Animate(deltaTime);
            }
        }

        private void TunnelTeleport(Character character, int tunnel)
        {
            if (tunnel != Level.NO_TUNNEL)
            {
                character.MoveTo(_currentLevel.GetTunnelPosition(3 - tunnel));
            }
        }

        private void CheckPlayerDeath(Player player)
        {
            if (player != null && player.Visible)
            {
                for (int i = 0; i < _enemies.Count; i++)
                {
                    Character enemy = _enemies[i];
                    int distanceX = (int)MathF.Abs(enemy.PixelPositionX - player.PixelPositionX);
                    int distanceY = (int)MathF.Abs(enemy.PixelPositionY - player.PixelPositionY);
                    if (distanceX <= 2 && distanceY <= 2)
                    {
                        KillPlayer(player);
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

            player.LoseLife();
            _deaths.Add(new Death(_playerDeathSheet, player.PixelPositionX, player.PixelPositionY, Color.White, player.CurrentRotation, player.CurrentScale));

            _playerDeathSound.Play();
            CameraShake.Shake(3, 50, (float)_playerDeathSound.Duration.TotalSeconds / 2);
            ToCage(player, (float)_playerDeathSound.Duration.TotalSeconds);
        }

        private void ToCage(Player player, float timer)
        {
            player.TimeToCage = timer;
            player.Visible = false;
        }

        private void ToCage(Player player)
        {
            if (player != null)
            {
                if (_levelState == LEVEL_WIZARD)
                {
                    NextLevel();
                }
                else
                {
                    if (player.HasLivesLeft())
                    {
                        player.MoveTo(_currentLevel.GetCellPosition(player.CagePositionX, player.CagePositionY));
                        player.LookTo(new Vector2(MathF.Sign(5 - player.CagePositionX), 0));
                        player.SetFrame(1);
                        player.Visible = true;
                        player.InCage = true;
                        player.TimeInCage = 0;
                    }
                    else
                    {
                        if (IsMultiPlayer)
                        {
                            if (_player1.HasLivesLeft() || _player2.HasLivesLeft())
                            {
                                player.Die();
                            }
                            else
                            {
                                EndGame();
                            }
                        }
                        else
                        {
                            EndGame();
                        }
                    }
                }
            }
        }

        private void KillPlayerBullets()
        {
            _player1.KillBullet();
            if (IsMultiPlayer)
            {
                _player2.KillBullet();
            }
        }

        private void DrawRemainingLives(Player player, SpriteBatch spriteBatch)
        {
            if (player != null)
            {
                int offset = player.InCage ? 1 : 0;
                int remainingLives = player.RemainingLives;
                if (player.TimeToCage > 0)
                {
                    remainingLives++;
                }
                Vector2 startPosition = _currentLevel.GetCellPosition(player.CagePositionX, player.CagePositionY);
                Vector2 direction = new Vector2(MathF.Sign(5 - player.CagePositionX), 0);
                for (int i = 0; i < MathF.Min(2 + 1 - offset, remainingLives); i++)
                {
                    _playerSheet.DrawFrame(1, spriteBatch, startPosition + direction * (i + offset) * 16 + new Vector2(DISPLAY_OFFSET_X, DISPLAY_OFFSET_Y), 0, new Vector2(direction.X, 1), player.Color);
                }
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
            Vector2 randomPosition = _currentLevel.GetRandomPosition();
            int preferredDirection = MathF.Sign(_currentLevel.PixelWidth / 2 - randomPosition.X);
            if (preferredDirection == 0)
            {
                preferredDirection = _random.Next(0, 2) * 2 - 1;
            }
            Worluk enemy = new Worluk(_worlukSheet, GARWOR_COLOR, preferredDirection, ConfigManager.GetConfig(Constants.WORLUK_SCORE, Constants.DEFAULT_WORLUK_SCORE));

            enemy.MoveTo(randomPosition);
            enemy.LookTo(_currentLevel.PickPossibleDirection(enemy, out int _));

            _enemies.Add(enemy);
        }

        private void SpawnWizard()
        {
            Wizard enemy = new Wizard(_wizardSheet, _currentLevel, ConfigManager.GetConfig(Constants.WIZARD_SCORE, Constants.DEFAULT_WIZARD_SCORE));

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

                float speedModificator = _currentLevel.CurrentThreshold == _currentStage / 2 && _currentStage % 2 != 0 ? 1.1f : 1f;
                enemy.SetThresholdSpeed(_currentLevel.CurrentThreshold, speedModificator);
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
                        WorlukEscape();
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

                if (enemy.CanFireAtPlayer(_player1))
                {
                    enemy.Fire();
                }
                if (enemy.CanFireAtPlayer(_player2))
                {
                    enemy.Fire();
                }

                enemy.UpdateVisible(_player1);
                enemy.UpdateVisible(_player2);
            }
        }

        #endregion

        #region Level
        private void UpdateEnemiesSpawn()
        {
            if (_levelState > LEVEL_KILL_ENEMIES)
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
                                SpawnEnemy(_burworSheet, GARWOR_COLOR, canBecomeInvisible: true, ConfigManager.GetConfig(Constants.GARWOR_SCORE, Constants.DEFAULT_GARWOR_SCORE));
                                _garworToSpawn--;
                            }
                        }
                        else
                        {
                            if (_thorworToSpawn > 0)
                            {
                                // spawn a thorwor
                                SpawnEnemy(_thorworSheet, THORWOR_COLOR, canBecomeInvisible: true, ConfigManager.GetConfig(Constants.THORWOR_SCORE, Constants.DEFAULT_THORWOR_SCORE));
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