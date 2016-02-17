using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Homework2
{
	/// <summary>
	/// Concrete impelementation of the Rangefinder sensor.  Returns the range to the nearest point
	/// lying on an Agent's bounding box found along a single ray cast by itself.
	/// </summary>
	public class Rangefinder : Sensor
	{
		#region Fields
		private float headingOffset;
		private Point foundPoint;
		private Texture2D texture;
		private string label;
		#endregion

		#region Properties
		public float Reading { get; private set; }
		public float Heading 
		{
			get 
			{
				return (owner.Heading + headingOffset) % MathHelper.ToRadians (360);
			}
			private set {
			}
		}
		public Point FoundPoint {
			get {
				return foundPoint;
			}
			private set{ }
		}
		#endregion

		#region Constructors
		public Rangefinder (Agent owner, int range, float headingOffset, string label) : base(owner, range)
		{
			this.headingOffset = headingOffset;
			this.label = label;
		}
		#endregion

		#region Methods
		public override void Update(List<Agent> agents)
		{
			float currentReading = findMinDistance (agents);
			if (currentReading < range)
			{
				Reading = currentReading;
			} 
			else
			{
				Reading = range;
			}
		}
			
		private float findMinDistance(List<Agent> agents)
		{
			float runningMinimum = range;
			bool found = false;

			Point rangefinderOrigin = new Point ((int)Origin.X, (int)Origin.Y);
			Point rangefinderEnd = new Point ((int)(Origin.X + range * Math.Cos (Heading)),
				(int)(Origin.Y + range * Math.Sin (Heading)));
			
			foreach (Agent agent in agents)
			{
				// Determine intersection and compute distance
				Rectangle boundingBox = agent.BoundingBox;

				// Get 4 corners of rectangle moving from top left clockwise.
				Point corner1 = new Point (boundingBox.Left, boundingBox.Top);
				Point corner2 = new Point (boundingBox.Right, boundingBox.Top);
				Point corner3 = new Point (boundingBox.Right, boundingBox.Bottom);
				Point corner4 = new Point (boundingBox.Left, boundingBox.Bottom);

				Point[] points = { corner1, corner2, corner3, corner4 };

				for (int i = 0; i < points.Length; i++)
				{
					Point tempPoint = lineIntersection (points [i % points.Length], points [(i + 1) % points.Length],
						                          rangefinderOrigin, rangefinderEnd);

					Vector2 intersectionVector = new Vector2 (tempPoint.X, tempPoint.Y);
					float distance = Vector2.Distance (Origin, intersectionVector);
					if (distance < runningMinimum)
					{
						found = true;
						runningMinimum = distance;
						foundPoint = tempPoint;
					}
				}
			}

			if (!found)
				foundPoint = rangefinderEnd;

			return runningMinimum;
		}

		private Point lineIntersection (Point a1, Point a2, Point b1, Point b2)
		{
			float ua = 0.0f;
			float ub = 0.0f;
			float denominator = (b2.Y - b1.Y) * (a2.X - a1.X) - (b2.X - b1.X) * (a2.Y - a1.Y);
			float uaNumerator = (b2.X - b1.X) * (a1.Y - b1.Y) - (b2.Y - b1.Y) * (a1.X - b1.X);
			float ubNumerator = (a2.X - a1.X) * (a1.Y - b1.Y) - (a2.Y - a1.Y) * (a1.X - b1.X);

			if (denominator != 0)
			{
				ua = uaNumerator / denominator;
				ub = ubNumerator / denominator;
			}

			if (ua > 0 && ua < 1 && ub > 0 && ub < 1)
			{
				return new Point ((int)(a1.X + ua * (a2.X - a1.X)), (int)(a1.Y + ua * (a2.Y - a1.Y)));
			}
			else
			{
				// Placeholder to just return a huge value of intersection at infinity
				return new Point (100000, 100000);
			}
		}

		public int Draw(SpriteBatch spriteBatch, SpriteFont font, int lineNum){
			Vector2 markerSize = font.MeasureString ("o");
			spriteBatch.DrawString (font, "o", new Vector2 (FoundPoint.X - markerSize.X / 2, FoundPoint.Y - markerSize.Y / 2), Color.Red);
			spriteBatch.DrawString (font, label +" Rangefinder: " + Reading , new Vector2 (0, font.LineSpacing * lineNum), Color.Black);
			return lineNum + 1;
		}
		#endregion
	}
}

