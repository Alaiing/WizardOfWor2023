using C3.MonoGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WizardOfWor
{
	public class SpriteSheet
	{
		private readonly Texture2D _texture;
		private Rectangle[] frames;
		public int FrameCount => frames.Length;

		private Vector2 _spritePivot;
		public Vector2 SpritePivot => _spritePivot;

        public int LeftMargin { get; private set; }
        public int RightMargin { get; private set; }
        public int TopMargin { get; private set; }
        public int BottomMargin { get; private set; }

        public SpriteSheet(ContentManager content, string asset, int spriteWidth, int spriteHeight, int spritePivotX = 0, int spritePivotY = 0)
		{
			_texture = content.Load<Texture2D>(asset);
			_spritePivot = new Vector2(spritePivotX, spritePivotY);
			LeftMargin = spritePivotX;
			RightMargin = spriteWidth - spritePivotY;
			TopMargin = spritePivotY;
			BottomMargin = spriteHeight - spritePivotY;
			InitFrames(spriteWidth, spriteHeight);
		}

		private void InitFrames(int spriteWidth, int spriteHeight)
		{
			int xCount = _texture.Width / spriteWidth;
			int yCount = _texture.Height / spriteHeight;
			frames = new Rectangle[xCount * yCount];

			for (int x = 0; x < xCount; x++) 
			{
				for (int y = 0; y < yCount; y++) 
				{
					frames[x + y * xCount] = new Rectangle(x * spriteWidth, y * spriteHeight, spriteWidth, spriteHeight);
				}
			}
		}

		public void DrawFrame(int frameIndex, SpriteBatch spriteBatch, Vector2 position, float rotation, Vector2 scale, Color color)
		{
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (scale.X < 0)
			{
				spriteEffects |= SpriteEffects.FlipHorizontally;
				scale.X = -scale.X;
			}
            if (scale.Y < 0)
            {
				spriteEffects |= SpriteEffects.FlipVertically;
				scale.Y = -scale.Y;
            }
            spriteBatch.Draw(_texture, position + _spritePivot, frames[frameIndex], color, rotation, _spritePivot, scale, spriteEffects, 0);
        }
    }
}