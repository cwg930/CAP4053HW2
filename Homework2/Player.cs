using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Homework2
{
	public class Player : Agent
	{
		#region Fields
		private Vector2 center; 
		#endregion

		#region Properties
		public override Rectangle BoundingBox{
			get { return new Rectangle ((int)Position.X - AgentTexture.Width/2, (int)Position.Y - AgentTexture.Height/2, Width, Height); }
		}
		public AdjacentAgentSensor AASensor { get; set;}
		public List<PieSliceSensor> PieSliceSensors{ get; private set; }
		public List<Rangefinder> Rangefinders{ get; private set; }
		#endregion

		#region Methods
		public void Initialize(Texture2D texture, Vector2 position, float heading)
		{
			AgentTexture = texture;
			Position = position;
			Heading = heading;
			center.X = AgentTexture.Width / 2;
			center.Y = AgentTexture.Height / 2;

			PieSliceSensors = new List<PieSliceSensor> ();
			Rangefinders = new List<Rangefinder> ();

			PieSliceSensors.Add (new PieSliceSensor (this, 100, 0, "1"));
			PieSliceSensors.Add (new PieSliceSensor (this, 100, MathHelper.ToRadians (90), "2"));
			PieSliceSensors.Add (new PieSliceSensor (this, 100, MathHelper.ToRadians (180), "3"));
			PieSliceSensors.Add (new PieSliceSensor (this, 100, MathHelper.ToRadians (270), "4"));

			Rangefinders.Add (new Rangefinder (this, 100, MathHelper.ToRadians (0)));
			Rangefinders.Add (new Rangefinder (this, 100, MathHelper.ToRadians (-45)));
			Rangefinders.Add (new Rangefinder (this, 100, MathHelper.ToRadians (45)));

			AASensor = new AdjacentAgentSensor (this, 100.0f);
		}

		public void UpdateSensors()
		{
			AASensor.Update (Game1.Agents);
			foreach (PieSliceSensor p in PieSliceSensors) {
				p.Update(Game1.Agents);
			}
			foreach (Rangefinder r in Rangefinders) {
				r.Update (Game1.Walls);
			}
		}
			
		public void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw (AgentTexture, Position, null, Color.White, Heading, center, 1.0f, SpriteEffects.None, 0f);
		}
		#endregion
	}
}

