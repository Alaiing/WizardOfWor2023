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
        public int PixelHeight => _height * _cellHeight;

        private RenderTarget2D _renderTarget;
        private Color[] _renderData;
        public RenderTarget2D RenderTarget2D => _renderTarget;

        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;

        public Level(string asset, int cellWidth, int cellHeight, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
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
        }

        public void Update(float deltaTime)
        {
            // TODO: open/close corridors
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

        public CanMoveData CanMove(int positionX, int positionY)
        {
            CanMoveData canMove = new CanMoveData();

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
                }
            }

            return canMove;
        }

        private void Draw()
        {
            _graphicsDevice.SetRenderTarget(_renderTarget);
            _graphicsDevice.Clear(new Color(0,0,0,0));
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (x < _width - 1)
                    {
                        _spriteBatch.FillRectangle(new Rectangle(x * _cellWidth + 8, y * _cellHeight + 8, 4, 2), Color.White);
                    }

                    if ((_grid[x, y] & CANT_MOVE_RIGHT) > 0)
                    {
                        _spriteBatch.FillRectangle(new Rectangle(x * _cellWidth + 8, y * _cellHeight, 4, 8), Color.White);
                    }

                    if ((_grid[x, y] & CANT_MOVE_DOWN) > 0)
                    {
                        _spriteBatch.FillRectangle(new Rectangle(x * _cellWidth, y * _cellHeight + 8, 8, 2), Color.White);
                    }
                }
            }
            _spriteBatch.End();
            _graphicsDevice.SetRenderTarget(null);

            if (_renderData == null) 
            {
                _renderData = new Color[PixelWidth * PixelHeight];
            }
            _renderTarget.GetData(_renderData);
        }

        public bool HasPixel(int x, int y)
        {
            int renderIndex = x + y * PixelWidth;
            return renderIndex < 0 || renderIndex >= _renderData.Length || _renderData[renderIndex].A > 0;
        }
    }
}
