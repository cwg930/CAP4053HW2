using System;
using System.Collections.Generic;

namespace Homework2
{
	struct Genome
	{
		public List<double> Weights { get; set; }
		public double fitness;

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

	public class GenAlg
	{ 
		const double maxPerturbation = 0.3;
		const int numEliteCopies = 1;
		const int numElite = 4;
		int populationSize;
		public List<Genome> Population { get; }
		int chromosomeLength;
		double totalFitness;
		double bestFitness { get; }
		double avgFitness;
		double worstFitness;
		int fittestGenome;
		double mutationRate;
		double crossoverRate;
		int generationCount;
		Random r;

		public GenAlg (int popSize, double mutRate, double crossRate, int numWeights)
		{
			this.mutationRate = mutRate;
			this.crossoverRate = crossRate;
			this.chromosomeLength = numWeights;
			this.totalFitness = 0;
			this.bestFitness = 0;
			this.avgFitness = 0;
			this.worstFitness = double.MaxValue;
			this.generationCount = 0;
			this.fittestGenome = 0;
			this.populationSize = popSize;
			r = new Random ();
			for (int i = 0; i < popSize; i++) {
				Population.Add (new Genome ());
				for (int j = 0; j < chromosomeLength; j++) {
					Population [i].Weights.Add (r.NextDouble ());
				}
			}
		}

		private Tuple<List<double>> Crossover(List<double> mom, List<double> dad)
		{
			Tuple<List<double>, List<double>> children = new Tuple<> ();
			if (r.NextDouble () > crossoverRate || mom == dad) {
				children.Item1 = mom;
				children.Item2 = dad;
				return children;
			}

			int crossPoint = r.Next (0, chromosomeLength - 1);

			for (int i = 0; i < crossPoint; i++) {
				children.Item1.Add (mom [i]);
				children.Item2.Add (dad [i]);
			}
			for (int i = crossPoint; i < mom.Count; i++) {
				children.Item1.Add (dad [i]);
				children.Item2.Add (mom [i]);
			}
			return children;
		}

		private void Mutate(List<double> chromosome)
		{
			for (int i = 0; i < chromosome.Count; i++) {
				if (r.NextDouble () < mutationRate) {
					chromosome [i] += (r.NextDouble () * 2 - 1) * maxPerturbation;
				}	
			}
			}

		private Genome GetChromosomeRoulette()
		{
			double slice = r.NextDouble () * totalFitness;

			Genome chosen;
			double currentFitness = 0;
			for (int i = 0; i < populationSize; i++) {
				currentFitness = Population [i].fitness;
				if (currentFitness >= slice) {
					chosen = Population [i];
					break;
				}
			}
			return chosen;
		}

		private void FitnessScaleRank()
		{
			int multiplier = 1;
			for (int i = 0; i < populationSize; i++) {
				Population [i].fitness = i * multiplier;
			}
			CalcFitnessStats ();
		}

		private void CalcFitnessStats()
		{
			totalFitness = 0;
			double highest = 0;
			double lowest = double.MaxValue;

			for (int i = 0; i < populationSize; i++) {
				if (Population [i].fitness > highest) {
					highest = Population [i].fitness;
					fittestGenome = i;
					bestFitness = highest;
				}

				if (Population [i].fitness < lowest) {
					lowest = Population [i].fitness;
					worstFitness = lowest;
				}

				totalFitness += Population [i].fitness;
			}
			avgFitness = totalFitness / populationSize;
		}

		private void Reset()
		{
			totalFitness = 0;
			bestFitness = 0;
			worstFitness = double.MaxValue;
			avgFitness = 0;
		}

		public List<Genome> Epoch(List<Genome> oldPopulation)
		{
			Population = oldPopulation;
			Reset ();

			Population.Sort ();
			CalcFitnessStats ();

			List<Genome> newPopulation;

			if (!(numEliteCopies * numElite % 2)) { 
				for (int i = numElite; i > 0; i--) {
					newPopulation.Add(Population[(populationSize - 1) - i]);
				}
			}

			while (newPopulation.Count < populationSize) {
				Genome mom = GetChromosomeRoulette ();
				Genome dad = GetChromosomeRoulette ();

				Tuple<List<double>,List<double>> children = new Tuple<List<double>, List<double>> ();
				children = Crossover (mom, dad);
				Mutate (children.Item1);
				Mutate (children.Item2);

				newPopulation.Add (children.Item1);
				newPopulation.Add (children.Item2);
			}
		}


	}
}

