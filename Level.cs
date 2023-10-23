using C3.MonoGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace WizardOfWor
{
    public class Level
    {
        public const int CAN_MOVE = 0;
        public const int CANT_MOVE_RIGHT = 1;
        public const int CANT_MOVE_DOWN = 2;
        public const float TIME_BETWEEN_ACCELERATIONS = 10;
        public const int MAX_THRESHOLDS = 4;
        private const float TUNNEL_COOLDOWN = 2f;

        public const int NO_TUNNEL = 0;
        public const int TUNNEL_LEFT = 1;
        public const int TUNNEL_RIGHT = 2;

        public struct CanMoveData
        {
            public bool Up, Down, Left, Right;
            public override string ToString()
            {
                return $"Up: {Up} - Down: {Down} - Left: {Left} - Right: {Right}";
            }
        }

        private readonly int[,] _grid;
        private int _width;
        private int _height;
        private int _cellWidth;
        private int _cellHeight;
        public Color Color { get; private set; }

        public int PixelWidth => _width * _cellWidth;
        public int PixelHeight => _height * _cellHeight + RadarHeight;
        public int RadarHeight => (_height + 1) * 2;

        private RenderTarget2D _renderTarget;
        private Color[] _renderData;
        public RenderTarget2D RenderTarget2D => _renderTarget;

        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;

        private float _elapsedTime;
        private int _currentThreshold;
        public int CurrentThreshold => _currentThreshold;

        private float _tunnelTimer;
        public bool TunnelsOpen { get; private set; }
        private int _tunnelY = 3;
        private int _tunnelLeftX = 1;
        private int _tunnelRightX = 11;

        private Random _random;

        public Level(string asset, int cellWidth, int cellHeight, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Random random)
        {
            string[] lines = System.IO.File.ReadAllLines(asset);
            _height = lines.Length;
            _width = lines[0].Length;
            _grid = new int[_width, _height];

            for (int y = 0; y < lines.Length; y++)
            {
                for (int x = 0; x < lines[y].Length; x++)
                {
                    _grid[x, y] = int.Parse(lines[y][x].ToString());
                }
            }

            _cellWidth = cellWidth;
            _cellHeight = cellHeight;

            _spriteBatch = spriteBatch;
            _graphicsDevice = graphicsDevice;

            _renderTarget = new RenderTarget2D(_graphicsDevice, PixelWidth, PixelHeight);
            Draw();
            _random = random;
        }

        public void Reset(int currentStage)
        {
            _elapsedTime = 0;
            _currentThreshold = Math.Min(MAX_THRESHOLDS, currentStage / 2);
            _tunnelTimer = 0;
            TunnelsOpen = true;
        }

        public void Update(float deltaTime)
        {
            if (_currentThreshold < MAX_THRESHOLDS)
            {
                _elapsedTime += deltaTime;
                if (_elapsedTime >= TIME_BETWEEN_ACCELERATIONS)
                {
                    _currentThreshold = Math.Min(_currentThreshold + 1, MAX_THRESHOLDS);
                    _elapsedTime -= TIME_BETWEEN_ACCELERATIONS;
                }
            }

            _tunnelTimer += deltaTime;
            if (_tunnelTimer > TUNNEL_COOLDOWN)
            {
                TunnelsOpen = !TunnelsOpen;
                _tunnelTimer -= TUNNEL_COOLDOWN;
            }
        }

        public Vector2 GetCellPosition(int x, int y)
        {
            return new Vector2(x * _cellWidth, y * _cellHeight);
        }

        public bool IsOnGridCell(int positionX, int positionY)
        {
            int deltaX = positionX % _cellWidth;
            int deltaY = positionY % _cellHeight;

            return deltaX == 0 && deltaY == 0;
        }

        public Vector2 GetRandomPosition()
        {
            int gridX = 1 + _random.Next(11);
            int gridY = 1 + _random.Next(6);
            return GetCellPosition(gridX, gridY);
        }

        public Vector2 GetTunnelPosition(int tunnel)
        {
            Vector2 newPosition = new Vector2();
            newPosition.Y = _tunnelY * _cellHeight;
            if (tunnel == TUNNEL_LEFT)
                newPosition.X = _tunnelLeftX * _cellWidth;
            else if (tunnel == TUNNEL_RIGHT)
                newPosition.X = _tunnelRightX * _cellWidth;

            return newPosition;
        }

        public CanMoveData CanMove(int positionX, int positionY, out int tunnelTeleport)
        {
            CanMoveData canMove = new CanMoveData();

            tunnelTeleport = NO_TUNNEL;

            int closestGridCellX = positionX / _cellWidth;
            int deltaX = positionX % _cellWidth;
            int closestGridCellY = positionY / _cellHeight;
            int deltaY = positionY % _cellHeight;

            if (deltaX != 0)
            {
                canMove.Left = canMove.Right = true;
                canMove.Up = canMove.Down = false;
            }
            else if (deltaY != 0)
            {
                canMove.Left = canMove.Right = false;
                canMove.Up = canMove.Down = true;
            }
            else
            {
                if (closestGridCellX < 1 || closestGridCellX > _width - 1 || closestGridCellY < 1 || closestGridCellY > _height - 1)
                {
                    canMove.Left = canMove.Right = canMove.Up = canMove.Down = false;

                }
                else
                {
                    if (closestGridCellX == _width - 1)
                    {
                        canMove.Right = false;
                    }
                    else
                    {
                        canMove.Right = (_grid[closestGridCellX, closestGridCellY] & CANT_MOVE_RIGHT) == 0;
                    }

                    if (closestGridCellY == _height - 1)
                    {
                        canMove.Down = false;
                    }
                    else
                    {
                        canMove.Down = (_grid[closestGridCellX, closestGridCellY] & CANT_MOVE_DOWN) == 0;
                    }

                    if (closestGridCellX - 1 < 0)
                    {
                        canMove.Left = false;
                    }
                    else
                    {
                        canMove.Left = (_grid[closestGridCellX - 1, closestGridCellY] & CANT_MOVE_RIGHT) == 0;
                    }

                    if (closestGridCellY - 1 < 0)
                    {
                        canMove.Up = false;
                    }
                    else
                    {
                        canMove.Up = (_grid[closestGridCellX, closestGridCellY - 1] & CANT_MOVE_DOWN) == 0;
                    }

                    if (TunnelsOpen && closestGridCellY == _tunnelY)
                    {
                        if (closestGridCellX == _tunnelLeftX)
                        {
                            tunnelTeleport = TUNNEL_LEFT;
                        }
                        else if (closestGridCellX == _tunnelRightX)
                        {
                            tunnelTeleport = TUNNEL_RIGHT;
                        }
                    }
                }
            }

            return canMove;
        }

        public Vector2 PickPossibleDirection(Enemy character, out int tunnel)
        {
            tunnel = NO_TUNNEL;
            if (IsOnGridCell(character.PixelPositionX, character.PixelPositionY))
            {
                int sideOfLevel = MathF.Sign(PixelWidth / 2 - character.PixelPositionX);
                bool isOnWrongSide = character.PreferredHorizontalDirection != 0 && sideOfLevel == character.PreferredHorizontalDirection;

                if (character.CanChangeDirection)
                {
                    CanMoveData canMove = CanMove(character.PixelPositionX, character.PixelPositionY, out tunnel);

                    if (character.PreferredHorizontalDirection != 0 && tunnel != NO_TUNNEL)
                    {
                        character.CanChangeDirection = false;
                        if (tunnel == TUNNEL_RIGHT)
                        {
                            return new Vector2(1, 0);
                        }
                        if (tunnel == TUNNEL_LEFT)
                        {
                            return new Vector2(-1, 0);
                        }
                    }

                    List<Vector2> possibleDirections = new();
                    if (character.MoveDirection.X > 0 || character.MoveDirection.X < 0)
                    {
                        if (character.MoveDirection.X > 0 && (canMove.Right || tunnel == TUNNEL_RIGHT)
                            || character.MoveDirection.X < 0 && (canMove.Left || tunnel == TUNNEL_LEFT))
                        {
                            if (isOnWrongSide)
                            {
                                if (character.PreferredHorizontalDirection == character.MoveDirection.X)
                                {
                                    character.CanChangeDirection = false;
                                    return character.MoveDirection;
                                }
                                else if (!canMove.Down && !canMove.Up)
                                {
                                    possibleDirections.Add(character.MoveDirection);
                                }
                            }
                            else
                            {
                                possibleDirections.Add(character.MoveDirection);
                            }

                        }

                        if (canMove.Up)
                            possibleDirections.Add(new Vector2(0, -1));
                        if (canMove.Down)
                            possibleDirections.Add(new Vector2(0, 1));

                        if (possibleDirections.Count == 0)
                            possibleDirections.Add(-character.MoveDirection);
                    }

                    if (character.MoveDirection.Y > 0 || character.MoveDirection.Y < 0)
                    {
                        if (character.MoveDirection.Y > 0 && canMove.Down || character.MoveDirection.Y < 0 && canMove.Up)
                        {
                            possibleDirections.Add(character.MoveDirection);
                        }

                        if (canMove.Right || tunnel == TUNNEL_RIGHT)
                        {
                            if (isOnWrongSide && character.PreferredHorizontalDirection > 0)
                            {
                                character.CanChangeDirection = false;
                                return new Vector2(1, 0);
                            }
                            else
                            {
                                possibleDirections.Add(new Vector2(1, 0));
                            }
                        }
                        if (canMove.Left || tunnel == TUNNEL_LEFT)
                        {
                            if (isOnWrongSide && character.PreferredHorizontalDirection < 0)
                            {
                                character.CanChangeDirection = false;
                                return new Vector2(-1, 0);
                            }
                            else
                            {
                                possibleDirections.Add(new Vector2(-1, 0));
                            }
                        }

                        if (possibleDirections.Count == 0)
                            possibleDirections.Add(-character.MoveDirection);
                    }

                    if (possibleDirections.Count > 1)
                    {
                        if (isOnWrongSide && character.PreferredHorizontalDirection != 0)
                        {
                            possibleDirections.Remove(new Vector2(-character.PreferredHorizontalDirection, 0));
                        }
                    }

                    Vector2 chosenDirection = possibleDirections[_random.Next(0, possibleDirections.Count)];
                    character.CanChangeDirection = false;

                    return chosenDirection;
                }
            }
            else
            {
                character.CanChangeDirection = true;
            }

            return character.MoveDirection;
        }

        private void Draw()
        {
            _graphicsDevice.SetRenderTarget(_renderTarget);
            _graphicsDevice.Clear(new Color(0, 0, 0, 0));
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (x < _width - 1 && !(y == _height - 1 && x < _width - 3 && x > 1))
                    {
                        _spriteBatch.FillRectangle(new Rectangle(x * _cellWidth + 8, y * _cellHeight + 8, 4, 2), Color.White);
                    }

                    if ((_grid[x, y] & CANT_MOVE_DOWN) > 0)
                    {
                        _spriteBatch.FillRectangle(new Rectangle(x * _cellWidth, y * _cellHeight + 8, 8, 2), Color.White);
                    }

                    if (y == _tunnelY && (x == _tunnelLeftX - 1 || x == _tunnelRightX))
                        continue;

                    if ((_grid[x, y] & CANT_MOVE_RIGHT) > 0)
                    {
                        DrawVerticalWall(x, y, Color.White);
                    }
                }
            }
            _spriteBatch.FillRectangle(new Rectangle(52, 78, 4, 16), Color.White);
            _spriteBatch.FillRectangle(new Rectangle(52 + 12 * 4, 78, 4, 16), Color.White);
            _spriteBatch.FillRectangle(new Rectangle(54, 78, 12 * 4, 2), Color.White);
            _spriteBatch.FillRectangle(new Rectangle(54, 78 + 7 * 2, 12 * 4, 2), Color.White);

            _spriteBatch.End();
            _graphicsDevice.SetRenderTarget(null);

            if (_renderData == null)
            {
                _renderData = new Color[PixelWidth * PixelHeight];
            }
            _renderTarget.GetData(_renderData);
        }

        public void DrawTunnels(Color color, int offsetX, int offsetY)
        {
            if (!TunnelsOpen)
            {
                DrawVerticalWall(_tunnelLeftX - 1, _tunnelY, color, offsetX, offsetY);
                DrawVerticalWall(_tunnelRightX, _tunnelY, color, offsetX, offsetY);
            }
        }

        private void DrawVerticalWall(int x, int y, Color color, int offsetX = 0, int offsetY = 0)
        {
            _spriteBatch.FillRectangle(new Rectangle(x * _cellWidth + 8 + offsetX, y * _cellHeight + offsetY, 4, 8), color);
        }

        public void DrawRadar(List<Enemy> enemies)
        {
            foreach (Enemy enemy in enemies)
            {
                int closestGridCellX = enemy.PixelPositionX / _cellWidth;
                int closestGridCellY = enemy.PixelPositionY / _cellHeight + 1;

                Color color = enemy.Color;
                color.A = 128;
                _spriteBatch.FillRectangle(new Rectangle(56 + closestGridCellX * 4, 83 + closestGridCellY * 2, 4, 2), color);
            }
        }

        public bool HasPixel(int x, int y)
        {
            int renderIndex = x + y * PixelWidth;
            return renderIndex < 0 || renderIndex >= _renderData.Length || _renderData[renderIndex].A > 0;
        }
    }
}
