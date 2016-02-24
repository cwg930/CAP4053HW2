using System;
using System.Collections.Generic;

namespace Homework2
{
	public class Genome : IComparable
	{
		private const double mutationRate = 0.25f;
		private const double mutationStrength = 0.5f;
		private const double crossoverRate = 0.7f;

		private double fitness;
		private List<double> weights;

		public List<double> Weights { get { return weights; } set { weights = value; } }
		public double Fitness { get { return fitness; } set { fitness = value; } }
		static Random r = new Random();

		public Genome()
		{
			fitness = 0;
			weights = new List<double> ();
		}
		public Genome(double fitness, List<double> weights)
		{
			this.Weights = weights;
			this.fitness = fitness;
		}

		public static bool operator <(Genome lhs, Genome rhs)
		{
			return lhs.fitness < rhs.fitness;
		}

		public static bool operator >(Genome lhs, Genome rhs)
		{
			return lhs.fitness > rhs.fitness;
		}

		public void Mutate()
		{
			for (int i = 0; i < weights.Count; i++) {
				if (r.NextDouble () < mutationRate) {
					weights [i] += (r.NextDouble () * 2 - 1) * mutationStrength;
				}	
			}
		}
			
		public static Tuple<Genome, Genome> Crossover(Genome mom, Genome dad){
			Tuple<Genome, Genome> children = new Tuple<Genome, Genome> (new Genome(), new Genome());
			if (r.NextDouble () > crossoverRate || mom == dad) {
				children.Item1.Weights = new List<double>(mom.Weights);
				children.Item2.Weights = new List<double>(dad.Weights);
				return children;
			}

			int crossPoint = r.Next (0, mom.Weights.Count - 1);

			for (int i = 0; i < crossPoint; i++) {
				children.Item1.Weights.Add (mom.Weights [i]);
				children.Item2.Weights.Add (dad.Weights [i]);
			}
			for (int i = crossPoint; i < mom.Weights.Count; i++) {
				children.Item1.Weights.Add (dad.Weights [i]);
				children.Item2.Weights.Add (mom.Weights [i]);
			}
			return children;
		}
			
		public int CompareTo (object obj)
		{
			Genome g = obj as Genome;
			if (this.fitness > g.fitness)
				return 1;
			else if (this.fitness < g.fitness)
				return -1;
			else
				return 0;
		}

	}
}

