using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Homework2
{
	/// <summary>
	/// Abstract base class for sensor implementations. Sensor types should be
	/// derived from this class to allow for polymorphic management of sensor
	/// tasks and actions.
	/// </summary>
	public abstract class Sensor
	{
		#region Fields
		protected Agent owner;
		protected float range;
		#endregion

		#region Properties
		public Vector2 Origin 
		{ 
			get
			{ 
				return owner.Position; 
			}
			private set 
			{
			}
		}
		#endregion

		#region Constructors
		public Sensor (Agent owner, float range)
		{
			this.owner = owner;
			this.range = range;
		}
		#endregion

		#region Methods
		public abstract void Update(List<Agent> agents);
		#endregion
	}
}

