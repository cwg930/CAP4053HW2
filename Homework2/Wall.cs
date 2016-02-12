using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Homework2
{
	public class Wall : Agent
	{
		public override Rectangle BoundingBox {
			get { return new Rectangle ((int)Position.X, (int)Position.Y, Width, Height); }
		}

		public void Initialize(Texture2D texture, Vector2 position)
		{
			AgentTexture = texture;
			Position = position;
		}

		public void Draw(SpriteBatch sb)
		{
			sb.Draw (AgentTexture, Position, Color.White);
		}

	}
}

