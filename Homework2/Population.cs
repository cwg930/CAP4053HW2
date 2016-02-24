using System;
using System.Collections.Generic;

namespace Homework2
{

	public class Population
	{ 
		const int numEliteCopies = 1;
		const int numElite = 2;
		List<Genome> genomes;
		int chromosomeLength;
		double totalFitness;
		double bestFitness;
		double avgFitness;
		double worstFitness;
		int fittestGenome;
		static Random r = new Random();

		public List<Genome> Genomes { get { return genomes; } }

		public Population (int popSize, int numWeights)
		{
			this.chromosomeLength = numWeights;
			this.totalFitness = 0;
			this.bestFitness = 0;
			this.avgFitness = 0;
			this.worstFitness = double.MaxValue;
			genomes = new List<Genome> ();
			for (int i = 0; i < popSize; i++) {
				genomes.Add (new Genome ());
				for (int j = 0; j < chromosomeLength; j++) {
					genomes [i].Weights.Add (r.NextDouble ());
				}
			}
		}
			
		public Population(int numWeights){
			this.chromosomeLength = numWeights;
			this.chromosomeLength = numWeights;
			this.totalFitness = 0;
			this.bestFitness = 0;
			this.avgFitness = 0;
			this.worstFitness = double.MaxValue;
			genomes = new List<Genome> ();
		}

		private Genome GetChromosomeRoulette()
		{
			/*double slice = r.NextDouble () * totalFitness;

			Genome chosen = null;
			double currentFitness = 0;
			for (int i = 0; i < genomes.Count; i++) {
				currentFitness = genomes [i].Fitness;
				if (currentFitness >= slice) {
					chosen = genomes [i];
					break;
				}
			}
			return chosen;*/
			int chosen = r.Next (0, genomes.Count);
			return genomes[chosen];


		}

		private void FitnessScaleRank()
		{
			int multiplier = 1;
			for (int i = 0; i < genomes.Count; i++) {
				genomes [i].Fitness = i * multiplier;
			}
			CalcFitnessStats ();
		}

		private void CalcFitnessStats()
		{
			totalFitness = 0;
			double highest = 0;
			double lowest = double.MaxValue;

			for (int i = 0; i < genomes.Count; i++) {
				if (genomes [i].Fitness > highest) {
					highest = genomes [i].Fitness;
					fittestGenome = i;
					bestFitness = highest;
				}

				if (genomes [i].Fitness < lowest) {
					lowest = genomes [i].Fitness;
					worstFitness = lowest;
				}

				totalFitness += genomes [i].Fitness;
			}
			avgFitness = totalFitness / genomes.Count;
		}



		private void Reset()
		{
			totalFitness = 0;
			bestFitness = 0;
			worstFitness = double.MaxValue;
			avgFitness = 0;
		}
			


		public Population Epoch()
		{
			Reset ();

			genomes.Sort ();
			CalcFitnessStats ();

			Population nextGeneration = new Population (this.chromosomeLength);
			if ((numEliteCopies * numElite % 2) == 0) { 
				for (int i = numElite; i > 0; i--) {
					nextGeneration.genomes.Add(genomes[(genomes.Count - 1) - i]);
				}
			}

			while (nextGeneration.genomes.Count < genomes.Count) {
				Genome mom = GetChromosomeRoulette ();
				Genome dad = GetChromosomeRoulette ();

				Tuple<Genome,Genome> children = Genome.Crossover (mom, dad);
				children.Item1.Mutate ();
				children.Item2.Mutate ();

				nextGeneration.genomes.Add (children.Item1);
				nextGeneration.genomes.Add (children.Item2);
			}
			return nextGeneration;
		}



	}
}

