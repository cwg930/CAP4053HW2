using System;
using Microsoft.Xna.Framework;

namespace Homework2
{
	public class GoalSensor : Sensor
	{
		private float distance;
		private float angle;

		public float Distance { get { return distance; } }
		public float Angle { get { return angle; } }

		public GoalSensor (Agent owner, float range) : base(owner, range)
		{
		}

		public override void Update (System.Collections.Generic.List<Agent> agents)
		{
			throw new NotImplementedException ();
		}

		public void Update(Vector2 target)
		{
			Vector2 v = target - owner.Position;
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
			angle = relativeHeading;
			distance = Vector2.Distance (owner.Position, target);
		}
	}
}

