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
		private const float maxTurnSpeed = 1/MathHelper.TwoPi;
		private const float maxMoveSpeed = 8.0f;
		private float noCollisionBonus;
		private GoalSensor goalSensor;
		#endregion

		#region Properties
		public override Rectangle BoundingBox{
			get { return new Rectangle ((int)Position.X - AgentTexture.Width/2, (int)Position.Y - AgentTexture.Height/2, Width, Height); }
		}
		public AdjacentAgentSensor AASensor { get; set;}
		public List<PieSliceSensor> PieSliceSensors{ get; private set; }
		public List<Rangefinder> Rangefinders{ get; private set; }
		public Genome Genotype { get { return genotype; } set{ genotype = value; } }
		public NeuralNet NavNetwork { get { return navNetwork; } }
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

			Rangefinders.Add (new Rangefinder (this, 100, MathHelper.ToRadians (0), "Center"));
			Rangefinders.Add (new Rangefinder (this, 100, MathHelper.ToRadians (-45), "Left"));
			Rangefinders.Add (new Rangefinder (this, 100, MathHelper.ToRadians (45), "Right"));

			AASensor = new AdjacentAgentSensor (this, 100.0f);
			goalSensor = new GoalSensor (this, 0.0f);

			navNetwork = new NeuralNet (5, 3);
		}

		public void Update(Vector2 target)
		{
			List<double> networkInputs = new List<double> ();

			goalSensor.Update (target);
			networkInputs.Add (goalSensor.Distance);
			networkInputs.Add (goalSensor.Angle);
			foreach (Rangefinder r in Rangefinders) {
				r.Update (Game1.Walls);
				networkInputs.Add (r.Reading); //scale rangefinders to be between 0 and 1
			}
				
			List<double> networkOutput = navNetwork.Update (networkInputs);

			if (networkOutput.Count < navNetwork.NumOutputs) {
				Console.WriteLine ("Error in network calculations");
				return;
			}
		/*	Console.Write("\nInput: ");
			foreach (double d in networkInputs) {
				Console.Write (d + ", ");
			}
		*/
			moveForward = (float)networkOutput [0];
			//moveBack = (float)networkOutput [1];
			turnLeft = (float)networkOutput [1];
			turnRight = (float)networkOutput [2];

			float rotation = turnRight - turnLeft;
			//float moveDir = moveForward - moveBack;
			//Console.Write ("\nRot: " + rotation + ", Dir: "+ moveDir);

			rotation = MathHelper.Clamp (rotation * maxTurnSpeed, -maxTurnSpeed, maxTurnSpeed);

			Heading += rotation;

			if (moveForward > 0.5f && Rangefinders [0].Reading > 15 && Rangefinders [1].Reading > 10 && Rangefinders [2].Reading > 10) {
				Vector2 velocity = new Vector2 (MathHelper.Clamp(HeadingVector.X * maxMoveSpeed * moveForward,-maxMoveSpeed, maxMoveSpeed), 
					MathHelper.Clamp(HeadingVector.Y * maxMoveSpeed * moveForward, -maxMoveSpeed, maxMoveSpeed));
				Position += velocity;
			}

			bool collided = false;
			foreach (Wall w in Game1.Walls) {
				if (this.DetectCollision (w)) {
					collided = true;
				}
			}
			if (!collided) {
				noCollisionBonus += 1;
			}
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
				+ "\nPosition (x,y): " + Position.ToString (), new Vector2 (32, font.LineSpacing * lineNum + 32), Color.Black);
				lineNum += 2;
			}
				lineNum = AASensor.Draw (spriteBatch, font, lineNum, false);
				foreach (PieSliceSensor p in PieSliceSensors) {
					lineNum = p.Draw (spriteBatch, font, lineNum, false);
				}
				foreach (Rangefinder r in Rangefinders) {
					lineNum = r.Draw (spriteBatch, font, lineNum, false);
				}
			return lineNum;

		}

		public bool SeekTarget(Vector2 target, Viewport viewport)
		{
			float distance = Vector2.Distance (Position, target);
			if (distance >= 1f) {
				Console.WriteLine ("Target: " + target.ToString() + "\nDistance: " + distance);
				Vector2 v = Vector2.Normalize (target - Position);
				float angle = (float)Math.Acos (Vector2.Dot (Vector2.Normalize (HeadingVector), v));
				Console.WriteLine ("angle: " + angle);
				if (angle >= 0.1f) {
					//float angle = (float)Math.Acos (Vector2.Dot (Vector2.Normalize (player.HeadingVector), v));
					float crossZ = Vector3.Normalize (Vector3.Cross (new Vector3 (Position.X, Position.Y, 0), new Vector3 (v.X, v.Y, 0))).Z;
					Heading += maxTurnSpeed * crossZ;
				} else {
					Position += v * distance/maxMoveSpeed;
				}
				float clampedX = MathHelper.Clamp (Position.X, Width / 2, viewport.Width - Width / 2);
				float clampedY = MathHelper.Clamp (Position.Y, Height / 2, viewport.Height - Height / 2);
				Position = new Vector2 (clampedX, clampedY);
				return false;
			} else {
				return true;
			}

		}

		public void EndOfRun(Vector2 start, Vector2 target)
		{
			genotype.Fitness += Vector2.Distance (Position, start) - Vector2.Distance (Position, target);
			genotype.Fitness += 1 / Vector2.Distance (Position, target);
			if (noCollisionBonus > 20)
				genotype.Fitness += 20;
			else
				genotype.Fitness += noCollisionBonus;
		}

		public void UpdateGenotype(Genome genome){
			this.genotype = genome;
			navNetwork.SetWeights (genotype.Weights);
		}
		#endregion
	}
}

