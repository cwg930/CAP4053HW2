using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Homework2
{
	public class AdjacentAgentSensor : Sensor
	{
		#region Properties
		public Dictionary<Agent, Tuple<float, float>> AgentsInRange { get; }
		#endregion

		#region Constructors
		public AdjacentAgentSensor(Agent owner, float range) : base (owner, range){
			AgentsInRange = new Dictionary<Agent, Tuple<float, float>>();
		}
		#endregion

		#region Methods
		//Finds all agents in range of owner then calculates distance and relative heading
		public override void Update (List<Agent> agents)
		{
			AgentsInRange.Clear ();
			foreach (Agent a in agents){
				if (Vector2.Distance (owner.Position, a.Position) <= range) {
					Vector2 v = a.Position - owner.Position;
					//cross product used to determine if target is cw/ccw from heading
					Vector3 crossResult = Vector3.Cross (new Vector3(owner.HeadingVector.X, owner.HeadingVector.Y, 0), new Vector3(v.X, v.Y, 0));
					//dot product of 2 unit vectors = cosine of angle between them
					float relativeHeading = (float)Math.Acos(Vector2.Dot(Vector2.Normalize(owner.HeadingVector), Vector2.Normalize(v)));
					relativeHeading = MathHelper.ToDegrees (relativeHeading);
					//make all angles out of 360 for consistency
					if (crossResult.Z < 0) {
						relativeHeading *= -1;
						relativeHeading += 360;
					}
					AgentsInRange.Add (a, new Tuple<float, float>(Vector2.Distance(owner.Position, a.Position), relativeHeading));
				}
			}
		}

		public int Draw(SpriteBatch sb, SpriteFont font, int lineNum, bool showDebugText){
			if (showDebugText) {
				foreach (KeyValuePair<Agent, Tuple<float, float>> agent in AgentsInRange) {
					sb.DrawString (font, "Agent: " + agent.Key.ToString () + " Distance: " + agent.Value.Item1
					+ " Rel. Heading: " + agent.Value.Item2, new Vector2 (0, font.LineSpacing * lineNum), Color.Black);
					lineNum++;
				}
			}
			return lineNum;
		}
		#endregion
	}
}