using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Homework2
{
	public class PieSliceSensor : Sensor
	{
		#region Fields
		private float headingOffset;
		private String marker;
		#endregion

		#region Properties
		public float MinAngle
		{
			get { 
				float temp = (owner.Heading + headingOffset - MathHelper.ToRadians (45)) % MathHelper.ToRadians (360);
				if (temp < 0)
					temp += MathHelper.ToRadians (360);
				return temp;
			}
			private set {}
		}
		public float MaxAngle
		{
			get { return (owner.Heading + headingOffset + MathHelper.ToRadians (45)) % MathHelper.ToRadians (360);}
			private set {}
		}
		public float Orientation 
		{
			get { return (owner.Heading + headingOffset) % MathHelper.ToRadians(360); }
			private set { }
		}
		public int ActivationLevel {
			get;
			private set;
		}
		public Vector2 HeadingVector { get { return new Vector2 ((float)Math.Cos (Orientation), (float)Math.Sin (Orientation)); } }
		public String Marker {
			get { return marker; }
			private set{ }
		}
		public List<Agent> DetectedAgents { get; set; }
		#endregion

		#region Constructors
		public PieSliceSensor (Agent owner, float range, float headingOffset, String marker) : base(owner, range)
		{
			this.headingOffset = headingOffset;
			this.marker = marker;
			DetectedAgents = new List<Agent> ();
		}
		#endregion

		#region Methods
		public override void Update(List<Agent> agents)
		{
			ActivationLevel = 0;
			DetectedAgents.Clear ();
			foreach (Agent a in agents)
			{
				// Determine if the agent falls within our min and max angle
				if (Vector2.Distance (owner.Position, a.Position) <= range)
				{
					//dot product of 2 unit vectors = cosine of angle between them
					Vector2 v = a.Position - owner.Position;
					Vector3 crossResult = Vector3.Cross (new Vector3 (this.HeadingVector.X, this.HeadingVector.Y, 0), new Vector3 (v.X, v.Y, 0));
					float relativeHeading = (float)Math.Acos (Vector2.Dot (Vector2.Normalize (this.HeadingVector), Vector2.Normalize (v)));
					if (crossResult.Z < 0) {
						relativeHeading *= -1;
						relativeHeading += MathHelper.ToRadians(360);
					}

					float highConstraint = MaxAngle;
					float lowConstraint = MinAngle;
					// When the minimum angle is within the pie slice sensor arc of the 0 angle,
					//  we have a special case that must be handled to account for values wrapping
					//  from the full circle 2*PI angle to small, near 0 angles.
					if (lowConstraint >= MathHelper.ToRadians (270))
					{
						// Initially check if we're between low and 2*PI
						float temp = highConstraint;
						highConstraint = 2.0f * (float)Math.PI;
						CheckConstraints (a, relativeHeading, lowConstraint, highConstraint);
						// Also then check if we were between 0 and our initial "high" constraint which would
						//  be some small angle.
						highConstraint = temp;
						lowConstraint = 0.0f;
						CheckConstraints (a, relativeHeading, lowConstraint, highConstraint);
					}
					else
					{
						// If we're not in the special case just check that the relative heading is
						//  bounded by the min and max angles (constraints here).
						CheckConstraints (a, relativeHeading, lowConstraint, highConstraint);
					}
				}
			}
		}

		private void CheckConstraints(Agent a, float relativeHeading, float lowConstraint, float highConstraint)
		{
			if ((Orientation + relativeHeading) % MathHelper.ToRadians (360) < highConstraint && (Orientation + relativeHeading) % MathHelper.ToRadians (360) > lowConstraint)
			{
				ActivationLevel++;
				DetectedAgents.Add (a);
			}
		}
		#endregion
	}
}