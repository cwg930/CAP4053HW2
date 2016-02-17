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
		private NeuralNet navNetwork;
		private Genome genotype;
		private float moveForward;
		private float moveBack;
		private float turnLeft;
		private float turnRight;
		private float maxTurnSpeed;
		private float maxMoveSpeed;
		private float noCollisionBonus;
		#endregion

		#region Properties
		public override Rectangle BoundingBox{
			get { return new Rectangle ((int)Position.X - AgentTexture.Width/2, (int)Position.Y - AgentTexture.Height/2, Width, Height); }
		}
		public AdjacentAgentSensor AASensor { get; set;}
		public List<PieSliceSensor> PieSliceSensors{ get; private set; }
		public List<Rangefinder> Rangefinders{ get; private set; }
		public Genome Genotype { get { return genotype; } set{ genotype = value; } }
		#endregion

		#region Methods
		public void Initialize(Texture2D texture, Vector2 position, float heading, float turnSpeed, float moveSpeed)
		{
			AgentTexture = texture;
			Position = position;
			Heading = heading;
			center.X = AgentTexture.Width / 2;
			center.Y = AgentTexture.Height / 2;
			maxMoveSpeed = moveSpeed;
			maxTurnSpeed = turnSpeed;

			PieSliceSensors = new List<PieSliceSensor> ();
			Rangefinders = new List<Rangefinder> ();

			PieSliceSensors.Add (new PieSliceSensor (this, 100, 0, "1"));
			PieSliceSensors.Add (new PieSliceSensor (this, 100, MathHelper.ToRadians (90), "2"));
			PieSliceSensors.Add (new PieSliceSensor (this, 100, MathHelper.ToRadians (180), "3"));
			PieSliceSensors.Add (new PieSliceSensor (this, 100, MathHelper.ToRadians (270), "4"));

			Rangefinders.Add (new Rangefinder (this, 100, MathHelper.ToRadians (0), "Center"));
			Rangefinders.Add (new Rangefinder (this, 100, MathHelper.ToRadians (-45), "Left"));
			Rangefinders.Add (new Rangefinder (this, 100, MathHelper.ToRadians (45), "Right"));

			AASensor = new AdjacentAgentSensor (this, 100.0f);
			navNetwork = new NeuralNet (4, 4);
		}

		public bool Update()
		{
			List<double> networkInputs = new List<double> ();

			foreach (Rangefinder r in Rangefinders) {
				r.Update (Game1.Walls);
				networkInputs.Add (r.Reading/r.Range);
			}
				
			List<double> networkOutput = navNetwork.Update (networkInputs);

			if (networkOutput.Count < navNetwork.NumOutputs) {
				return false;
			}

			moveForward = networkOutput [0];
			moveBack = networkOutput [1];
			turnLeft = networkOutput [2];
			turnRight = networkOutput [3];

			double rotation = turnRight - turnLeft;
			double moveDir = moveForward - moveBack;

			rotation = MathHelper.Clamp (rotation * maxTurnSpeed, -maxTurnSpeed, maxTurnSpeed);

			Heading += rotation;

			if (!this.DetectCollision (Game1.Walls)) {
				Vector2 velocity = new Vector2 (HeadingVector.X * maxMoveSpeed * moveDir, HeadingVector.Y * maxMoveSpeed * moveDir);
				Position += velocity;
			}

			if (!this.DetectCollision (Game1.Walls)) {
				noCollisionBonus += 1;
			}
			return true;
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
			
		public int Draw(SpriteBatch spriteBatch, SpriteFont font, int lineNum, bool playerDebugActive)
		{
			spriteBatch.Draw (AgentTexture, Position, null, Color.White, Heading, center, 1.0f, SpriteEffects.None, 0f);
			if (playerDebugActive) {
				spriteBatch.DrawString (font, "Heading (deg): " + (MathHelper.ToDegrees (Heading) % 360)
				+ "\nPosition (x,y): " + Position.ToString (), new Vector2 (0, font.LineSpacing * lineNum), Color.Black);
				lineNum += 2;
				lineNum = AASensor.Draw (spriteBatch, font, lineNum);
				foreach (PieSliceSensor p in PieSliceSensors) {
					lineNum = p.Draw (spriteBatch, font, lineNum);
				}
				foreach (Rangefinder r in Rangefinders) {
					lineNum = r.Draw (spriteBatch, font, lineNum);
				}
			}
		}

		public void EndOfRun()
		{
			genotype.Fitness += noCollisionBonus;
		}
		#endregion
	}
}

