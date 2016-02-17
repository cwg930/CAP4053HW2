using System;
using System.Collections.Generic;

namespace Homework2
{
	public class Genome
	{
		private double fitness;
		private List<double> weights;

		public List<double> Weights { get { return weights; } set { weights = value; } }
		public double Fitness { get { return fitness; } set { fitness = value; } }

		public Genome()
		{
			fitness = 0;
		}
		public Genome(double fitness, List<double> weights)
		{
			this.Weights = weights;
			this.fitness = fitness;
		}

		public bool operator <(Genome lhs, Genome rhs)
		{
			return lhs.fitness < rhs.fitness;
		}


	}
}

